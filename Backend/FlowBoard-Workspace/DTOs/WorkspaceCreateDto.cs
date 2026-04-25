using System;
using System.ComponentModel.DataAnnotations;

namespace FlowBoard_Workspace.DTOs
{
    public class WorkspaceCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [RegularExpression("PUBLIC|PRIVATE")]
        public string Visibility { get; set; } = "PRIVATE";
        
        public string? LogoUrl { get; set; }
    }
}
