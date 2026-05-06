using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NTech.KeyVault.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppConfigurationNoUniqe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppConfigurations_ApplicationId_Version",
                table: "AppConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_ApplicationId_Version",
                table: "AppConfigurations",
                columns: new[] { "ApplicationId", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppConfigurations_ApplicationId_Version",
                table: "AppConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_ApplicationId_Version",
                table: "AppConfigurations",
                columns: new[] { "ApplicationId", "Version" },
                unique: true);
        }
    }
}
