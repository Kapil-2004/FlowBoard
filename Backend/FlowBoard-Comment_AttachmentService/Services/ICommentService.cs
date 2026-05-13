using FlowBoard_Comment_AttachmentService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlowBoard_Comment_AttachmentService.Services
{
    public interface ICommentService
    {
        Task<Comment> AddCommentAsync(Comment comment);
        Task<IEnumerable<Comment>> GetByCardAsync(int cardId);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<IEnumerable<Comment>> GetRepliesAsync(int parentId);
        Task<Comment> UpdateCommentAsync(int id, string content);
        Task DeleteCommentAsync(int id);
        
        Task<Attachment> AddAttachmentAsync(Attachment attachment);
        Task<IEnumerable<Attachment>> GetAttachmentsByCardAsync(int cardId);
        Task DeleteAttachmentAsync(int id);
        Task<int> GetCommentCountAsync(int cardId);
    }
}
