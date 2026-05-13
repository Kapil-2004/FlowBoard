using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_ListService.DTOs;

namespace FlowBoard_ListService.Services
{
    /// <summary>
    /// Service contract for UC4 – List/Column-Service.
    /// Declares all list CRUD, position management, archival, and board-transfer operations.
    /// </summary>
    public interface IListService
    {
        /// <summary>Creates a new list appended at the rightmost position of the board.</summary>
        Task<ListDto> CreateListAsync(CreateListDto createDto);

        /// <summary>Returns a list by its primary key, or null if not found.</summary>
        Task<ListDto?> GetListByIdAsync(int listId);

        /// <summary>Returns all active (non-archived) lists for a board ordered by Position.</summary>
        Task<List<ListDto>> GetListsByBoardAsync(int boardId);

        /// <summary>Renames or recolours a list.</summary>
        Task<ListDto> UpdateListAsync(int listId, UpdateListDto updateDto);

        /// <summary>
        /// Drag-and-drop reorder: atomically reassigns Position values so they match
        /// the supplied ordered list of IDs. Uses EF Core batch update in a transaction.
        /// </summary>
        Task ReorderListsAsync(int boardId, List<int> orderedListIds);

        /// <summary>Soft-deletes a list by setting IsArchived = true.</summary>
        Task ArchiveListAsync(int listId);

        /// <summary>Restores an archived list back to active.</summary>
        Task UnarchiveListAsync(int listId);

        /// <summary>Permanently removes a list from the database.</summary>
        Task DeleteListAsync(int listId);

        /// <summary>Moves a list to a different board (appended at the end of target board).</summary>
        Task<ListDto> MoveListAsync(int listId, int targetBoardId);

        /// <summary>Returns only archived lists for a given board.</summary>
        Task<List<ListDto>> GetArchivedListsAsync(int boardId);
    }
}
