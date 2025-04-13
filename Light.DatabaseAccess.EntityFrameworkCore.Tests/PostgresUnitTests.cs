using System.Data;
using System.Reflection;
using FluentAssertions;
using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests;

public sealed class PostgresUnitTests
{
    [Theory]
    [InlineData(QueryTrackingBehavior.NoTracking)]
    [InlineData(QueryTrackingBehavior.TrackAll)]
    [InlineData(QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    public void PassQueryTrackingBehaviorToBaseClass(QueryTrackingBehavior queryTrackingBehavior)
    {
        using var dbContext = new MyDbContext(new DbContextOptionsBuilder<MyDbContext>().UseNpgsql().Options);
        using var session = new SomeSession(dbContext, queryTrackingBehavior);

        dbContext.ChangeTracker.QueryTrackingBehavior.Should().Be(queryTrackingBehavior);
    }

    [Theory]
    [InlineData(IsolationLevel.ReadCommitted, QueryTrackingBehavior.TrackAll)]
    [InlineData(IsolationLevel.Serializable, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(IsolationLevel.RepeatableRead, QueryTrackingBehavior.NoTracking)]
    public void PassIsolationLevelAndQueryTrackingBehaviorToBaseClass(
        IsolationLevel isolationLevel,
        QueryTrackingBehavior queryTrackingBehavior
    )
    {
        using var dbContext = new MyDbContext(new DbContextOptionsBuilder<MyDbContext>().UseNpgsql().Options);
        using var session = new SomeTransactionalSession(dbContext, isolationLevel, queryTrackingBehavior);

        var sessionType = typeof(EfClient<MyDbContext>.WithTransaction);
        var actualLevel = (IsolationLevel) sessionType
                                          .GetField("_isolationLevel", BindingFlags.Instance | BindingFlags.NonPublic)!
                                          .GetValue(session)!;
        actualLevel.Should().Be(isolationLevel);
        dbContext.ChangeTracker.QueryTrackingBehavior.Should().Be(queryTrackingBehavior);
    }

    private sealed class SomeSession(
        MyDbContext dbContext,
        QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll
    )
        : EfSession<MyDbContext>(dbContext, queryTrackingBehavior);

    private sealed class SomeTransactionalSession(
        MyDbContext dbContext,
        IsolationLevel isolationLevel,
        QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll
    )
        : EfSession<MyDbContext>.WithTransaction(dbContext, isolationLevel, queryTrackingBehavior);
}