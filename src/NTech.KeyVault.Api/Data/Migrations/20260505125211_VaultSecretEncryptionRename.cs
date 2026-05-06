using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NTech.KeyVault.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class VaultSecretEncryptionRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EncryptedData",
                table: "VaultSecrets",
                newName: "ValueNonce");

            migrationBuilder.RenameColumn(
                name: "DataNonce",
                table: "VaultSecrets",
                newName: "EncryptedValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValueNonce",
                table: "VaultSecrets",
                newName: "EncryptedData");

            migrationBuilder.RenameColumn(
                name: "EncryptedValue",
                table: "VaultSecrets",
                newName: "DataNonce");
        }
    }
}
