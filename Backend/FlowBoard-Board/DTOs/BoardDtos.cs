using System;
using System.Collections.Generic;

namespace FlowBoard_Board.DTOs
{
    public class BoardDto
    {
        public int BoardId { get; set; }
        public int WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public Guid CreatedById { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BoardMemberDto> Members { get; set; } = new List<BoardMemberDto>();
    }

    public class CreateBoardDto
    {
        public int WorkspaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Background { get; set; } = "#FFFFFF";
        public string Visibility { get; set; } = "PRIVATE";
    }

    public class UpdateBoardDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Background { get; set; }
        public string? Visibility { get; set; }
    }

    public class BoardMemberDto
    {
        public int BoardMemberId { get; set; }
        public int BoardId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }

    public class AddBoardMemberDto
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = "MEMBER";
    }

    public class UpdateMemberRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }
}
