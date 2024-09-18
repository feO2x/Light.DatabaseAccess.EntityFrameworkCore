# Light.DatabaseAccess.EntityFrameworkCore

*Implements the database access abstractions from [Light.SharedCore](https://github.com/feO2x/Light.SharedCore/) for Entity Framework Core.*

![Light Logo](light-logo.png)

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/feO2x/Light.DatabaseAccess.EntityFrameworkCore/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-1.1.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages/Light.DatabaseAccess.EntityFrameworkCore/)

## How to install

Light.DatabaseAccess.EntityFrameworkCore is compiled against .NET 8 and available as a NuGet package. It can be installed via:

- **Package Reference in csproj**: `<PackageReference Include="Light.DatabaseAccess.EntityFrameworkCore" Version="1.1.0" />`
- **dotnet CLI**: `dotnet add package Light.DatabaseAccess.EntityFrameworkCore`
- **Visual Studio Package Manager Console**: `Install-Package Light.DatabaseAccess.EntityFrameworkCore`

## Why should you use Light.DatabaseAccess.EntityFrameworkCore?

When we implement a service that wants to perform database I/O, we often see Entity Framework Core's `DbContext` used as a direct dependency. The following code example shows this for a simple CRUD update operation:

```csharp
public sealed class UpdateContactService
{
    private readonly UpdateContactDtoValidator _validator;
    private readonly MyDbContext _dbContext;

    public UpdateContactService(
        UpdateContactDtoValidator validator,
        MyDbContext dbContext
    )
    {
        _validator = validator;
        _dbContext = dbContext;
    }

    public async Task<IResult> UpdateContactAsync(
        UpdateContactDto contact,
        CancellationToken cancellationToken = default
    )
    {
        if (validator.CheckForErrors(dto, out var errors))
        {
            return Results.BadRequest(errors);
        }
        
        var existingContact = await dbContext
            .Contacts
            .FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);
        
        if (existingContact is null)
        {
            return Results.NotFound();
        }
        
        existingContact.FirstName = dto.FirstName;
        existingContact.LastName = dto.LastName;
        existingContact.Email = dto.Email;
        existingContact.Phone = dto.Phone;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Results.NoContent();
    }
}
```

This piece of domain logic is tightly coupled to Entity Framework Core, it cannot be run without it. In Unit Tests where we want to replace the I/O calls with a test double, we cannot simply implement a mock on our own, we need to rely on the capabilities of mocking frameworks like NSubstitute or Moq which only work by using reflection internally. Furthermore, what if we want to replace EF Core with e.g. Dapper or plain ADO.NET for certain endpoints to benefit from their performance characteristics? While not impossible, this would result in specifics like SQL statements being spread across our business logic, because we did not separate database access from the domain, violating the Single Responsibility Principle.

Instead, I recommend to create an interface that abstracts the database access code. For the example above, it could look like this:

```csharp
public interface IUpdateContactSession : IAsyncSession
{
    Task<Contact?> GetContactAsync(Guid id, CancellationToken cancellationToken = default);
}
```

The `IAsyncSession` interface is part of Light.SharedCore and provides the `SaveChangesAsync` method as well as the capability to dispose the session. To easily implement this interface, use the abstract base classes of **Light.DatabaseAccess.EntityFrameworkCore**:

```csharp
public sealed class EfUpdateContactSession : EfAsyncSession<MyDbContext>, IUpdateContactSession
{
    public EfUpdateContactSession(MyDbContext dbContext) : base(dbContext) { }
    
    public Task<Contact?> GetContactAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext
            .Contacts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
```

The `EfAsyncSession<TDbContext>` base class implements `IAsyncSession` for you and forwards the calls `SaveChangesAsync`, `DisposeAsync`, and `Dispose` to the `DbContext`. Now, you can refactor the `UpdateContactService` to use the `IUpdateContactSession` interface:

```csharp
public sealed class UpdateContactService
{
    private readonly UpdateContactDtoValidator _validator;
    private readonly IUpdateContactSession _session;

    public UpdateContactService(
        UpdateContactDtoValidator validator,
        IUpdateContactSession session
    )
    {
        _validator = validator;
        _session = session;
    }

    public async Task<IResult> UpdateContactAsync(
        UpdateContactDto contact,
        CancellationToken cancellationToken = default
    )
    {
        if (validator.CheckForErrors(dto, out var errors))
        {
            return Results.BadRequest(errors);
        }
        
        var existingContact = await _session.GetContactAsync(dto.Id, cancellationToken);
        
        if (existingContact is null)
        {
            return Results.NotFound();
        }
        
        existingContact.FirstName = dto.FirstName;
        existingContact.LastName = dto.LastName;
        existingContact.Email = dto.Email;
        existingContact.Phone = dto.Phone;
        
        await _session.SaveChangesAsync(cancellationToken);
        
        return Results.NoContent();
    }
}
```

For everything to work, don't forget to register the `IUpdateContactSession` in the dependency injection container:

```csharp
services.AddScoped<IUpdateContactSession, EfUpdateContactSession>();
```

You can now easily replace the `EfUpdateContactSession` with any other implementation of `IUpdateContactSession` without changing the business logic, and easily implement your own in-memory mock for unit tests.

## The base classes of Light.DatabaseAccess.EntityFrameworkCore

Light.SharedCore provides two essential interfaces for database access:

- `IAsyncReadOnlySession`: represents a connection to the database that only reads data. Data will not be manipulated and thus a `SaveChangesAsync` method is not available. This interface is implemented by `EfAsyncReadOnlySession<TDbContext>` in this package. This base class will set the `ChangeTracker.QueryTrackingBehavior` to `NoTrackingWithIdentityResolution` by default to avoid overhead - this way, you do not need to call `AsNoTracking` or `AsNoTrackingWithIdentityResolution` in your queries. You can adjust this by passing a different `queryTrackingBehavior` value to the constructor.
- `IAsyncSession`: represents a connection to the database which manipulates data. It has an additional `SaveChangesAsync` method to persist changes to the database. This interface is implemented by `EfAsyncSession<TDbContext>` in this package - the Query Tracking Behavior is set to `TrackAll` (the default value for EF Core's DB Context).

If you want to use a dedicated transaction for a session, you can derive from the `EfAsyncSession<TDbContext>.WithTransaction` or `EfAsyncReadOnlySession<TDbContext>.WithTransaction` classes. In addition to the `DbContext` property, these base classes provide a `GetDbContextAsync` method which will initialize the transaction upon first retrieval. This underlying transaction will be committed when `SaveChangesAsync` is called. You can pass the isolation level of the transaction via the constructor, the default value is `IsolationLevel.ReadCommitted`.

The `IUpdateContactSession` implementation from above could look like this when using a dedicated transaction:

```csharp
public sealed class EfUpdateContactSession : EfAsyncSession<MyDbContext>.WithTransaction,
                                             IUpdateContactSession
{
    public EfUpdateContactSession(MyDbContext dbContext)
        : base(dbContext, IsolationLevel.ReadCommitted) { }
    
    public async Task<Contact?> GetContactAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // The first call to GetDbContextAsync initializes the underlying transaction asynchronously
        var dbContext = await GetDbContextAsync(cancellationToken); 
        return await dbContext
            .Contacts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
```

> Most ADO.NET providers like Npgsql or Microsoft.Data.SqlClient do not actually make a call to the database when BeginTransaction(Async) is called. They simply mark the next command so that it includes a BEGIN TRANSACTION statement at the beginning. Therefore, it is usually OK to simply use the `DbContext` property in your sessions deriving from `EfAsyncSession<TDbContext>.WithTransaction` or `EfAsyncReadOnlySession<TDbContext>.WithTransaction`. Please consult the documentation of your ADO.NET provider for details.

## Creating ADO.NET DbCommand objects from DbContext

While Entity Framework Core is powerful, you sometimes need to resort to ADO.NET for specific SQL queries. All session base classes provide a `CreateDbCommand` method that returns a fully initialized `DbCommand` object. If necessary, the DB connection will be opened and assigned to the command, the same thing is done with a possible transaction. The following code shows two examples of sessions that use commands:

```csharp
public sealed class AdoNetDeleteAllContactsSession : EfAsyncSession<MyDbContext>.WithTransaction,
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

public sealed class AdoNetGetAllContactsSession : EfAsyncReadOnlySession<MyDbContext>, IGetAllContactsSession
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
```

Please note that DB commands usually have their own implicit READ COMMITTED transaction if you do not specify an explicit one. If you execute multiple commands within the same session, you probably want to put a transaction around them unless your queries incorporate optimistic concurrency.   

## Tips and tricks

- If you have a service that only reads data, use the `IAsyncReadOnlySession` interface instead of `IAsyncSession`. Derive your session implementation from `EfAsyncReadOnlySession<TDbContext>`. This way, you can ensure that no accidental writes are made to the database and follow the Dependency Inversion Principle.
- Provide a session for each use case: instead of having a single session for all database operations or for a single entity, create a session for each use case (in backend services, this means one DB session per endpoint). This keeps each of your sessions focussed. If you have queries that are used in multiple sessions, place the code in a static method and call it from all sessions. This pattern promotes Vertical Slice Architecture.
- Do not call `SaveChangesAsync` multiple times during a scope (in ASP.NET Core, this would be an endpoint call). This will effectively negate the transactional capabilities of the database: what happens if the first `SaveChangesAsync` call succeeds, but subsequent ones fail? The database will be in an inconsistent state.
- Do not introduce other disposable resources or finalizers in your session. A session is a [Humble Object](https://martinfowler.com/bliki/HumbleObject.html) that represents a connection with an optional transaction to one third-party system. If you want to access different services or resources, create a new session for each of them. If you want to access multiple systems for the same data (e.g. a Redis distributed cache and the database), use pipes and filters with a dedicated session for each filter in the pipeline.
- The base classes are not implemented in a thread-safe way. Register them as a scoped dependency so that each context has its own instance. Do not use them as a singleton or transient dependency (Microsoft's DI container does not like transient dependencies that implement `IDisposable`/`IAsyncDisposable`).
- Also follow the Dependency Inversion Principle when it comes to the design of your session interface: do not expose database-specific details, like `IQueryable<T>` or `DbSet<T>`. Instead, design the interface from the caller's perspective. This way, you can easily replace the underlying database technology without changing the business logic.
- When designing sessions, be careful about the number of entities that you load into memory with a single operation. In GET endpoints, use proper paging and filtering to avoid loading the entire table into memory, and use `AsNoTracking` or `AsNoTrackingWithIdentityResolution` when you don't need change tracking.
