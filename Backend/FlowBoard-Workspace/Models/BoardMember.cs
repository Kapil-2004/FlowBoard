using System;

namespace FlowBoard_Workspace.Models
{
    public class BoardMember
    {
        public int BoardMemberId { get; set; }
        public int BoardId { get; set; }
        public Guid UserId { get; set; } // Links to User.UserId in Auth service
        public string Role { get; set; } = "MEMBER"; // OBSERVER, MEMBER, or ADMIN
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Board? Board { get; set; }
    }
}
