using FlowBoard_Comment_AttachmentService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlowBoard_Comment_AttachmentService.Repositories
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> FindByCardIdAsync(int cardId);
        Task<IEnumerable<Comment>> FindByAuthorIdAsync(int authorId);
        Task<Comment?> FindByCommentIdAsync(int commentId);
        Task<IEnumerable<Comment>> FindByParentCommentIdAsync(int parentCommentId);
        Task<int> CountByCardIdAsync(int cardId);
        Task DeleteByCommentIdAsync(int commentId);
        Task<Comment> AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        
        // Attachment methods
        Task<Attachment> AddAttachmentAsync(Attachment attachment);
        Task<IEnumerable<Attachment>> GetAttachmentsByCardIdAsync(int cardId);
        Task<Attachment?> GetAttachmentByIdAsync(int attachmentId);
        Task DeleteAttachmentAsync(int attachmentId);
    }
}
