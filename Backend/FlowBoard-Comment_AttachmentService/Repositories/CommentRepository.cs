using FlowBoard_Comment_AttachmentService.Data;
using FlowBoard_Comment_AttachmentService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlowBoard_Comment_AttachmentService.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CommentDbContext _context;

        public CommentRepository(CommentDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> FindByCardIdAsync(int cardId)
        {
            return await _context.Comments
                .Where(c => c.CardId == cardId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> FindByAuthorIdAsync(int authorId)
        {
            return await _context.Comments
                .Where(c => c.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<Comment?> FindByCommentIdAsync(int commentId)
        {
            return await _context.Comments.FindAsync(commentId);
        }

        public async Task<IEnumerable<Comment>> FindByParentCommentIdAsync(int parentCommentId)
        {
            return await _context.Comments
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountByCardIdAsync(int cardId)
        {
            return await _context.Comments.CountAsync(c => c.CardId == cardId);
        }

        public async Task DeleteByCommentIdAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                comment.IsDeleted = true;
                comment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task UpdateAsync(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<Attachment> AddAttachmentAsync(Attachment attachment)
        {
            await _context.Attachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<IEnumerable<Attachment>> GetAttachmentsByCardIdAsync(int cardId)
        {
            return await _context.Attachments
                .Where(a => a.CardId == cardId)
                .ToListAsync();
        }

        public async Task<Attachment?> GetAttachmentByIdAsync(int attachmentId)
        {
            return await _context.Attachments.FindAsync(attachmentId);
        }

        public async Task DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment != null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
