using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserMfaMethod> UserMfaMethods { get; set; }

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
        }
    }
}
