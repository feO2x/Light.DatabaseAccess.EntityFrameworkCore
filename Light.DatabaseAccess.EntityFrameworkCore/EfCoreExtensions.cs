using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Light.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Light.DatabaseAccess.EntityFrameworkCore;

/// <summary>
/// Contains extension methods for Entity Framework Core.
/// </summary>
public static class EfCoreExtensions
{
    /// <summary>
    /// Creates a new DB command of the specified type and optionally sets the SQL command text.
    /// If the underlying DB connection is not open, it will be opened asynchronously before creating the command.
    /// If the connection is already open and a transaction is active, the transaction will be attached to the command.
    /// </summary>
    /// <param name="dbContext">The DB context managing the underlying DB connection.</param>
    /// <param name="sql">
    /// The SQL statement that will be set as the command text on the created command (optional).
    /// </param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <typeparam name="TDbCommand">
    /// The subtype of the DB command you want to create. This type must correspond to the ADO.NET provider that is
    /// configured with the DB context.
    /// </typeparam>
    /// <returns>The DB command cast to the subtype.</returns>
    public static ValueTask<TDbCommand> CreateCommandAsync<TDbCommand>(
        this DbContext dbContext,
        string? sql = null,
        CancellationToken cancellationToken = default
    )
        where TDbCommand : DbCommand
    {
        var dbConnection = dbContext.Database.GetDbConnection();
        
        return dbConnection.State == ConnectionState.Open ?
            new ValueTask<TDbCommand>(CreateCommandAndTryAttachTransaction<TDbCommand>(dbContext, dbConnection, sql)) :
            InitializeConnectionAndCreateCommandAsync<TDbCommand>(dbContext, sql, cancellationToken);
    }
    
    private static async ValueTask<TDbCommand> InitializeConnectionAndCreateCommandAsync<TDbCommand>(
        DbContext dbContext,
        string? sql,
        CancellationToken cancellationToken
    )
            where TDbCommand : DbCommand
    {
        // The connection is not open here, so we try to open it
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        
        // Implementations of EF Core's IRelationalConnection interface might instantiate a new DB connection object,
        // so we call GetDbConnection again to get the most current connection.
        var dbCommand = (TDbCommand) dbContext.Database.GetDbConnection().CreateCommand();
        
        if (!sql.IsNullOrWhiteSpace())
        {
            dbCommand.CommandText = sql;
        }
        
        // As the connection was not open at the beginning of this method, there cannot be a transaction.
        // We will simply return the DbCommand.
        return dbCommand;
    }

    private static TDbCommand CreateCommandAndTryAttachTransaction<TDbCommand>(DbContext dbContext, DbConnection dbConnection, string? sql)
        where TDbCommand : DbCommand
    {
        // When we hit this method, we know that the connection is open.
        var dbCommand = (TDbCommand) dbConnection.CreateCommand();
        
        if (!sql.IsNullOrWhiteSpace())
        {
            dbCommand.CommandText = sql;
        }
        
        if (dbContext.TryGetCurrentTransaction(out var transaction))
        {
            dbCommand.Transaction = transaction;
        }

        return dbCommand;
    }

    private static bool TryGetCurrentTransaction(
        this DbContext dbContext,
        [NotNullWhen(true)] out DbTransaction? transaction
    )
    {
        if (dbContext
               .MustNotBeNull()
               .Database
               .CurrentTransaction is IInfrastructure<DbTransaction> dbTransactionProvider)
        {
            transaction = dbTransactionProvider.Instance;
            return true;
        }

        transaction = null;
        return false;
    }
}