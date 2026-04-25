using System;

namespace FlowBoard_Workspace.DTOs
{
    public class WorkspaceMemberDto
    {
        public int MemberId { get; set; }
        public int WorkspaceId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
