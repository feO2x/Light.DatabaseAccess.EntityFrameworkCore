﻿using System.Threading.Tasks;
using FluentAssertions;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.AddContact;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.DeleteAllContacts;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.GetAllContacts;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.Contacts.ManipulateContacts;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Light.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests;

[TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
public sealed class PostgresIntegrationTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private const string ReadUncommittedKey = "ReadUncommitted";
    private const string AdoNetKey = "AdoNet";
    private readonly AsyncServiceScope _scope;
    private readonly ServiceProvider _serviceProvider;

    public PostgresIntegrationTests(PostgresFixture postgresFixture)
    {
        _serviceProvider = new ServiceCollection()
                          .AddDbContext<MyDbContext>(options => options.UseNpgsql(postgresFixture.ConnectionString))
                          .AddScoped<IGetAllContactsSession, EfGetAllContactsSession>()
                          .AddScoped<GetAllContactsService>()
                          .AddScoped<IAddContactSession, EfAddContactSession>()
                          .AddScoped<AddContactService>()
                          .AddScoped<IManipulateContactsSession, EfManipulateContactsSession>()
                          .AddScoped<ManipulateContactsService>()
                          .AddKeyedScoped<IGetAllContactsSession, EfGetAllContactsReadUncommittedSession>(
                               ReadUncommittedKey)
                          .AddKeyedScoped<GetAllContactsService>(
                               ReadUncommittedKey,
                               (sp, key) => new (sp.GetRequiredKeyedService<IGetAllContactsSession>(key))
                           )
                          .AddKeyedScoped<IGetAllContactsSession, AdoNetGetAllContactsSession>(AdoNetKey)
                          .AddKeyedScoped<GetAllContactsService>(
                               AdoNetKey,
                               (sp, key) => new (sp.GetRequiredKeyedService<IGetAllContactsSession>(key))
                           )
                          .AddScoped<IDeleteAllContactsSession, EfDeleteAllContactsSession>()
                          .AddScoped<DeleteAllContactsService>()
                          .AddKeyedScoped<IDeleteAllContactsSession, AdoNetDeleteAllContactsSession>(AdoNetKey)
                          .AddKeyedScoped(
                               AdoNetKey,
                               (sp, key) =>
                                   new DeleteAllContactsService(
                                       sp.GetRequiredKeyedService<IDeleteAllContactsSession>(key))
                           )
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

        contacts.Should().BeEquivalentTo(AllContacts.DefaultContacts);
    }

    [Fact]
    public async Task GetAllContactsViaCommand()
    {
        var getAllContactsService = _scope.ServiceProvider.GetRequiredKeyedService<GetAllContactsService>(AdoNetKey);

        var contacts = await getAllContactsService.GetAllContactsAsync();

        contacts.Should().BeEquivalentTo(AllContacts.DefaultContacts);
    }

    [Fact]
    public async Task GetAllContactsViaCommandWithoutTransaction()
    {
        var dbContext = _scope.ServiceProvider.GetRequiredService<MyDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        var getAllContactsService = _scope.ServiceProvider.GetRequiredKeyedService<GetAllContactsService>(AdoNetKey);

        var contacts = await getAllContactsService.GetAllContactsAsync();

        contacts.Should().BeEquivalentTo(AllContacts.DefaultContacts);
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
    public async Task AddContact()
    {
        var addContactService = _scope.ServiceProvider.GetRequiredService<AddContactService>();

        await addContactService.AddContactAsync(AllContacts.NewContact);

        var allContacts =
            await _scope.ServiceProvider.GetRequiredService<GetAllContactsService>().GetAllContactsAsync();
        var expectedContacts = ManipulateContactsService.GetExpectedManipulatedContacts();
        expectedContacts.Add(AllContacts.NewContact);
        allContacts.Should().BeEquivalentTo(expectedContacts);
    }

    [Fact]
    public async Task DeleteAllContacts()
    {
        var deleteAllContactsService = _scope.ServiceProvider.GetRequiredService<DeleteAllContactsService>();

        await deleteAllContactsService.DeleteAllContactsAsync();

        var contacts = await _scope.ServiceProvider.GetRequiredService<GetAllContactsService>().GetAllContactsAsync();
        contacts.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllContactsViaCommand()
    {
        var deleteAllContactsService =
            _scope.ServiceProvider.GetRequiredKeyedService<DeleteAllContactsService>(AdoNetKey);

        await deleteAllContactsService.DeleteAllContactsAsync();

        var contacts = await _scope.ServiceProvider.GetRequiredService<GetAllContactsService>().GetAllContactsAsync();
        contacts.Should().BeEmpty();
    }
}