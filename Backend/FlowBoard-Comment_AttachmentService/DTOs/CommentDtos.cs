using System;

namespace FlowBoard_Comment_AttachmentService.DTOs
{
    public class CreateCommentDto
    {
        public int CardId { get; set; }
        public int AuthorId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
    }

    public class UpdateCommentDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class CommentDto
    {
        public int CommentId { get; set; }
        public int CardId { get; set; }
        public int AuthorId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AttachmentDto
    {
        public int AttachmentId { get; set; }
        public int CardId { get; set; }
        public int UploaderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long SizeKb { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
