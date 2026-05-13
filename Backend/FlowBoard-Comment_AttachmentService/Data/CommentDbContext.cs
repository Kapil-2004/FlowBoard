using FlowBoard_Comment_AttachmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_Comment_AttachmentService.Data
{
    public class CommentDbContext : DbContext
    {
        public CommentDbContext(DbContextOptions<CommentDbContext> options) : base(options) { }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filter for Soft Delete
            modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
            
            // Indexing for performance
            modelBuilder.Entity<Comment>().HasIndex(c => c.CardId);
            modelBuilder.Entity<Attachment>().HasIndex(a => a.CardId);
        }
    }
}
