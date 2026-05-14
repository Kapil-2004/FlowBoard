using FlowBoard.Auth.Helpers;
using FlowBoard.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Auth.Data
{
    /// <summary>
    /// EF Core DbContext for the Auth Service database.
    /// Seeds a fixed PlatformAdmin account on startup.
    /// </summary>
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        /// <summary>Maps to the "users" table.</summary>
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Users table ──────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_users_email");

                entity.HasIndex(u => new { u.Provider, u.ProviderId })
                      .HasDatabaseName("IX_users_provider_provider_id");

                entity.Property(u => u.Provider).HasDefaultValue("LOCAL");
                entity.Property(u => u.Role).HasDefaultValue("Member");
                entity.Property(u => u.IsActive).HasDefaultValue(true);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // ── Seed: fixed Platform Admin account ───────────────────────────
            // Credentials: admin@flowboard.app / Admin@FlowBoard123
            // Change the password via PUT /api/admin/users/{id}/role after first login.
            var adminId = new Guid("00000000-0000-0000-0000-000000000001");
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId       = adminId,
                FullName     = "Platform Admin",
                Email        = "admin@flowboard.app",
                PasswordHash = PasswordHasher.Hash("Admin@FlowBoard123"),
                Provider     = "LOCAL",
                Role         = "PlatformAdmin",
                IsActive     = true,
                CreatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt    = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
