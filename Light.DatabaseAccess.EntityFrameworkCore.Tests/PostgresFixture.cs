using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder("postgres:15.1")
           .WithDatabase("ContactsDb")
           .WithUsername("foo")
           .WithPassword("bar")
           .Build();

    public string ConnectionString => _postgresContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        var dbContextOptions = new DbContextOptionsBuilder<MyDbContext>()
           .UseNpgsql(ConnectionString)
           .Options;
        await using var dbContext = new MyDbContext(dbContextOptions);
        await dbContext.Database.MigrateAsync();
        dbContext.Contacts.AddRange(AllContacts.DefaultContacts);
        await dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => _postgresContainer.DisposeAsync().AsTask();
}
