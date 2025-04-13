using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.SharedCore.DatabaseAccessAbstractions;
using Microsoft.EntityFrameworkCore;

namespace Light.DatabaseAccess.EntityFrameworkCore;

/// <summary>
/// <para>
/// Represents a base class for humble objects connecting to a database via an Entity Framework <see cref="DbContext"/>.
/// This session should be used in use cases where you need to manipulate data - commit the changes by calling the
/// session's <see cref="SaveChangesAsync"/> method.
/// </para>
/// <para>
/// No dedicated transaction is used for this session - if you require it, derive from the
/// <see cref="EfSession{TDbContext}.WithTransaction" /> class instead.
/// If you only need to read data in your use case, consider deriving from <see cref="EfClient{TDbContext}" /> instead.
/// </para>
/// <para>
/// WARNING: you should not register your derived classes with your DI container using a singleton or transient lifetime.
/// This base class is not implemented in a thread-safe manner and is not designed to be used as a singleton.
/// Furthermore, Microsoft's DI container has issues with transient services that implement
/// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
/// </para>
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext" /> type you derived for your app.</typeparam>
public abstract class EfSession<TDbContext> : EfClient<TDbContext>, ISession
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="EfSession{TDbContext}" />.
    /// </summary>
    /// <param name="dbContext">The DB context used to access the database.</param>
    /// <param name="queryTrackingBehavior">
    /// The value indicating how the results of a query are tracked by the change tracker of the DB context.
    /// The default value is <see cref="QueryTrackingBehavior.TrackAll" />.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is null.</exception>
    protected EfSession(
        TDbContext dbContext,
        QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll
    )
        : base(dbContext, queryTrackingBehavior) { }

    /// <summary>
    /// Saves the changes in the DB context.
    /// </summary>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        DbContext.SaveChangesAsync(cancellationToken);

    /// <summary>
    /// <para>
    /// Represents a base class for humble objects connecting to a database via an Entity Framework <see cref="DbContext"/>.
    /// This session should be used in use cases where you need to manipulate data - commit the changes by calling the
    /// session's <see cref="SaveChangesAsync"/> method.
    /// </para>
    /// <para>
    /// A dedicated transaction is used for this session - if you do not require it, derive from the
    /// <see cref="EfSession{TDbContext}" /> class instead.
    /// If you only need to read data in your use case, consider deriving from <see cref="EfClient{TDbContext}.WithTransaction" /> instead.
    /// </para>
    /// <para>
    /// </para>
    /// WARNING: you should not register your derived classes with your DI container using a singleton or transient lifetime.
    /// This base class is not implemented in a thread-safe manner and is not designed to be used as a singleton.
    /// Furthermore, Microsoft's DI container has issues with transient services that implement
    /// <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
    /// </summary>
    public new abstract class WithTransaction : EfClient<TDbContext>.WithTransaction, ISession
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EfSession{TDbContext}.WithTransaction" />.
        /// </summary>
        /// <param name="dbContext">The DB context used to access the database.</param>
        /// <param name="isolationLevel">
        /// The isolation level that is used for the underlying transaction.
        /// The default value is <see cref="IsolationLevel.ReadCommitted" />.
        /// </param>
        /// <param name="queryTrackingBehavior">
        /// The value indicating how the results of a query are tracked by the change tracker of the DB context.
        /// The default value is <see cref="QueryTrackingBehavior.TrackAll" />.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContext" /> is nulls.</exception>
        protected WithTransaction(
            TDbContext dbContext,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll
        )
            : base(dbContext, isolationLevel, queryTrackingBehavior) { }

        /// <summary>
        /// Saves the changes in the DB context and commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">An optional token to cancel the operation.</param>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            SaveChangesAndCommitAsync(cancellationToken);
    }
}