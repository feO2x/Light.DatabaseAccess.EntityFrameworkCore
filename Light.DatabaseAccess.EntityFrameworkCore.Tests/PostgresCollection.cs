using Xunit;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests;

/// <summary>
/// Shares a single <see cref="PostgresFixture" /> (and thus a single PostgreSQL container) across
/// all integration test classes. Test classes in this collection run sequentially and share the
/// database state, so new test classes must not depend on or disturb the seeded default contacts.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
