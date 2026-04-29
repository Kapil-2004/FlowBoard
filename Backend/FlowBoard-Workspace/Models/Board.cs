using System;
using System.Collections.Generic;

namespace FlowBoard_Workspace.Models
{
    public class Board
    {
        public int BoardId { get; set; }
        public int WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Background { get; set; } = "#FFFFFF"; // Hex color or background URL
        public string Visibility { get; set; } = "PRIVATE"; // PRIVATE or PUBLIC
        public Guid CreatedById { get; set; } // Links to User.UserId in Auth service
        public bool IsClosed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Workspace? Workspace { get; set; }
        public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
    }
}
