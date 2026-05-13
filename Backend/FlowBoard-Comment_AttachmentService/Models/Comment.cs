using System;
using System.ComponentModel.DataAnnotations;

namespace FlowBoard_Comment_AttachmentService.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        public int CardId { get; set; }
        public int AuthorId { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation property for threading if needed, but we can manage via ParentCommentId
    }
}
