using System.Threading.Tasks;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private const int Port = 5432;
    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder()
           .WithDatabase("ContactsDb")
           .WithUsername("foo")
           .WithPassword("bar")
           .WithPortBinding(Port)
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