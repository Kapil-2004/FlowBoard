using System;

namespace FlowBoard_Workspace.DTOs
{
    public class WorkspaceResponseDto
    {
        public int WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public string Visibility { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
