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

    public DbSet<ReviewTemplates> ReviewTemplates { get; set; }

    public DbSet<ReviewStages> ReviewStages { get; set; }

    public DbSet<StageTransitions> StageTransitions { get; set; }

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

            entity.HasOne(e => e.ReviewTemplate)
                  .WithMany(t => t.TodoLists)
                  .HasForeignKey(e => e.TemplateId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.CurrentStage)
                  .WithMany(s => s.TodoLists)
                  .HasForeignKey(e => e.CurrentStageId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_TodoLists_Status");

            entity.HasIndex(e => e.CreatedByUserId)
                    .HasDatabaseName("IX_TodoLists_CreatedByUserId");

            entity.HasIndex(e => e.CurrentReviewerUserId)
                    .HasDatabaseName("IX_TodoLists_CurrentReviewerUserId");

            entity.HasIndex(e => e.TemplateId)
                    .HasDatabaseName("IX_TodoLists_TemplateId");

            entity.HasIndex(e => e.CurrentStageId)
                    .HasDatabaseName("IX_TodoLists_CurrentStageId");
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

            entity.HasOne(e => e.ReviewStage)
                  .WithMany(s => s.Reviews)
                  .HasForeignKey(e => e.StageId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TodoId)
                    .HasDatabaseName("IX_Reviews_TodoId");

            entity.HasIndex(e => e.ReviewerUserId)
                    .HasDatabaseName("IX_Reviews_ReviewerUserId");

            entity.HasIndex(e => e.ReviewedAt)
                    .HasDatabaseName("IX_Reviews_ReviewedAt");

            entity.HasIndex(e => e.StageId)
                    .HasDatabaseName("IX_Reviews_StageId");
        });

        modelBuilder.Entity<ReviewTemplates>(entity =>
        {
            entity.HasKey(e => e.TemplateId);

            entity.Property(e => e.TemplateId)
                    .ValueGeneratedOnAdd();

            entity.Property(e => e.TemplateName)
                    .IsRequired()
                    .HasMaxLength(255);

            entity.Property(e => e.Description)
                    .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TemplateName)
                    .IsUnique()
                    .HasDatabaseName("IX_ReviewTemplates_TemplateName");
        });

        modelBuilder.Entity<ReviewStages>(entity =>
        {
            entity.HasKey(e => e.StageId);

            entity.Property(e => e.StageId)
                    .ValueGeneratedOnAdd();

            entity.Property(e => e.StageName)
                    .IsRequired()
                    .HasMaxLength(255);

            entity.Property(e => e.StageOrder)
                    .IsRequired();

            entity.HasOne(e => e.ReviewTemplate)
                  .WithMany(t => t.ReviewStages)
                  .HasForeignKey(e => e.TemplateId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RequiredRole)
                  .WithMany()
                  .HasForeignKey(e => e.RequiredRoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SpecificReviewerUser)
                   .WithMany()
                   .HasForeignKey(e => e.SpecificReviewerUserId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired(false);

            entity.HasIndex(e => new { e.TemplateId, e.StageOrder })
                    .IsUnique()
                    .HasDatabaseName("IX_ReviewStages_TemplateId_StageOrder");

            entity.HasIndex(e => e.RequiredRoleId)
                    .HasDatabaseName("IX_ReviewStages_RequiredRoleId");

            entity.HasIndex(e => e.SpecificReviewerUserId)
                    .HasDatabaseName("IX_ReviewStages_SpecificReviewerUserId");
        });

        modelBuilder.Entity<StageTransitions>(entity =>
        {
            entity.HasKey(e => e.TransitionId);

            entity.Property(e => e.TransitionId)
                    .ValueGeneratedOnAdd();

            entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasMaxLength(50);

            entity.Property(e => e.ResultStatus)
                    .IsRequired()
                    .HasMaxLength(50);

            entity.HasOne(e => e.FromStage)
                  .WithMany(s => s.FromStageTransitions)
                  .HasForeignKey(e => e.StageId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ToStage)
                  .WithMany(s => s.ToStageTransitions)
                  .HasForeignKey(e => e.NextStageId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => new { e.StageId, e.ActionName })
                    .IsUnique()
                    .HasDatabaseName("IX_StageTransitions_StageId_ActionName");

            entity.HasIndex(e => e.NextStageId)
                    .HasDatabaseName("IX_StageTransitions_NextStageId");
        });

        modelBuilder.Entity<Roles>().HasData(
            new Roles { RoleId = 1, RoleName = "員工" },
            new Roles { RoleId = 2, RoleName = "資深員工" },
            new Roles { RoleId = 3, RoleName = "主管" },
            new Roles { RoleId = 4, RoleName = "管理員" }
        );

        modelBuilder.Entity<Permissions>().HasData(
            new Permissions { PermissionId = 1, PermissionName = "todo_create" },
            new Permissions { PermissionId = 2, PermissionName = "todo_review_level1" },
            new Permissions { PermissionId = 3, PermissionName = "todo_review_level2" },
            new Permissions { PermissionId = 4, PermissionName = "admin_manage" }
        );

        modelBuilder.Entity<Roles_Permissions>().HasData(
            new Roles_Permissions { RoleId = 1, PermissionId = 1 },
            new Roles_Permissions { RoleId = 2, PermissionId = 2 },
            new Roles_Permissions { RoleId = 3, PermissionId = 3 },
            new Roles_Permissions { RoleId = 4, PermissionId = 4 }
        );

        modelBuilder.Entity<Users>().HasData(
            new Users { UserId = 101, CreatedAt = new DateTime(2024, 1, 1) },
            new Users { UserId = 102, CreatedAt = new DateTime(2024, 1, 1) },
            new Users { UserId = 103, CreatedAt = new DateTime(2024, 1, 1) },
            new Users { UserId = 104, CreatedAt = new DateTime(2024, 1, 1) },
            new Users { UserId = 105, CreatedAt = new DateTime(2024, 1, 1) },
            new Users { UserId = 106, CreatedAt = new DateTime(2024, 1, 1) }
        );

        modelBuilder.Entity<Users_Roles>().HasData(
            new Users_Roles { UserId = 101, RoleId = 1 },
            new Users_Roles { UserId = 102, RoleId = 2 },
            new Users_Roles { UserId = 103, RoleId = 2 },
            new Users_Roles { UserId = 104, RoleId = 3 },
            new Users_Roles { UserId = 105, RoleId = 3 },
            new Users_Roles { UserId = 106, RoleId = 4 }
        );
    }
}