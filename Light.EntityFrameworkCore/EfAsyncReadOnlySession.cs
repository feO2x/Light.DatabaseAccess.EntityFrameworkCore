using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;
using Light.SharedCore.DatabaseAccessAbstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Light.EntityFrameworkCore;

/// <summary>
/// <para>
/// Represents a session that allows read-only access to the database.
/// No dedicated transaction is used for this session - if you require it, derive from the
/// <see cref="EfAsyncReadOnlySession{TDbContext}.WithTransaction" /> class instead.
/// If you want to manipulate the database, derive from <see cref="EfAsyncSession{TDbContext}" /> instead.
/// </para>
/// <para>
/// WARNING: do not register your derived session as a singleton or transient service with your DI container.
/// The bases classes are not implemented in a thread-safe manner and are not designed to be used as singletons.
/// Furthermore, Microsoft's DI container has issues with transient services that implement
/// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext" /> type you derived for your app.</typeparam>
public abstract class EfAsyncReadOnlySession<TDbContext> : IAsyncReadOnlySession
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="EfAsyncReadOnlySession{TDbContext}" />
    /// </summary>
    /// <param name="dbContext">The DB context used to access the database.</param>
    /// <param name="queryTrackingBehavior">
    /// The value indicating how the results of a query are tracked by the change tracker of the DB context.
    /// The default value is <see cref="QueryTrackingBehavior.NoTrackingWithIdentityResolution" />.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
    protected EfAsyncReadOnlySession(
        TDbContext dbContext,
        QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution
    )
    {
        DbContext = dbContext.MustNotBeNull();
        dbContext.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
    }

    /// <summary>
    /// Gets the DB context used to access the database.
    /// </summary>
    public TDbContext DbContext { get; }

    /// <summary>
    /// Disposes the DB context.
    /// </summary>
    public void Dispose() => DbContext.Dispose();

    /// <summary>
    /// Disposes the DB context.
    /// </summary>
    public ValueTask DisposeAsync() => DbContext.DisposeAsync();

    /// <summary>
    /// <para>
    /// Represents a session that allows read-only access to the database and uses a dedicated transaction.
    /// This can be useful when you need a dedicated isolation level like READ UNCOMMITTED or REPEATABLE READ for
    /// your queries. If you want to manipulate data, derive from
    /// <see cref="EfAsyncSession{TDbContext}.WithTransaction" /> instead.
    /// </para>
    /// <para>
    /// WARNING: do not register your derived session as a singleton or transient service with your DI container.
    /// The bases classes are not implemented in a thread-safe manner and are not designed to be used as singletons.
    /// Furthermore, Microsoft's DI container has issues with transient services that implement
    /// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
    /// </para>
    /// </summary>
    public abstract class WithTransaction : IAsyncReadOnlySession
    {
        private readonly TDbContext _dbContext;
        private readonly IsolationLevel _isolationLevel;
        private IDbContextTransaction? _transaction;

        /// <summary>
        /// Initializes a new instance of <see cref="EfAsyncReadOnlySession{TDbContext}.WithTransaction" />.
        /// </summary>
        /// <param name="dbContext">The DB context used to access the database.</param>
        /// <param name="isolationLevel">
        /// The isolation level that is used for the underlying transaction.
        /// The default value is <see cref="IsolationLevel.ReadCommitted" />.
        /// </param>
        /// <param name="queryTrackingBehavior">
        /// The value indicating how the results of a query are tracked by the change tracker of the DB context.
        /// The default value is <see cref="QueryTrackingBehavior.NoTrackingWithIdentityResolution" />.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
        protected WithTransaction(
            TDbContext dbContext,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution
        )
        {
            _dbContext = dbContext.MustNotBeNull();
            dbContext.ChangeTracker.QueryTrackingBehavior = queryTrackingBehavior;
            _isolationLevel = isolationLevel;
        }

        /// <summary>
        /// Disposes the underlying transaction and the DB context.
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _dbContext.Dispose();
        }

        /// <summary>
        /// Disposes the underlying transaction and the DB context.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
            }

            await _dbContext.DisposeAsync();
        }

        /// <summary>
        /// Gets the DB context used to access the database.
        /// </summary>
        /// <param name="cancellationToken">An optional token to cancel the operation.</param>
        [MemberNotNull(nameof(_transaction))]
        public ValueTask<TDbContext> GetDbContextAsync(CancellationToken cancellationToken = default) =>
            _transaction is not null ?
                new ValueTask<TDbContext>(_dbContext) :
                InitializeAndGetDbContextAsync(cancellationToken);

        [MemberNotNull(nameof(_transaction))]
        private async ValueTask<TDbContext> InitializeAndGetDbContextAsync(CancellationToken cancellationToken)
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync(_isolationLevel, cancellationToken);
            return _dbContext;
        }

        /// <summary>
        /// Saves the changes in the DB context and commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">An optional token to cancel the operation.</param>
        protected async Task SaveChangesAndCommitAsync(CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
    }
}