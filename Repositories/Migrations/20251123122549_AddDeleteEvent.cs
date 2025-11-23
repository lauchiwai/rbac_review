using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Roles_ReviewerRole",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_CreatedByRole",
                table: "TodoLists",
                column: "CreatedByRole");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Roles_ReviewerRole",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_Roles_CreatedByRole",
                table: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_TodoLists_CreatedByRole",
                table: "TodoLists");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Roles_ReviewerRole",
                table: "Reviews",
                column: "ReviewerRole",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
