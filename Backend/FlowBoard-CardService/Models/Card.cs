using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowBoard_CardService.Models
{
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CardId { get; set; }
        public int ListId { get; set; }
        public int BoardId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = "MEDIUM"; // LOW/MEDIUM/HIGH/CRITICAL
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "TO_DO"; // TO_DO/IN_PROGRESS/IN_REVIEW/DONE
        
        public DateOnly? DueDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public int? AssigneeId { get; set; }
        public int CreatedById { get; set; }
        public bool IsArchived { get; set; }
        
        [MaxLength(50)]
        public string? CoverColor { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
