using FlowBoard_Comment_AttachmentService.Models;
using FlowBoard_Comment_AttachmentService.Repositories;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlowBoard_Comment_AttachmentService.Services
{
    public class CommentServiceImpl : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CommentServiceImpl(ICommentRepository repository, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            var created = await _repository.AddAsync(comment);
            
            // Trigger Notification via MassTransit
            // await _publishEndpoint.Publish(new { CardId = created.CardId, AuthorId = created.AuthorId, Content = created.Content });
            
            return created;
        }

        public async Task<IEnumerable<Comment>> GetByCardAsync(int cardId)
        {
            return await _repository.FindByCardIdAsync(cardId);
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await _repository.FindByCommentIdAsync(commentId);
        }

        public async Task<IEnumerable<Comment>> GetRepliesAsync(int parentId)
        {
            return await _repository.FindByParentCommentIdAsync(parentId);
        }

        public async Task<Comment> UpdateCommentAsync(int id, string content)
        {
            var comment = await _repository.FindByCommentIdAsync(id);
            if (comment == null) throw new Exception("Comment not found");
            
            comment.Content = content;
            await _repository.UpdateAsync(comment);
            return comment;
        }

        public async Task DeleteCommentAsync(int id)
        {
            await _repository.DeleteByCommentIdAsync(id);
        }

        public async Task<Attachment> AddAttachmentAsync(Attachment attachment)
        {
            var created = await _repository.AddAttachmentAsync(attachment);
            
            // Trigger Notification for Attachment
            // await _publishEndpoint.Publish(new { CardId = created.CardId, UploaderId = created.UploaderId, FileName = created.FileName });
            
            return created;
        }

        public async Task<IEnumerable<Attachment>> GetAttachmentsByCardAsync(int cardId)
        {
            return await _repository.GetAttachmentsByCardIdAsync(cardId);
        }

        public async Task DeleteAttachmentAsync(int id)
        {
            await _repository.DeleteAttachmentAsync(id);
        }

        public async Task<int> GetCommentCountAsync(int cardId)
        {
            return await _repository.CountByCardIdAsync(cardId);
        }
    }
}
