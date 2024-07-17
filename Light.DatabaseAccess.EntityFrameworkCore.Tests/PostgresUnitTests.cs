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
    [InlineData(IsolationLevel.ReadCommitted, QueryTrackingBehavior.TrackAll)]
    [InlineData(IsolationLevel.Serializable, QueryTrackingBehavior.NoTrackingWithIdentityResolution)]
    [InlineData(IsolationLevel.RepeatableRead, QueryTrackingBehavior.NoTracking)]
    public void PassIsolationLevelAndQueryTrackingBehaviorToBaseClass(
        IsolationLevel isolationLevel,
        QueryTrackingBehavior queryTrackingBehavior
    )
    {
        var dbContext = new MyDbContext(new DbContextOptionsBuilder<MyDbContext>().UseNpgsql().Options);

        var session = new SomeSession(dbContext, isolationLevel, queryTrackingBehavior);

        var sessionType = typeof(EfAsyncReadOnlySession<MyDbContext>.WithTransaction);
        var actualLevel = (IsolationLevel) sessionType
           .GetField("_isolationLevel", BindingFlags.Instance | BindingFlags.NonPublic)!
           .GetValue(session)!;
        actualLevel.Should().Be(isolationLevel);
        dbContext.ChangeTracker.QueryTrackingBehavior.Should().Be(queryTrackingBehavior);
    }

    private sealed class SomeSession : EfAsyncSession<MyDbContext>.WithTransaction
    {
        public SomeSession(
            MyDbContext dbContext,
            IsolationLevel isolationLevel,
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll
        ) : base(dbContext, isolationLevel, queryTrackingBehavior) { }
    }
}