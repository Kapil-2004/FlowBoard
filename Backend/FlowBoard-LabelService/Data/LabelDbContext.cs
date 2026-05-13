using FlowBoard_LabelService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_LabelService.Data
{
    public class LabelDbContext : DbContext
    {
        public LabelDbContext(DbContextOptions<LabelDbContext> options) : base(options) { }

        public DbSet<Label> Labels { get; set; }
        public DbSet<CardLabel> CardLabels { get; set; }
        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CardLabel>()
                .HasKey(cl => new { cl.CardId, cl.LabelId });

            modelBuilder.Entity<CardLabel>()
                .HasOne(cl => cl.Label)
                .WithMany(l => l.CardLabels)
                .HasForeignKey(cl => cl.LabelId);
        }
    }
}
