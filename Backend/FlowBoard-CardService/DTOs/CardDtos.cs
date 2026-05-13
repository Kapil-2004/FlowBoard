using System;
using System.Collections.Generic;

namespace FlowBoard_CardService.DTOs
{
    public class CardDto
    {
        public int CardId { get; set; }
        public int ListId { get; set; }
        public int BoardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateOnly? DueDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public int? AssigneeId { get; set; }
        public int CreatedById { get; set; }
        public bool IsArchived { get; set; }
        public string? CoverColor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCardDto
    {
        public int ListId { get; set; }
        public int BoardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = "MEDIUM";
        public string Status { get; set; } = "TO_DO";
        public DateOnly? DueDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public int? AssigneeId { get; set; }
        public string? CoverColor { get; set; }
    }

    public class UpdateCardDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateOnly? StartDate { get; set; }
        public string? CoverColor { get; set; }
    }

    public class MoveCardDto
    {
        public int TargetListId { get; set; }
        public int TargetPosition { get; set; }
    }

    public class ReorderCardsDto
    {
        public List<int> CardIds { get; set; } = new List<int>();
    }

    public class AssignCardDto
    {
        public int? AssigneeId { get; set; }
    }
}
