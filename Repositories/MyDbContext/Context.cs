using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.MyDbContext;

public partial class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    { }

    public DbSet<Roles> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.RoleName)
                  .IsRequired()
                  .HasMaxLength(255);
        });
    }
}