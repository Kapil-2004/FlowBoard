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
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }

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

            // Board Configuration
            modelBuilder.Entity<Board>(entity =>
            {
                entity.HasKey(e => e.BoardId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Visibility).IsRequired().HasDefaultValue("PRIVATE");
                entity.Property(e => e.Background).IsRequired().HasDefaultValue("#FFFFFF");
                entity.Property(e => e.CreatedById).IsRequired(); // No foreign key to Auth DB
                entity.Property(e => e.IsClosed).IsRequired().HasDefaultValue(false);

                entity.HasOne(d => d.Workspace)
                    .WithMany()
                    .HasForeignKey(d => d.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // BoardMember Configuration
            modelBuilder.Entity<BoardMember>(entity =>
            {
                entity.HasKey(e => e.BoardMemberId);
                entity.Property(e => e.Role).IsRequired().HasDefaultValue("MEMBER");
                entity.Property(e => e.UserId).IsRequired(); // No foreign key to Auth DB

                entity.HasOne(d => d.Board)
                    .WithMany(p => p.Members)
                    .HasForeignKey(d => d.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
