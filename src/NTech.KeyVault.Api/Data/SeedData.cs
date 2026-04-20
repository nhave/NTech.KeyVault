using NTech.KeyVault.Api.Services;

namespace NTech.KeyVault.Api.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(AppDbContext db, IServiceScope serviceScope, CancellationToken cancellationToken)
        {
            var configuration = serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
            var encryptionService = serviceScope.ServiceProvider.GetRequiredService<IEncryptionService>();

            //var defaultUser = configuration.GetSection("NTech:DefaultUser");

            //if (!db.Users.Any())
            //{
            //    var user = new User
            //    {
            //        Username = defaultUser["Username"]!,
            //        PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultUser["Password"]!),
            //        EmailLookupHash = encryptionService.CreateLookupHash(defaultUser["Email"]!),
            //    };

            //    var userInfo = new UserInfo
            //    {
            //        FirstName = defaultUser["FirstName"]!,
            //        LastName = defaultUser["LastName"]!,
            //        Email = defaultUser["Email"]!,
            //    };

            //    byte[] data = userInfo.Serialize();
            //    EncryptionResult result = encryptionService.Encrypt(data);

            //    user.EncryptedUserInfo = result.EncryptedValue;
            //    user.UserInfoNonce = result.ValueNonce;
            //    user.EncryptedDataKey = result.EncryptedDataKey;
            //    user.DataKeyNonce = result.DataKeyNonce;

            //    db.Users.Add(user);
            //    await db.SaveChangesAsync(cancellationToken);

            //    var userRole = new UserRole
            //    {
            //        UserId = user.Id,
            //        Role = Roles.SystemAdmin
            //    };

            //    db.UserRoles.Add(userRole);
            //    await db.SaveChangesAsync(cancellationToken);
            //}
        }
    }
}
