using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Npgsql;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.DeleteAllContacts;

public sealed class AdoNetDeleteAllContactsSession : EfSession<MyDbContext>.WithTransaction,
                                                     IDeleteAllContactsSession
{
    public AdoNetDeleteAllContactsSession(MyDbContext dbContext) : base(dbContext, IsolationLevel.Serializable) { }

    public async Task DeleteAllContactsAsync(CancellationToken cancellationToken = default)
    {
        await using var command =
            await CreateCommandAsync<NpgsqlCommand>("DELETE FROM \"Contacts\";", cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}