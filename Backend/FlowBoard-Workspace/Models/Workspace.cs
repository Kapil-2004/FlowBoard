using System;
using System.Collections.Generic;

namespace FlowBoard_Workspace.Models
{
    public class Workspace
    {
        public int WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid OwnerId { get; set; } // Links to User.UserId in Auth service
        public string Visibility { get; set; } = "PRIVATE"; // PUBLIC or PRIVATE
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    }
}
