using System;

namespace FlowBoard_Workspace.Models
{
    public class WorkspaceMember
    {
        public int MemberId { get; set; }
        public int WorkspaceId { get; set; }
        public Guid UserId { get; set; } // Links to User.UserId in Auth service
        public string Role { get; set; } = "MEMBER"; // ADMIN or MEMBER
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Workspace? Workspace { get; set; }
    }
}
