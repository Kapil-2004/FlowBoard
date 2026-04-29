using System;

namespace FlowBoard_Workspace.DTOs
{
    public class BoardCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Background { get; set; } = "#FFFFFF";
        public string Visibility { get; set; } = "PRIVATE";
    }

    public class BoardUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
    }

    public class BoardResponseDto
    {
        public int BoardId { get; set; }
        public int WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Background { get; set; } = "#FFFFFF";
        public string Visibility { get; set; } = "PRIVATE";
        public Guid CreatedById { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BoardMemberDto
    {
        public int BoardMemberId { get; set; }
        public int BoardId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "MEMBER";
        public DateTime AddedAt { get; set; }
    }
}
