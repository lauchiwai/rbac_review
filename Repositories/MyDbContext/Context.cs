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

    public DbSet<TodoLists> TodoLists { get; set; }

    public DbSet<Reviews> Reviews { get; set; }

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

        modelBuilder.Entity<TodoLists>(entity =>
        {
            entity.HasKey(e => e.TodoListId);

            entity.Property(e => e.TodoListId)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.Status)
                 .IsRequired()
                 .HasMaxLength(50);

            entity.Property(e => e.CreatedByRole)
                 .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("GETDATE()");

            entity.HasOne<Roles>()
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByRole)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reviews>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.ReviewId)
                  .ValueGeneratedOnAdd();

            entity.HasOne(e => e.Todo)
                  .WithMany()
                  .HasForeignKey(e => e.TodoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewerRoleNavigation)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewerRole)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.ReviewLevel)
                  .IsRequired();

            entity.Property(e => e.Action)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.ReviewedAt)
                  .IsRequired()
                  .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.TodoId)
                  .HasDatabaseName("IX_Reviews_TodoId");

            entity.HasIndex(e => e.ReviewerRole)
                  .HasDatabaseName("IX_Reviews_ReviewerRole");
        });
    }
}