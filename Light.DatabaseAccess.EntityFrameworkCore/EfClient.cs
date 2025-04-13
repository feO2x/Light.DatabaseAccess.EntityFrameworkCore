using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Light.DatabaseAccess.EntityFrameworkCore;

/// <summary>
/// <para>
/// Represents a base class for humble objects connecting to a database via an Entity Framework <see cref="DbContext"/>.
/// This base class should be applied in use cases where you only read data from the database or in rare scenarios
/// where no transaction must be committed (usually because implicit transactions are used). For this reason,
/// <see cref="QueryTrackingBehavior.NoTrackingWithIdentityResolution" /> is set on the DB context by default. Use the
/// corresponding constructor parameter to change this behavior.
/// </para>
/// <para>
/// No dedicated transaction is used for this session - if you require it, derive from the
/// <see cref="EfClient{TDbContext}.WithTransaction" /> class instead.
/// If you want to manipulate data, derive from <see cref="EfSession{TDbContext}" /> instead.
/// </para>
/// <para>
/// WARNING: you should not register your derived classes with your DI container using a singleton or transient lifetime.
/// This base class is not implemented in a thread-safe manner and is not designed to be used as a singleton.
/// Furthermore, Microsoft's DI container has issues with transient services that implement
/// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext" /> type you derived for your app.</typeparam>
public abstract class EfClient<TDbContext> : IAsyncDisposable, IDisposable
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="EfClient{TDbContext}" />
    /// </summary>
    /// <param name="dbContext">The DB context used to access the database.</param>
    /// <param name="queryTrackingBehavior">
    /// The value indicating how the results of a query are tracked by the change tracker of the DB context.
    /// The default value is <see cref="QueryTrackingBehavior.NoTrackingWithIdentityResolution" />.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
    protected EfClient(
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
    // ReSharper disable once MemberCanBeProtected.Global
    public TDbContext DbContext { get; }

    /// <summary>
    /// Disposes the DB context.
    /// </summary>
    //  Dispose and DisposeAsync can be overridden by reimplementing the interfaces in subclasses
#pragma warning disable CA1816
    public void Dispose() => DbContext.Dispose();

    /// <summary>
    /// Disposes the DB context.
    /// </summary>
    public ValueTask DisposeAsync() => DbContext.DisposeAsync();
#pragma warning restore CA1816

    /// <summary>
    /// Creates a new DB command of the specified type and optionally sets the SQL command text.
    /// If the underlying DB connection is not open, it will be opened asynchronously before creating the command.
    /// If the connection is already open and a transaction is active, the transaction will be attached to the command.
    /// </summary>
    /// <param name="sql">
    /// The SQL statement that will be set as the command text on the created command (optional).
    /// </param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <typeparam name="TDbCommand">
    /// The subtype of the DB command you want to create. This type must correspond to the ADO.NET provider that is
    /// configured with the DB context.
    /// </typeparam>
    /// <returns>
    /// The DB command cast to the subtype.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown when the created command cannot be cast to <typeparamref name="TDbCommand"/>.
    /// </exception>
    // ReSharper disable once VirtualMemberNeverOverridden.Global -- can be overridden by library users
    // ReSharper disable once MemberCanBeProtected.Global
    public virtual ValueTask<TDbCommand> CreateCommandAsync<TDbCommand>(
        string? sql = null,
        CancellationToken cancellationToken = default
    )
        where TDbCommand : DbCommand =>
        DbContext.CreateCommandAsync<TDbCommand>(sql, cancellationToken);

    /// <summary>
    /// <para>
    /// Represents a base class for humble objects connecting to a database via an Entity Framework <see cref="DbContext"/>.
    /// This base class should be applied in use cases where you only read data from the database or in rare scenarios
    /// where no transaction must be committed (usually because implicit transactions are used). For this reason,
    /// <see cref="QueryTrackingBehavior.NoTrackingWithIdentityResolution" /> is set on the DB context by default. Use the
    /// corresponding constructor parameter to change this behavior.
    /// </para>
    /// <para>
    /// This client uses a dedicated transaction internally.
    /// This can be useful when you need a dedicated isolation level like READ UNCOMMITTED or REPEATABLE READ for
    /// your queries. If you want to manipulate data, derive from <see cref="EfSession{TDbContext}.WithTransaction" />
    /// instead.
    /// </para>
    /// <para>
    /// WARNING: you should not register your derived classes with your DI container using a singleton or transient lifetime.
    /// This base class is not implemented in a thread-safe manner and is not designed to be used as a singleton.
    /// Furthermore, Microsoft's DI container has issues with transient services that implement
    /// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
    /// </para>
    /// </summary>
    public abstract class WithTransaction : IAsyncDisposable, IDisposable
    {
        private readonly TDbContext _dbContext;
        private readonly IsolationLevel _isolationLevel;
        private IDbContextTransaction? _transaction;

        /// <summary>
        /// Initializes a new instance of <see cref="EfClient{TDbContext}.WithTransaction" />.
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
        /// <para>
        /// Gets the DB context used to access the database. Initializes the transaction if it is not
        /// already initialized.
        /// </para>
        /// <para>
        /// PLEASE NOTE: there is an asynchronous method <see cref="GetDbContextAsync" /> which initializes
        /// the transaction asynchronously. However, ADO.NET providers like Npgsql simply indicate
        /// to the next command being executed that there should be a BEGIN TRANSACTION statement at the beginning
        /// of the SQL script, which is why it is usually OK to use this property synchronously.
        /// Also, you can use this property when you are sure that the transaction is already initialized.
        /// </para>
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        [MemberNotNull(nameof(_transaction))]
        public TDbContext DbContext
        {
            get
            {
                _transaction ??= _dbContext.Database.BeginTransaction();
                return _dbContext;
            }
        }

        /// <summary>
        /// Disposes the underlying transaction and the DB context.
        /// </summary>
        // Dispose and DisposeAsync can be overridden by reimplementing the interfaces in subclasses
#pragma warning disable CA1816
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
#pragma warning restore CA1816


        /// <summary>
        /// Gets the DB context used to access the database. Initializes the transaction if it is not
        /// already initialized.
        /// </summary>
        /// <param name="cancellationToken">An optional token to cancel the operation.</param>
        [MemberNotNull(nameof(_transaction))]
        // ReSharper disable once MemberCanBeProtected.Global
        public ValueTask<TDbContext> GetDbContextAsync(CancellationToken cancellationToken = default) =>
            _transaction is not null ?
                new ValueTask<TDbContext>(_dbContext) :
                InitializeAndGetDbContextAsync(cancellationToken);

        [MemberNotNull(nameof(_transaction))]
        private async ValueTask<TDbContext> InitializeAndGetDbContextAsync(CancellationToken cancellationToken)
        {
#pragma warning disable 8774 // the warning is incorrect when the caller awaits the GetDbContextAsync method
            _transaction = await _dbContext.Database.BeginTransactionAsync(_isolationLevel, cancellationToken);
            return _dbContext;
#pragma warning restore 8774
        }

        /// <summary>
        /// Creates a new DB command of the specified type and optionally sets the SQL command text.
        /// If the underlying connection is not open and/or the underlying transaction is not started yet, they will be
        /// initialized. Both of them will be attached to the command.
        /// </summary>
        /// <param name="sql">
        /// The SQL statement that will be set as the command text on the created command (optional).
        /// </param>
        /// <param name="cancellationToken">An optional token to cancel the operation.</param>
        /// <typeparam name="TDbCommand">
        /// The subtype of the DB command you want to create. This type must correspond to the ADO.NET provider that is
        /// configured with the DB context.
        /// </typeparam>
        /// <returns>The DB command cast to the subtype.</returns>
        // ReSharper disable once VirtualMemberNeverOverridden.Global -- can be overridden by library users
        // ReSharper disable once MemberCanBeProtected.Global
        public virtual ValueTask<TDbCommand> CreateCommandAsync<TDbCommand>(
            string? sql = null,
            CancellationToken cancellationToken = default
        )
            where TDbCommand : DbCommand =>
            _transaction is not null ?
                _dbContext.CreateCommandAsync<TDbCommand>(sql, cancellationToken) :
                InitializeTransactionAndCreateCommandAsync<TDbCommand>(sql, cancellationToken);

        private async ValueTask<TDbCommand> InitializeTransactionAndCreateCommandAsync<TDbCommand>(
            string? sql,
            CancellationToken cancellationToken
        )
            where TDbCommand : DbCommand
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            return await _dbContext.CreateCommandAsync<TDbCommand>(sql, cancellationToken);
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