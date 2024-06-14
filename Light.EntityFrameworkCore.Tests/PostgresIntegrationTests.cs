using System.Threading.Tasks;
using FluentAssertions;
using Light.EntityFrameworkCore.Tests.Contacts.DeleteAllContacts;
using Light.EntityFrameworkCore.Tests.Contacts.GetAllContacts;
using Light.EntityFrameworkCore.Tests.Contacts.ManipulateContacts;
using Light.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Light.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Light.EntityFrameworkCore.Tests;

[TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
public sealed class PostgresIntegrationTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private const string ReadUncommittedKey = "ReadUncommitted";
    private readonly AsyncServiceScope _scope;
    private readonly ServiceProvider _serviceProvider;

    public PostgresIntegrationTests(PostgresFixture postgresFixture)
    {
        _serviceProvider = new ServiceCollection()
           .AddDbContext<MyDbContext>(options => options.UseNpgsql(postgresFixture.ConnectionString))
           .AddScoped<IGetAllContactsSession, EfGetAllContactsSession>()
           .AddScoped<GetAllContactsService>()
           .AddScoped<IManipulateContactsSession, EfManipulateContactsSession>()
           .AddScoped<ManipulateContactsService>()
           .AddKeyedScoped<IGetAllContactsSession, EfGetAllContactsReadUncommittedSession>(ReadUncommittedKey)
           .AddKeyedScoped<GetAllContactsService>(
                ReadUncommittedKey,
                (sp, key) => new GetAllContactsService(sp.GetRequiredKeyedService<IGetAllContactsSession>(key))
            )
           .AddScoped<IDeleteAllContactsSession, EfDeleteAlLContactsSession>()
           .AddScoped<DeleteAllContactsService>()
           .BuildServiceProvider();
        _scope = _serviceProvider.CreateAsyncScope();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    [Fact]
    public async Task GetAllContacts()
    {
        var getAllContactsService = _scope.ServiceProvider.GetRequiredService<GetAllContactsService>();

        var contacts = await getAllContactsService.GetAllContactsAsync();

        contacts.Should().BeEquivalentTo(Contact.DefaultContacts);
    }

    [Fact]
    public async Task ManipulateContacts()
    {
        var manipulateContactsService = _scope.ServiceProvider.GetRequiredService<ManipulateContactsService>();

        await manipulateContactsService.ManipulateContactsAsync();

        var manipulatedContacts =
            await _scope.ServiceProvider.GetRequiredService<GetAllContactsService>().GetAllContactsAsync();
        var expectedContacts = ManipulateContactsService.GetExpectedManipulatedContacts();
        manipulatedContacts.Should().BeEquivalentTo(expectedContacts);
    }

    [Fact]
    public async Task GetAllWithReadUncommittedTransaction()
    {
        var getAllContactsService =
            _scope.ServiceProvider.GetRequiredKeyedService<GetAllContactsService>(ReadUncommittedKey);

        var contacts = await getAllContactsService.GetAllContactsAsync();

        var expectedContacts = ManipulateContactsService.GetExpectedManipulatedContacts();
        contacts.Should().BeEquivalentTo(expectedContacts);
    }

    [Fact]
    public async Task DeleteAllContacts()
    {
        var deleteAllContactsService = _scope.ServiceProvider.GetRequiredService<DeleteAllContactsService>();

        await deleteAllContactsService.DeleteAllContactsAsync();
        
        var contacts = await _scope.ServiceProvider.GetRequiredService<GetAllContactsService>().GetAllContactsAsync();
        contacts.Should().BeEmpty();
    }
}