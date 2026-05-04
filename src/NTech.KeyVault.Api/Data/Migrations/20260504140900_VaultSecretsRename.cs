using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NTech.KeyVault.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class VaultSecretsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaultSecrets");

            migrationBuilder.CreateTable(
                name: "AppSecrets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    EncryptedData = table.Column<byte[]>(type: "bytea", nullable: false),
                    DataNonce = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedDataKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    DataKeyNonce = table.Column<byte[]>(type: "bytea", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSecrets_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSecrets_ApplicationId_Name",
                table: "AppSecrets",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSecrets");

            migrationBuilder.CreateTable(
                name: "VaultSecrets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataKeyNonce = table.Column<byte[]>(type: "bytea", nullable: false),
                    DataNonce = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedData = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedDataKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaultSecrets_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VaultSecrets_ApplicationId_Name",
                table: "VaultSecrets",
                columns: new[] { "ApplicationId", "Name" },
                unique: true);
        }
    }
}
