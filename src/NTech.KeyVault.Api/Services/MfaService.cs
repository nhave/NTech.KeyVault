using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;
using NTech.KeyVault.Common.Models.Dtos.Requests;
using NTech.KeyVault.Common.Models.Dtos.Responses;
using OtpNet;
using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace NTech.KeyVault.Api.Services
{
    public interface IMfaService
    {
        public Task<EnableMfaResponse> EnableMfaAsync(EnableMfaRequest dto);
        public Task<byte[]> GenerateTotpQRCodeAsync();
        public Task<VerifyTotpResponse> VerifyTotpAsync(VerifyTotpRequest dto);
        public Task DisableMfaAsync(DisableMfaRequest dto);
    }

    public class MfaService(IMfaRepository mfaRepository, IUserService userService, IEncryptionService encryptionService) : IMfaService
    {
        public async Task<EnableMfaResponse> EnableMfaAsync(EnableMfaRequest dto)
        {
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            var existingMfaMethod = await mfaRepository.GetMfaMethodAsync(user.Id, dto.MethodType);
            if (existingMfaMethod != null && existingMfaMethod.IsEnabled)
                throw new ArgumentException("MFA method already enabled for this user.");

            switch (dto.MethodType)
            {
                case MfaMethodType.Totp:
                    var totpSecret = await CreateTotpMfaMethodAsync(user);
                    return new EnableMfaResponse(totpSecret);
                default:
                    throw new NotSupportedException("Unsupported MFA method type.");
            }
        }

        public async Task<byte[]> GenerateTotpQRCodeAsync()
        {
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            var mfaMethod = await mfaRepository.GetMfaMethodAsync(user.Id, MfaMethodType.Totp);
            if (mfaMethod == null)
                throw new InvalidOperationException("TOTP MFA method not found for user.");

            if (mfaMethod.IsEnabled)
                throw new InvalidOperationException("TOTP MFA method is already enabled.");

            var totpSecretBytes = encryptionService.Decrypt(
                mfaMethod.EncryptedSecret,
                mfaMethod.EncryptedDataKey,
                mfaMethod.SecretNonce,
                mfaMethod.DataKeyNonce);

            var totpSecret = Encoding.UTF8.GetString(totpSecretBytes.Value);

            // Generate otpauth URI
            var totpUri = $"otpauth://totp/{user.Username}?secret={totpSecret}&issuer=NTech KeyVault";

            // Generate QR code
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPng = qrCode.GetGraphic(5);

            return qrCodeAsPng;
        }

        public async Task<VerifyTotpResponse> VerifyTotpAsync(VerifyTotpRequest dto)
        {
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            var mfaMethod = await mfaRepository.GetMfaMethodAsync(user.Id, MfaMethodType.Totp);
            if (mfaMethod == null)
                throw new InvalidOperationException("TOTP MFA method not found for user.");

            if (mfaMethod.IsEnabled)
                throw new InvalidOperationException("TOTP MFA method is already enabled.");

            var totpSecretBytes = encryptionService.Decrypt(
                mfaMethod.EncryptedSecret,
                mfaMethod.EncryptedDataKey,
                mfaMethod.SecretNonce,
                mfaMethod.DataKeyNonce);

            var totpSecret = Encoding.UTF8.GetString(totpSecretBytes.Value);

            var totp = new Totp(Base32Encoding.ToBytes(totpSecret));
            if (totp.VerifyTotp(dto.Code, out long timeStepMatched, new VerificationWindow(1, 1)))
            {
                mfaMethod.IsEnabled = true;

                var backupCodes = new List<String>();
                for (int i = 0; i < 10; i++)
                {
                    backupCodes.Add(GenerateTotpBackupCode(4));
                }
                mfaMethod.BackupCodeHashes = [.. backupCodes.Select(code => encryptionService.CreateLookupHash(code))];

                await mfaRepository.UpdateMfaMethodAsync(mfaMethod);

                return new VerifyTotpResponse(backupCodes);
            }
            else
            {
                throw new ArgumentException("Invalid TOTP code.");
            }
        }

        public async Task DisableMfaAsync(DisableMfaRequest dto)
        {
            var user = await userService.GetCurrentUserAsync();
            if (user == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            var mfaMethod = await mfaRepository.GetMfaMethodAsync(user.Id, MfaMethodType.Totp);
            if (mfaMethod == null || !mfaMethod.IsEnabled)
                throw new InvalidOperationException("MFA method is not enabled.");

            switch (dto.MethodType)
            {
                case MfaMethodType.Totp:
                    await DisableTotpMethodAsync(user, mfaMethod, dto.Code);
                    return;
                default:
                    throw new NotSupportedException("Unsupported MFA method type.");
            }
        }

        private async Task DisableTotpMethodAsync(User user, UserMfaMethod mfaMethod, string code)
        {
            var totpSecretBytes = encryptionService.Decrypt(
                mfaMethod.EncryptedSecret,
                mfaMethod.EncryptedDataKey,
                mfaMethod.SecretNonce,
                mfaMethod.DataKeyNonce);

            var totpSecret = Encoding.UTF8.GetString(totpSecretBytes.Value);

            var totp = new Totp(Base32Encoding.ToBytes(totpSecret));

            if (totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(1, 1)))
            {
                await mfaRepository.DeleteMfaMethodAsync(mfaMethod);
                return;
            }
            throw new ArgumentException("Invalid TOTP code. MFA method not disabled.");
        }

        private async Task<string> CreateTotpMfaMethodAsync(User user)
        {
            var secretKey = KeyGeneration.GenerateRandomKey(20);
            var totpSecret = Base32Encoding.ToString(secretKey);

            var secretBytes = Encoding.UTF8.GetBytes(totpSecret);
            var enc = encryptionService.Encrypt(secretBytes);

            var mfa = new UserMfaMethod
            {
                UserId = user.Id,
                User = user,
                Method = MfaMethodType.Totp,
                IsEnabled = false,
                EncryptedSecret = enc.EncryptedValue,
                SecretNonce = enc.ValueNonce,
                EncryptedDataKey = enc.EncryptedDataKey,
                DataKeyNonce = enc.DataKeyNonce
            };
            await mfaRepository.AddMfaMethodAsync(mfa);

            return totpSecret;
        }

        private string GenerateTotpBackupCode(int byteLength)
        {
            byte[] bytes = new byte[byteLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var code = BitConverter.ToString(bytes).Replace("-", "");
            return code.Substring(0, 4) + "-" + code.Substring(4, 4);
        }
    }
}
