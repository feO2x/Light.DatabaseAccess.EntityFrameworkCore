using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Npgsql;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.GetAllContacts;

public sealed class AdoNetGetAllContactsSession : EfClient<MyDbContext>, IGetAllContactsSession
{
    public AdoNetGetAllContactsSession(MyDbContext dbContext) : base(dbContext) { }

    public async Task<List<Contact>> GetContactsAsync(CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync<NpgsqlCommand>(
            "SELECT \"Id\", \"FirstName\", \"LastName\", \"Email\" FROM \"Contacts\";",
            cancellationToken
        );

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);
        return await DeserializeContactsAsync(reader, cancellationToken);
    }

    private static async Task<List<Contact>> DeserializeContactsAsync(
        NpgsqlDataReader reader,
        CancellationToken cancellationToken
    )
    {
        var entities = new List<Contact>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var entity = new Contact
            {
                Id = reader.GetGuid(0),
                FirstName = reader.GetString(1),
                LastName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Email = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
            entities.Add(entity);
        }

        return entities;
    }
}