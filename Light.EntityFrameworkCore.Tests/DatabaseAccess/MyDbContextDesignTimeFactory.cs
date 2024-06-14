using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Light.EntityFrameworkCore.Tests.DatabaseAccess;

public sealed class MyDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
           .UseNpgsql(string.Empty)
           .Options;

        return new MyDbContext(options);
    }
}