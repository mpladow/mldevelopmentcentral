using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MldevDashboard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemAccountAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemAccountAccesses",
                columns: table => new
                {
                    SystemDefinitionId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAccountAccesses", x => new { x.SystemDefinitionId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_SystemAccountAccesses_Systems_SystemDefinitionId",
                        column: x => x.SystemDefinitionId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemAccountAccesses_AccountId",
                table: "SystemAccountAccesses",
                column: "AccountId");

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Systems] WHERE [SystemKey] = N'global')
                BEGIN
                    INSERT INTO [Systems] ([SystemKey], [Label], [SortOrder], [IsActive])
                    VALUES (N'global', N'Global', 0, CAST(1 AS bit));
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemAccountAccesses");
        }
    }
}
