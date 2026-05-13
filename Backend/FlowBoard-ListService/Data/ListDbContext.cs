using Microsoft.EntityFrameworkCore;
using FlowBoard_ListService.Models;

namespace FlowBoard_ListService.Data
{
    /// <summary>
    /// EF Core DbContext for the List/Column microservice.
    /// Owns a single table: TaskLists.
    /// </summary>
    public class ListDbContext : DbContext
    {
        public ListDbContext(DbContextOptions<ListDbContext> options) : base(options)
        {
        }

        public DbSet<TaskList> TaskLists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskList>(entity =>
            {
                entity.HasKey(l => l.ListId);
                entity.Property(l => l.ListId).ValueGeneratedOnAdd();

                entity.Property(l => l.Name).IsRequired().HasMaxLength(255);

                // Ensure position uniqueness per board (soft constraint via application logic)
                entity.HasIndex(l => new { l.BoardId, l.Position });

                // Default values
                entity.Property(l => l.Color).HasDefaultValue("#6366F1");
                entity.Property(l => l.IsArchived).HasDefaultValue(false);
            });
        }
    }
}
