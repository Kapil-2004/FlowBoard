using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_ListService.DTOs;
using FlowBoard_ListService.Models;
using FlowBoard_ListService.Repositories;

namespace FlowBoard_ListService.Services
{
    /// <summary>
    /// Business-logic implementation for UC4 – List/Column-Service.
    /// Handles creation, retrieval, renaming, reordering, archival,
    /// deletion, and cross-board movement of TaskLists.
    /// </summary>
    public class ListServiceImpl : IListService
    {
        private readonly IListRepository _repository;

        public ListServiceImpl(IListRepository repository)
        {
            _repository = repository;
        }

        // ── Create ───────────────────────────────────────────────────────────────

        public async Task<ListDto> CreateListAsync(CreateListDto createDto)
        {
            // New list is appended at the rightmost position (maxPosition + 1).
            var maxPosition = await _repository.FindMaxPositionByBoardIdAsync(createDto.BoardId);
            var newPosition = maxPosition.HasValue ? maxPosition.Value + 1 : 0;

            var list = new TaskList
            {
                BoardId    = createDto.BoardId,
                Name       = createDto.Name,
                Color      = createDto.Color,
                Position   = newPosition,
                IsArchived = false,
                CreatedAt  = DateTime.UtcNow,
                UpdatedAt  = DateTime.UtcNow
            };

            var created = await _repository.CreateListAsync(list);
            return MapToDto(created);
        }

        // ── Read ─────────────────────────────────────────────────────────────────

        public async Task<ListDto?> GetListByIdAsync(int listId)
        {
            var list = await _repository.FindByListIdAsync(listId);
            return list == null ? null : MapToDto(list);
        }

        public async Task<List<ListDto>> GetListsByBoardAsync(int boardId)
        {
            var lists = await _repository.FindByBoardIdOrderByPositionAsync(boardId);
            return lists.Select(MapToDto).ToList();
        }

        public async Task<List<ListDto>> GetArchivedListsAsync(int boardId)
        {
            var lists = await _repository.FindByBoardIdAndIsArchivedAsync(boardId, true);
            return lists.Select(MapToDto).ToList();
        }

        // ── Update ───────────────────────────────────────────────────────────────

        public async Task<ListDto> UpdateListAsync(int listId, UpdateListDto updateDto)
        {
            var list = await _repository.FindByListIdAsync(listId);
            if (list == null) throw new KeyNotFoundException("List not found.");

            // Apply partial updates – only patch fields that are provided.
            if (updateDto.Name  != null) list.Name  = updateDto.Name;
            if (updateDto.Color != null) list.Color = updateDto.Color;

            list.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateListAsync(list);
            return MapToDto(updated);
        }

        // ── Reorder ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Drag-and-drop position update.
        /// The caller sends the complete ordered list of active IDs for the board.
        /// We reassign Position = index and commit atomically via EF Core transaction.
        /// </summary>
        public async Task ReorderListsAsync(int boardId, List<int> orderedListIds)
        {
            var activeLists = await _repository.FindByBoardIdOrderByPositionAsync(boardId);

            // Guard: every supplied ID must belong to this board.
            var boardListIds = activeLists.Select(l => l.ListId).ToHashSet();
            if (!orderedListIds.All(id => boardListIds.Contains(id)))
                throw new InvalidOperationException(
                    "One or more list IDs do not belong to this board.");

            // Guard: the caller must supply every active list (no gaps allowed).
            if (orderedListIds.Count != activeLists.Count)
                throw new InvalidOperationException(
                    "Reorder payload must contain all active lists for this board.");

            // Assign new positions according to the supplied order.
            for (int i = 0; i < orderedListIds.Count; i++)
            {
                var list = activeLists.First(l => l.ListId == orderedListIds[i]);
                list.Position  = i;
                list.UpdatedAt = DateTime.UtcNow;
            }

            // Persist atomically (EF Core transaction replaces Spring @Transactional batch-save).
            await _repository.BatchUpdatePositionsAsync(activeLists);
        }

        // ── Archive / Unarchive ──────────────────────────────────────────────────

        public async Task ArchiveListAsync(int listId)
        {
            var list = await _repository.FindByListIdAsync(listId);
            if (list == null) throw new KeyNotFoundException("List not found.");

            list.IsArchived = true;
            list.UpdatedAt  = DateTime.UtcNow;
            await _repository.UpdateListAsync(list);
        }

        public async Task UnarchiveListAsync(int listId)
        {
            var list = await _repository.FindByListIdAsync(listId);
            if (list == null) throw new KeyNotFoundException("List not found.");

            list.IsArchived = false;
            list.UpdatedAt  = DateTime.UtcNow;
            await _repository.UpdateListAsync(list);
        }

        // ── Delete ───────────────────────────────────────────────────────────────

        public async Task DeleteListAsync(int listId)
        {
            var list = await _repository.FindByListIdAsync(listId);
            if (list == null) throw new KeyNotFoundException("List not found.");

            await _repository.DeleteByListIdAsync(listId);
        }

        // ── Move ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Moves a list to a different board.  It is appended at the end of the
        /// target board's active lists (maxPosition + 1).
        /// </summary>
        public async Task<ListDto> MoveListAsync(int listId, int targetBoardId)
        {
            var list = await _repository.FindByListIdAsync(listId);
            if (list == null) throw new KeyNotFoundException("List not found.");

            var maxPosition  = await _repository.FindMaxPositionByBoardIdAsync(targetBoardId);
            var newPosition  = maxPosition.HasValue ? maxPosition.Value + 1 : 0;

            list.BoardId   = targetBoardId;
            list.Position  = newPosition;
            list.UpdatedAt = DateTime.UtcNow;

            var moved = await _repository.UpdateListAsync(list);
            return MapToDto(moved);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static ListDto MapToDto(TaskList list) => new ListDto
        {
            ListId     = list.ListId,
            BoardId    = list.BoardId,
            Name       = list.Name,
            Position   = list.Position,
            Color      = list.Color,
            IsArchived = list.IsArchived,
            CreatedAt  = list.CreatedAt,
            UpdatedAt  = list.UpdatedAt
        };
    }
}
