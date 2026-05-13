using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlowBoard_LabelService.Models
{
    public class Checklist
    {
        [Key]
        public int ChecklistId { get; set; }
        public int CardId { get; set; }
        [Required]
        public string Title { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
    }
}
