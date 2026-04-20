using NTech.KeyVault.Api.Services;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Core;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(AppDbContext db, IServiceScope serviceScope, CancellationToken cancellationToken)
        {
            var configuration = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
            var encryptionService = serviceScope.ServiceProvider.GetRequiredService<IEncryptionService>();

            var defaultUser = configuration.GetSection("NTech:DefaultUser");

            if (!db.Users.Any())
            {
                var userInfo = new UserInfo
                {
                    FullName = defaultUser["FullName"]!,
                    Email = defaultUser["Email"]!,
                };

                byte[] data = userInfo.Serialize();
                EncryptionResult result = encryptionService.Encrypt(data);

                var user = new User
                {
                    Username = defaultUser["Username"]!,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultUser["Password"]!),
                    EmailLookupHash = encryptionService.CreateLookupHash(defaultUser["Email"]!),
                    EncryptedUserInfo = result.EncryptedValue,
                    UserInfoNonce = result.ValueNonce,
                    EncryptedDataKey = result.EncryptedDataKey,
                    DataKeyNonce = result.DataKeyNonce
                };

                db.Users.Add(user);
                await db.SaveChangesAsync(cancellationToken);

                var userRole = new UserRole
                {
                    UserId = user.Id,
                    User = user,
                    Role = Roles.SystemAdmin
                };

                db.UserRoles.Add(userRole);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
