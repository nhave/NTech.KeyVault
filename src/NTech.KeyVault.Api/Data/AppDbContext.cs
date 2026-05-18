using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NTech.KeyVault.Common.Enums;
using NTech.KeyVault.Common.Models.Database;
using System.Text.Json;

namespace NTech.KeyVault.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserMfaMethod> UserMfaMethods { get; set; }

        public DbSet<Application> Applications { get; set; }
        public DbSet<AppConfiguration> AppConfigurations { get; set; }
        public DbSet<AppSecret> AppSecrets { get; set; }
        public DbSet<VaultSecret> VaultSecrets { get; set; }

        public DbSet<PrincipalPermission> PrincipalPermissions { get; set; }

        public override int SaveChanges()
        {
            // Ensure timestamps are added before saving changes
            AddTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Ensure timestamps are added before saving changes asynchronously
            AddTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Sets the creation and update timestamps for tracked entities that derive from Common.Models.Database.Common
        /// when they are added or modified.
        /// </summary>
        /// <remarks>This method updates the CreatedAt property only when an entity is added, and always
        /// updates the UpdatedAt property when an entity is added or modified. The timestamps are set to the current
        /// UTC time. This ensures that timestamp fields are consistently maintained for auditing purposes.</remarks>
        private void AddTimestamps()
        {
            // Get all tracked entities that are of type Common.Models.Database.Common and are either added or modified
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is Common.Models.Database.Common && (x.State == EntityState.Added || x.State == EntityState.Modified));

            // Iterate through the filtered entities and set the CreatedAt and UpdatedAt timestamps accordingly
            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;

                if (entity.State == EntityState.Added)
                {
                    ((Common.Models.Database.Common)entity.Entity).CreatedAt = now;
                }
                ((Common.Models.Database.Common)entity.Entity).UpdatedAt = now;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Configure the database connection using the connection string from the configuration
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("PostgresDb"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region UserEntity
            // Configure unique indexes for the User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Id)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailLookupHash)
                .IsUnique();
            #endregion

            #region RefreshTokenEntity
            // Configure indexes for the RefreshToken entity
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Id)
                .IsUnique();
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.ExpiryDate);

            // Configure relationships for the RefreshToken entity
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);

            // Configure self-referencing relationship for token rotation in the RefreshToken entity
            modelBuilder.Entity<RefreshToken>()
                .HasOne(t => t.ReplacedByToken)
                .WithMany()
                .HasForeignKey(t => t.ReplacedByTokenHash)
                .HasPrincipalKey(t => t.TokenHash)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion

            #region UserRoleEntity
            // Configure relationships for the UserRole entity
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            // Configure the conversion for the Roles enum to string
            modelBuilder.Entity<UserRole>()
                .Property(u => u.Role)
                .HasConversion<string>();
            #endregion

            #region UserMfaMethodEntity
            // Configure relationships for the UserMfaMethod entity
            modelBuilder.Entity<UserMfaMethod>()
                .HasOne(mfa => mfa.User)
                .WithMany(u => u.MfaMethods)
                .HasForeignKey(mfa => mfa.UserId);

            // Configure the conversion for the MfaMethodType enum to string
            modelBuilder.Entity<UserMfaMethod>()
                .Property(mfa => mfa.Method)
                .HasConversion<string>();
            #endregion

            #region ApplicationEntity
            // Configure relationships for the Application entity
            modelBuilder.Entity<Application>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion

            #region AppConfigurationEntity
            // Configure unique index for the Application entity
            modelBuilder.Entity<AppConfiguration>()
                .HasIndex(a => new { a.ApplicationId , a.Version })
                .IsUnique();

            // Configure the relationship between AppConfiguration and Application with cascade delete
            modelBuilder.Entity<AppConfiguration>()
                .HasOne(ac => ac.Application)
                .WithMany()
                .HasForeignKey(ac => ac.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the relationship between AppConfiguration and User for the CreatedBy property with SetNull on delete
            modelBuilder.Entity<AppConfiguration>()
                .HasOne(ac => ac.CreatedBy)
                .WithMany()
                .HasForeignKey(ac => ac.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion

            #region AppSecretEntity
            // Configure unique index for the AppSecret entity to ensure one secret per application-name combination
            modelBuilder.Entity<AppSecret>()
                .HasIndex(x => new { x.ApplicationId, x.Name })
                .IsUnique();

            // Configure the relationship between AppSecret and Application with cascade delete
            modelBuilder.Entity<AppSecret>()
                .HasOne(ac => ac.Application)
                .WithMany()
                .HasForeignKey(ac => ac.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region VaultSecretEntity
            // Configure unique index for the VaultSecret entity to ensure one secret per application-entity combination
            modelBuilder.Entity<VaultSecret>()
                .HasIndex(x => new { x.ApplicationId, x.EntityId })
                .IsUnique();

            // Configure the relationship between VaultSecret and Application with cascade delete
            modelBuilder.Entity<VaultSecret>()
                .HasOne(ac => ac.Application)
                .WithMany()
                .HasForeignKey(ac => ac.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region PrincipalPermissionEntity
            // Configure the PrincipalPermission entity
            // Configure the conversion for the PrincipalType enum to string
            modelBuilder.Entity<PrincipalPermission>()
                .Property(p => p.PrincipalType)
                .HasConversion<string>();

            // Configure the conversion for the ResourceType enum to string
            modelBuilder.Entity<PrincipalPermission>()
                .Property(p => p.ResourceType)
                .HasConversion<string>();

            // Create a ValueComparer for the List<Permission> to ensure proper change tracking and equality comparison
            var comparer = new ValueComparer<List<Permission>>(
                (a, b) => a!.SequenceEqual(b!),                     // equality
                a => a.Aggregate(0, (h, v) => HashCode.Combine(h, v.GetHashCode())), // hash
                a => a.ToList()                                     // snapshot (deep copy)
            );

            // Configure the conversion for the Permissions list to a JSON string
            modelBuilder.Entity<PrincipalPermission>()
                .Property(p => p.Permissions)
                .HasConversion(
                    v => JsonSerializer.Serialize(
                        v.Select(p => p.ToString().Replace("_", ":")).ToList(),
                        (JsonSerializerOptions)null!
                    ),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!)!
                        .Select(s => Enum.Parse<Permission>(s.Replace(":", "_")))
                        .ToList()
                )
                .Metadata.SetValueComparer(comparer);
            #endregion
        }
    }
}
