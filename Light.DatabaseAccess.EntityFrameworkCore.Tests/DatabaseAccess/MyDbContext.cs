using Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace Light.DatabaseAccess.EntityFrameworkCore.Tests.DatabaseAccess;

public sealed class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(
            options =>
            {
                options.Property(x => x.Id).ValueGeneratedNever();
                options.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
                options.Property(x => x.LastName).IsRequired(false).HasMaxLength(100);
                options.Property(x => x.Email).IsRequired(false).HasMaxLength(100);
            }
        );
    }
}