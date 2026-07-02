using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MldevDashboard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemThemeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemThemeSettings",
                columns: table => new
                {
                    SystemDefinitionId = table.Column<int>(type: "int", nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SurfaceColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BorderRadius = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemThemeSettings", x => x.SystemDefinitionId);
                    table.ForeignKey(
                        name: "FK_SystemThemeSettings_Systems_SystemDefinitionId",
                        column: x => x.SystemDefinitionId,
                        principalTable: "Systems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemThemeSettings");
        }
    }
}
