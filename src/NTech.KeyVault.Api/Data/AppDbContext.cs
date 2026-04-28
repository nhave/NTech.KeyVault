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

        public DbSet<PrincipalPermission> PrincipalPermissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Configure the database connection using the connection string from the configuration
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("PostgresDb"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            // Configure relationships for the UserRole entity
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            // Configure the conversion for the Roles enum to string
            modelBuilder.Entity<UserRole>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // Configure relationships for the UserMfaMethod entity
            modelBuilder.Entity<UserMfaMethod>()
                .HasOne(mfa => mfa.User)
                .WithMany(u => u.MfaMethods)
                .HasForeignKey(mfa => mfa.UserId);

            // Configure the conversion for the MfaMethodType enum to string
            modelBuilder.Entity<UserMfaMethod>()
                .Property(mfa => mfa.Method)
                .HasConversion<string>();

            // Configure relationships for the Application entity
            modelBuilder.Entity<Application>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure the one-to-one relationship between Application and AppConfiguration
            modelBuilder.Entity<AppConfiguration>()
                .HasIndex(a => a.ApplicationId)
                .IsUnique();

            // Configure the relationship between AppConfiguration and Application with cascade delete
            modelBuilder.Entity<AppConfiguration>()
                .HasOne(ac => ac.Application)
                .WithOne()
                .HasForeignKey<AppConfiguration>(ac => ac.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

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
        }
    }
}
