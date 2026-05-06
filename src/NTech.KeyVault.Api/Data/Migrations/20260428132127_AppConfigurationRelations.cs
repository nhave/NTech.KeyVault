using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NTech.KeyVault.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppConfigurationRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurations_Users_CreatedById",
                table: "AppConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_ApplicationId_Version",
                table: "AppConfigurations",
                columns: new[] { "ApplicationId", "Version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurations_Users_CreatedById",
                table: "AppConfigurations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurations_Users_CreatedById",
                table: "AppConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_AppConfigurations_ApplicationId_Version",
                table: "AppConfigurations");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurations_Users_CreatedById",
                table: "AppConfigurations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
