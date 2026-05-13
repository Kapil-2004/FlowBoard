using System;

namespace FlowBoard_ListService.Models
{
    /// <summary>
    /// Represents a vertical column (list) on a Kanban board.
    /// UC4 – List/Column-Service entity.
    /// BoardId is a logical foreign key that references the FlowBoard-Board service
    /// (cross-service reference – no navigation property since services own separate DBs).
    /// </summary>
    public class TaskList
    {
        public int ListId { get; set; }

        /// <summary>
        /// Logical reference to the parent board (lives in FlowBoard-Board service).
        /// No EF navigation property because boards and lists are in separate databases.
        /// </summary>
        public int BoardId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Left-to-right display order. 0-indexed, contiguous integers.</summary>
        public int Position { get; set; }

        /// <summary>Hex accent colour used to visually distinguish columns (e.g. "#6366F1").</summary>
        public string Color { get; set; } = "#6366F1";

        public bool IsArchived { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
