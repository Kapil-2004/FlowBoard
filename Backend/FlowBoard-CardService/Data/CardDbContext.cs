using FlowBoard_CardService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_CardService.Data
{
    public class CardDbContext : DbContext
    {
        public CardDbContext(DbContextOptions<CardDbContext> options) : base(options) { }

        public DbSet<Card> Cards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>()
                .HasKey(c => c.CardId);

            modelBuilder.Entity<Card>()
                .HasIndex(c => c.ListId);
                
            modelBuilder.Entity<Card>()
                .HasIndex(c => c.BoardId);
                
            modelBuilder.Entity<Card>()
                .HasIndex(c => c.AssigneeId);
                
            modelBuilder.Entity<Card>()
                .HasIndex(c => new { c.ListId, c.Position });
        }
    }
}
