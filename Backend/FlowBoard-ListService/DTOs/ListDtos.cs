using System;
using System.Collections.Generic;

namespace FlowBoard_ListService.DTOs
{
    /// <summary>Read-only representation of a TaskList returned by the API.</summary>
    public class ListDto
    {
        public int ListId { get; set; }
        public int BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>Payload used when creating a new list on a board.</summary>
    public class CreateListDto
    {
        public int BoardId { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>Optional hex accent colour; defaults to indigo #6366F1.</summary>
        public string Color { get; set; } = "#6366F1";
    }

    /// <summary>Payload for renaming or recolouring an existing list.</summary>
    public class UpdateListDto
    {
        public string? Name { get; set; }
        public string? Color { get; set; }
    }

    /// <summary>
    /// Payload for drag-and-drop reorder.
    /// ListIds must contain every active list ID for the board in the desired left-to-right order.
    /// </summary>
    public class ReorderListsDto
    {
        public List<int> ListIds { get; set; } = new List<int>();
    }

    /// <summary>Payload for moving a list from one board to another.</summary>
    public class MoveListDto
    {
        public int TargetBoardId { get; set; }
    }
}
