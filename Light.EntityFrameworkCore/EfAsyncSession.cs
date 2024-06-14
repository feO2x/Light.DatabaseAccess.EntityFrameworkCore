using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.SharedCore.DatabaseAccessAbstractions;
using Microsoft.EntityFrameworkCore;

namespace Light.EntityFrameworkCore;

/// <summary>
/// <para>
/// Represents a database session that allows to manipulate data.
/// No dedicated transaction is used for this session - if you require it, derive from the
/// <see cref="EfAsyncSession{TDbContext}.WithTransaction" /> class instead.
/// If you only need to read data, derive from <see cref="EfAsyncReadOnlySession{TDbContext}" /> instead.
/// </para>
/// <para>
/// WARNING: do not register your derived session as a singleton or transient service with your DI container.
/// The bases classes are not implemented in a thread-safe manner and are not designed to be used as singletons.
/// Furthermore, Microsoft's DI container has issues with transient services that implement
/// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext" /> type you derived for your app.</typeparam>
public abstract class EfAsyncSession<TDbContext> : EfAsyncReadOnlySession<TDbContext>, IAsyncSession
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="EfAsyncSession{TDbContext}" />.
    /// </summary>
    /// <param name="dbContext">The DB context used to access the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
    protected EfAsyncSession(TDbContext dbContext) : base(dbContext) { }

    /// <summary>
    /// Saves the changes in the DB context.
    /// </summary>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        DbContext.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// <para>
    /// Represents a database session that allows to manipulate data and uses a dedicated transaction.
    /// This is useful in pessimistic concurrency scenarios or when you need a dedicated isolation level like
    /// READ UNCOMMITED for your queries.
    /// If you only need to read data, derive from
    /// <see cref="EfAsyncReadOnlySession{TDbContext}.WithTransaction" /> instead.
    /// </para>
    /// <para>
    /// </para>
    /// WARNING: do not register your derived session as a singleton or transient service with your DI container.
    /// The bases classes are not implemented in a thread-safe manner and are not designed to be used as singletons.
    /// Furthermore, Microsoft's DI container has issues with transient services that implement
    /// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
    /// </summary>
    public new abstract class WithTransaction : EfAsyncReadOnlySession<TDbContext>.WithTransaction, IAsyncSession
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EfAsyncSession{TDbContext}.WithTransaction" />.
        /// </summary>
        /// <param name="dbContext">The DB context used to access the database.</param>
        /// <param name="isolationLevel">
        /// The isolation level that is used for the underlying transaction.
        /// The default value is <see cref="IsolationLevel.ReadCommitted" />.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
        protected WithTransaction(TDbContext dbContext, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
            : base(dbContext, isolationLevel) { }

        /// <summary>
        /// Saves the changes in the DB context and commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">An optional token to cancel the operation.</param>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            SaveChangesAndCommitAsync(cancellationToken);
    }
}