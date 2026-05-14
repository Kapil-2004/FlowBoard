using FlowBoard_NotificationService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options) { }

        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);
                entity.Property(n => n.NotificationId).UseIdentityColumn();
                entity.Property(n => n.Type).IsRequired().HasMaxLength(50);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
                entity.Property(n => n.RelatedType).HasMaxLength(20);
                // Index for fast per-recipient reads
                entity.HasIndex(n => n.RecipientId);
                entity.HasIndex(n => new { n.RecipientId, n.IsRead });
            });
        }
    }
}
