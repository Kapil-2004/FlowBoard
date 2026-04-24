using FlowBoard.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Auth.Data
{
    /// <summary>
    /// EF Core DbContext for the Auth Service database.
    /// Uses a dedicated PostgreSQL database ("flowboard_auth") to keep the
    /// auth domain isolated from other microservices.
    /// </summary>
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        /// <summary>Maps to the "users" table defined in the UC1 schema.</summary>
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Users table ──────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                // Email must be unique across all providers
                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_users_email");

                // Index on (provider, provider_id) for fast OAuth lookups
                entity.HasIndex(u => new { u.Provider, u.ProviderId })
                      .HasDatabaseName("IX_users_provider_provider_id");

                // Default provider is LOCAL
                entity.Property(u => u.Provider)
                      .HasDefaultValue("LOCAL");

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("NOW()");

                entity.Property(u => u.UpdatedAt)
                      .HasDefaultValueSql("NOW()");
            });
        }
    }
}
