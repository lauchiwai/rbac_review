using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.MyDbContext;

public partial class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    { }

    public DbSet<Users> Users { get; set; }

    public DbSet<Users_Roles> Users_Roles { get; set; }

    public DbSet<Roles> Roles { get; set; }

    public DbSet<Permissions> Permissions { get; set; }

    public DbSet<Roles_Permissions> Roles_Permissions { get; set; }

    public DbSet<TodoLists> TodoLists { get; set; }

    public DbSet<Reviews> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                    .ValueGeneratedOnAdd();

            entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Users_Roles>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                    .WithMany(u => u.Users_Roles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users_Roles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.Property(e => e.RoleId)
                    .ValueGeneratedOnAdd();

            entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(255);

            entity.HasIndex(e => e.RoleName)
                    .IsUnique();
        });

        modelBuilder.Entity<Permissions>(entity =>
        {
            entity.HasKey(e => e.PermissionId);

            entity.Property(e => e.PermissionId)
                    .ValueGeneratedOnAdd();

            entity.Property(e => e.PermissionName)
                    .IsRequired()
                    .HasMaxLength(255);

            entity.HasIndex(e => e.PermissionName)
                    .IsUnique();
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

            entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CurrentReviewerUser)
                    .WithMany()
                    .HasForeignKey(e => e.CurrentReviewerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_TodoLists_Status");

            entity.HasIndex(e => e.CreatedByUserId)
                    .HasDatabaseName("IX_TodoLists_CreatedByUserId");

            entity.HasIndex(e => e.CurrentReviewerUserId)
                    .HasDatabaseName("IX_TodoLists_CurrentReviewerUserId");
        });

        modelBuilder.Entity<Reviews>(entity =>
        {
            entity.HasKey(e => e.ReviewId);

            entity.Property(e => e.ReviewId)
                    .ValueGeneratedOnAdd();

            entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(50);

            entity.Property(e => e.Comment)
                    .HasMaxLength(500);

            entity.Property(e => e.PreviousStatus)
                    .HasMaxLength(50);

            entity.Property(e => e.NewStatus)
                    .HasMaxLength(50);

            entity.Property(e => e.ReviewedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Todo)
                    .WithMany()
                    .HasForeignKey(e => e.TodoId)
                    .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewerUser)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NextReviewerUser)
                    .WithMany()
                    .HasForeignKey(e => e.NextReviewerUserId)
                    .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TodoId)
                    .HasDatabaseName("IX_Reviews_TodoId");

            entity.HasIndex(e => e.ReviewerUserId)
                    .HasDatabaseName("IX_Reviews_ReviewerUserId");

            entity.HasIndex(e => e.NextReviewerUserId)
                    .HasDatabaseName("IX_Reviews_NextReviewerUserId");

            entity.HasIndex(e => e.ReviewLevel)
                    .HasDatabaseName("IX_Reviews_ReviewLevel");

            entity.HasIndex(e => e.ReviewedAt)
                    .HasDatabaseName("IX_Reviews_ReviewedAt");
        });
    }
}