using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NTech.KeyVault.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppConfigurationOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppConfigurations_ApplicationId",
                table: "AppConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_ApplicationId",
                table: "AppConfigurations",
                column: "ApplicationId",
                unique: true);
        }
    }
}
