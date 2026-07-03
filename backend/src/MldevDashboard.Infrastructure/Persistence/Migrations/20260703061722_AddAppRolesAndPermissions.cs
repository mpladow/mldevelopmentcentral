using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MldevDashboard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemDefinitionId = table.Column<int>(type: "int", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationPermissions_Systems_SystemDefinitionId",
                        column: x => x.SystemDefinitionId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationRoles_Systems_SystemDefinitionId",
                        column: x => x.SystemDefinitionId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRolePermissions",
                columns: table => new
                {
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    ApplicationPermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRolePermissions", x => new { x.ApplicationRoleId, x.ApplicationPermissionId });
                    table.ForeignKey(
                        name: "FK_ApplicationRolePermissions_ApplicationPermissions_ApplicationPermissionId",
                        column: x => x.ApplicationPermissionId,
                        principalTable: "ApplicationPermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationRolePermissions_ApplicationRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemAccountRoles",
                columns: table => new
                {
                    SystemDefinitionId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAccountRoles", x => new { x.SystemDefinitionId, x.AccountId, x.ApplicationRoleId });
                    table.ForeignKey(
                        name: "FK_SystemAccountRoles_ApplicationRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "ApplicationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemAccountRoles_Systems_SystemDefinitionId",
                        column: x => x.SystemDefinitionId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPermissions_PermissionKey",
                table: "ApplicationPermissions",
                column: "PermissionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPermissions_SystemDefinitionId",
                table: "ApplicationPermissions",
                column: "SystemDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRolePermissions_ApplicationPermissionId",
                table: "ApplicationRolePermissions",
                column: "ApplicationPermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_Name",
                table: "ApplicationRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoles_SystemDefinitionId",
                table: "ApplicationRoles",
                column: "SystemDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccountRoles_AccountId",
                table: "SystemAccountRoles",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccountRoles_ApplicationRoleId",
                table: "SystemAccountRoles",
                column: "ApplicationRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationRolePermissions");

            migrationBuilder.DropTable(
                name: "SystemAccountRoles");

            migrationBuilder.DropTable(
                name: "ApplicationPermissions");

            migrationBuilder.DropTable(
                name: "ApplicationRoles");
        }
    }
}
