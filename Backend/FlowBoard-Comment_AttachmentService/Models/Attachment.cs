using System;
using System.ComponentModel.DataAnnotations;

namespace FlowBoard_Comment_AttachmentService.Models
{
    public class Attachment
    {
        [Key]
        public int AttachmentId { get; set; }
        public int CardId { get; set; }
        public int UploaderId { get; set; }
        [Required]
        public string FileName { get; set; } = string.Empty;
        [Required]
        public string FileUrl { get; set; } = string.Empty; // S3/Azure URL
        public string FileType { get; set; } = string.Empty;
        public long SizeKb { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
