using System;
using System.ComponentModel.DataAnnotations;

namespace FlowBoard_LabelService.Models
{
    public class ChecklistItem
    {
        [Key]
        public int ItemId { get; set; }
        public int ChecklistId { get; set; }
        [Required]
        public string Text { get; set; }
        public bool IsCompleted { get; set; }
        public int? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
