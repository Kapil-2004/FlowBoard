using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlowBoard_LabelService.Models
{
    public class Label
    {
        [Key]
        public int LabelId { get; set; }
        public int BoardId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Color { get; set; } // Hex
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<CardLabel> CardLabels { get; set; } = new List<CardLabel>();
    }
}
