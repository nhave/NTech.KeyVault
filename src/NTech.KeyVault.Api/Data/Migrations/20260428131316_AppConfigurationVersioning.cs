using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NTech.KeyVault.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppConfigurationVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "AppConfigurations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AppConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigurations_CreatedById",
                table: "AppConfigurations",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AppConfigurations_Users_CreatedById",
                table: "AppConfigurations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppConfigurations_Users_CreatedById",
                table: "AppConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_AppConfigurations_CreatedById",
                table: "AppConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "AppConfigurations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AppConfigurations");
        }
    }
}
