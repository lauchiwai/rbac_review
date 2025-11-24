using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Add_ReviewedAt_ReviewLevel_Indexes_To_Reviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewStatus",
                table: "Reviews",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousStatus",
                table: "Reviews",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewedAt",
                table: "Reviews",
                column: "ReviewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewLevel",
                table: "Reviews",
                column: "ReviewLevel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_ReviewedAt",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ReviewLevel",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "NewStatus",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "Reviews");
        }
    }
}
