using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_ListService.Models;

namespace FlowBoard_ListService.Repositories
{
    /// <summary>
    /// Repository contract for TaskList persistence.
    /// All method signatures mirror the Java Spring Data JPA pattern from the spec
    /// but use async/await for .NET.
    /// </summary>
    public interface IListRepository
    {
        // ── Queries ─────────────────────────────────────────────────────────────

        /// <summary>Returns all lists (active + archived) belonging to a board.</summary>
        Task<List<TaskList>> FindByBoardIdAsync(int boardId);

        /// <summary>Finds a single list by its primary key; returns null if not found.</summary>
        Task<TaskList?> FindByListIdAsync(int listId);

        /// <summary>Returns active (non-archived) lists ordered by Position ascending.</summary>
        Task<List<TaskList>> FindByBoardIdOrderByPositionAsync(int boardId);

        /// <summary>Filters lists by IsArchived state and returns them ordered by Position.</summary>
        Task<List<TaskList>> FindByBoardIdAndIsArchivedAsync(int boardId, bool isArchived);

        /// <summary>Counts only active (non-archived) lists on a board.</summary>
        Task<int> CountByBoardIdAsync(int boardId);

        /// <summary>
        /// Returns the highest Position value among active lists on a board,
        /// or null when the board has no active lists.
        /// </summary>
        Task<int?> FindMaxPositionByBoardIdAsync(int boardId);

        // ── Commands ────────────────────────────────────────────────────────────

        Task<TaskList> CreateListAsync(TaskList list);
        Task<TaskList> UpdateListAsync(TaskList list);

        /// <summary>
        /// Atomically writes new Position values for a set of lists using an EF Core
        /// transaction scope (replaces Spring Data JPA batch-save pattern).
        /// </summary>
        Task BatchUpdatePositionsAsync(List<TaskList> lists);

        Task DeleteByListIdAsync(int listId);
    }
}
