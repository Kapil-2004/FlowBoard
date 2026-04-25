using Microsoft.EntityFrameworkCore;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Data
{
    public class WorkspaceDbContext : DbContext
    {
        public WorkspaceDbContext(DbContextOptions<WorkspaceDbContext> options) : base(options)
        {
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Workspace Configuration
            modelBuilder.Entity<Workspace>(entity =>
            {
                entity.HasKey(e => e.WorkspaceId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Visibility).IsRequired().HasDefaultValue("PRIVATE");
                entity.Property(e => e.OwnerId).IsRequired(); // No foreign key to Auth DB
            });

            // WorkspaceMember Configuration
            modelBuilder.Entity<WorkspaceMember>(entity =>
            {
                entity.HasKey(e => e.MemberId);
                entity.Property(e => e.Role).IsRequired().HasDefaultValue("MEMBER");
                entity.Property(e => e.UserId).IsRequired(); // No foreign key to Auth DB

                entity.HasOne(d => d.Workspace)
                    .WithMany(p => p.Members)
                    .HasForeignKey(d => d.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
