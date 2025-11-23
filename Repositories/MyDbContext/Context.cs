using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.MyDbContext;

public partial class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    { }

    public DbSet<Roles> Roles { get; set; }

    public DbSet<Permissions> Permissions { get; set; }

    public DbSet<Roles_Permissions> Roles_Permissions { get; set; }


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

        modelBuilder.Entity<Permissions>(entity =>
        {
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionId)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.PermissionName)
                  .IsRequired()
                  .HasMaxLength(255);
        });

        modelBuilder.Entity<Roles_Permissions>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Roles_Permissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                  .WithMany(p => p.Roles_Permissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}