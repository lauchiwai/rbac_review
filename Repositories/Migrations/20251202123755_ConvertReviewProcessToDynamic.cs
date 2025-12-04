using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ConvertReviewProcessToDynamic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_NextReviewerUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ReviewLevel",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ReviewLevel",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "NextReviewerUserId",
                table: "Reviews",
                newName: "StageTransitionTransitionId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_NextReviewerUserId",
                table: "Reviews",
                newName: "IX_Reviews_StageTransitionTransitionId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStageId",
                table: "TodoLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "TodoLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_ReviewTemplates_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReviewStages",
                columns: table => new
                {
                    StageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    StageName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    RequiredRoleId = table.Column<int>(type: "int", nullable: false),
                    SpecificReviewerUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewStages", x => x.StageId);
                    table.ForeignKey(
                        name: "FK_ReviewStages_ReviewTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ReviewTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewStages_Roles_RequiredRoleId",
                        column: x => x.RequiredRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewStages_Users_SpecificReviewerUserId",
                        column: x => x.SpecificReviewerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StageTransitions",
                columns: table => new
                {
                    TransitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NextStageId = table.Column<int>(type: "int", nullable: true),
                    ResultStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageTransitions", x => x.TransitionId);
                    table.ForeignKey(
                        name: "FK_StageTransitions_ReviewStages_NextStageId",
                        column: x => x.NextStageId,
                        principalTable: "ReviewStages",
                        principalColumn: "StageId");
                    table.ForeignKey(
                        name: "FK_StageTransitions_ReviewStages_StageId",
                        column: x => x.StageId,
                        principalTable: "ReviewStages",
                        principalColumn: "StageId");
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "PermissionId", "PermissionName" },
                values: new object[,]
                {
                    { 1, "todo_create" },
                    { 2, "todo_review_level1" },
                    { 3, "todo_review_level2" },
                    { 4, "admin_manage" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "員工" },
                    { 2, "資深員工" },
                    { 3, "主管" },
                    { 4, "管理員" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt" },
                values: new object[,]
                {
                    { 101, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 102, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 103, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 104, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 105, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 106, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Roles_Permissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 }
                });

            migrationBuilder.InsertData(
                table: "Users_Roles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, 101 },
                    { 2, 102 },
                    { 2, 103 },
                    { 3, 104 },
                    { 3, 105 },
                    { 4, 106 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_CurrentStageId",
                table: "TodoLists",
                column: "CurrentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoLists_TemplateId",
                table: "TodoLists",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_StageId",
                table: "Reviews",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStages_RequiredRoleId",
                table: "ReviewStages",
                column: "RequiredRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStages_SpecificReviewerUserId",
                table: "ReviewStages",
                column: "SpecificReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStages_TemplateId_StageOrder",
                table: "ReviewStages",
                columns: new[] { "TemplateId", "StageOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTemplates_CreatedByUserId",
                table: "ReviewTemplates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTemplates_TemplateName",
                table: "ReviewTemplates",
                column: "TemplateName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitions_NextStageId",
                table: "StageTransitions",
                column: "NextStageId");

            migrationBuilder.CreateIndex(
                name: "IX_StageTransitions_StageId_ActionName",
                table: "StageTransitions",
                columns: new[] { "StageId", "ActionName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_ReviewStages_StageId",
                table: "Reviews",
                column: "StageId",
                principalTable: "ReviewStages",
                principalColumn: "StageId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_StageTransitions_StageTransitionTransitionId",
                table: "Reviews",
                column: "StageTransitionTransitionId",
                principalTable: "StageTransitions",
                principalColumn: "TransitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_ReviewStages_CurrentStageId",
                table: "TodoLists",
                column: "CurrentStageId",
                principalTable: "ReviewStages",
                principalColumn: "StageId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoLists_ReviewTemplates_TemplateId",
                table: "TodoLists",
                column: "TemplateId",
                principalTable: "ReviewTemplates",
                principalColumn: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_ReviewStages_StageId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_StageTransitions_StageTransitionTransitionId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_ReviewStages_CurrentStageId",
                table: "TodoLists");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoLists_ReviewTemplates_TemplateId",
                table: "TodoLists");

            migrationBuilder.DropTable(
                name: "StageTransitions");

            migrationBuilder.DropTable(
                name: "ReviewStages");

            migrationBuilder.DropTable(
                name: "ReviewTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TodoLists_CurrentStageId",
                table: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_TodoLists_TemplateId",
                table: "TodoLists");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_StageId",
                table: "Reviews");

            migrationBuilder.DeleteData(
                table: "Roles_Permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "Roles_Permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "Roles_Permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 3, 3 });

            migrationBuilder.DeleteData(
                table: "Roles_Permissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 4, 4 });

            migrationBuilder.DeleteData(
                table: "Users_Roles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 101 });

            migrationBuilder.DeleteData(
                table: "Users_Roles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 102 });

            migrationBuilder.DeleteData(
                table: "Users_Roles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 103 });

            migrationBuilder.DeleteData(
                table: "Users_Roles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 3, 104 });

            migrationBuilder.DeleteData(
                table: "Users_Roles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 3, 105 });

            migrationBuilder.DeleteData(
                table: "Users_Roles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 4, 106 });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "PermissionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 106);

            migrationBuilder.DropColumn(
                name: "CurrentStageId",
                table: "TodoLists");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "TodoLists");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "StageTransitionTransitionId",
                table: "Reviews",
                newName: "NextReviewerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_StageTransitionTransitionId",
                table: "Reviews",
                newName: "IX_Reviews_NextReviewerUserId");

            migrationBuilder.AddColumn<int>(
                name: "ReviewLevel",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewLevel",
                table: "Reviews",
                column: "ReviewLevel");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_NextReviewerUserId",
                table: "Reviews",
                column: "NextReviewerUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
