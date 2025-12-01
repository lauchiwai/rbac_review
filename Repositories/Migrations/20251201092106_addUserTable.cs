using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Roles_ReviewerRole",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_Roles_CreatedByRole",
                table: "TodoLists");

            migrationBuilder.RenameColumn(
                name: "CreatedByRole",
                table: "TodoLists",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TodoLists_CreatedByRole",
                table: "TodoLists",
                newName: "IX_TodoLists_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "ReviewerRole",
                table: "Reviews",
                newName: "ReviewerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ReviewerRole",
                table: "Reviews",
                newName: "IX_Reviews_ReviewerUserId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentReviewerUserId",
                table: "TodoLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextReviewerUserId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Users_Roles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users_Roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_Users_Roles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Roles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_CurrentReviewerUserId",
                table: "TodoLists",
                column: "CurrentReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_Status",
                table: "TodoLists",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_NextReviewerUserId",
                table: "Reviews",
                column: "NextReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionName",
                table: "Permissions",
                column: "PermissionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Roles_RoleId",
                table: "Users_Roles",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_NextReviewerUserId",
                table: "Reviews",
                column: "NextReviewerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_ReviewerUserId",
                table: "Reviews",
                column: "ReviewerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_Users_CreatedByUserId",
                table: "TodoLists",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_Users_CurrentReviewerUserId",
                table: "TodoLists",
                column: "CurrentReviewerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_NextReviewerUserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_ReviewerUserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_Users_CreatedByUserId",
                table: "TodoLists");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_Users_CurrentReviewerUserId",
                table: "TodoLists");

            migrationBuilder.DropTable(
                name: "Users_Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TodoLists_CurrentReviewerUserId",
                table: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_TodoLists_Status",
                table: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_Roles_RoleName",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_NextReviewerUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_PermissionName",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "CurrentReviewerUserId",
                table: "TodoLists");

            migrationBuilder.DropColumn(
                name: "NextReviewerUserId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "TodoLists",
                newName: "CreatedByRole");

            migrationBuilder.RenameIndex(
                name: "IX_TodoLists_CreatedByUserId",
                table: "TodoLists",
                newName: "IX_TodoLists_CreatedByRole");

            migrationBuilder.RenameColumn(
                name: "ReviewerUserId",
                table: "Reviews",
                newName: "ReviewerRole");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ReviewerUserId",
                table: "Reviews",
                newName: "IX_Reviews_ReviewerRole");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Roles_ReviewerRole",
                table: "Reviews",
                column: "ReviewerRole",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_Roles_CreatedByRole",
                table: "TodoLists",
                column: "CreatedByRole",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
