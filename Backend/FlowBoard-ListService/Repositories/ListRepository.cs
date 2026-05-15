using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_ListService.Data;
using FlowBoard_ListService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_ListService.Repositories
{
    /// <summary>
    /// EF Core implementation of IListRepository.
    /// Atomic position reorders use an explicit database transaction
    /// (equivalent to Spring Data JPA saveAll() inside @Transactional).
    /// </summary>
    public class ListRepository : IListRepository
    {
        private readonly ListDbContext _context;

        public ListRepository(ListDbContext context)
        {
            _context = context;
        }

        // ── Queries ─────────────────────────────────────────────────────────────

        public async Task<List<TaskList>> FindByBoardIdAsync(int boardId)
        {
            return await _context.TaskLists
                .Where(l => l.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<TaskList?> FindByListIdAsync(int listId)
        {
            return await _context.TaskLists.FindAsync(listId);
        }

        public async Task<List<TaskList>> FindByBoardIdOrderByPositionAsync(int boardId)
        {
            return await _context.TaskLists
                .Where(l => l.BoardId == boardId && !l.IsArchived)
                .OrderBy(l => l.Position)
                .ToListAsync();
        }

        public async Task<List<TaskList>> FindByBoardIdAndIsArchivedAsync(int boardId, bool isArchived)
        {
            return await _context.TaskLists
                .Where(l => l.BoardId == boardId && l.IsArchived == isArchived)
                .OrderBy(l => l.Position)
                .ToListAsync();
        }

        public async Task<int> CountByBoardIdAsync(int boardId)
        {
            return await _context.TaskLists
                .CountAsync(l => l.BoardId == boardId && !l.IsArchived);
        }

        public async Task<int?> FindMaxPositionByBoardIdAsync(int boardId)
        {
            var hasLists = await _context.TaskLists
                .AnyAsync(l => l.BoardId == boardId);

            if (!hasLists) return null;

            return await _context.TaskLists
                .Where(l => l.BoardId == boardId)
                .MaxAsync(l => l.Position);
        }

        // ── Commands ────────────────────────────────────────────────────────────

        public async Task<TaskList> CreateListAsync(TaskList list)
        {
            _context.TaskLists.Add(list);
            await _context.SaveChangesAsync();
            return list;
        }

        public async Task<TaskList> UpdateListAsync(TaskList list)
        {
            _context.TaskLists.Update(list);
            await _context.SaveChangesAsync();
            return list;
        }

        /// <summary>
        /// Commits updated Position values for all sibling lists in a single transaction.
        /// If any write fails the transaction is rolled back, keeping positions consistent.
        /// </summary>
        public async Task BatchUpdatePositionsAsync(List<TaskList> lists)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var list in lists)
                {
                    _context.TaskLists.Update(list);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteByListIdAsync(int listId)
        {
            var list = await _context.TaskLists.FindAsync(listId);
            if (list != null)
            {
                _context.TaskLists.Remove(list);
                await _context.SaveChangesAsync();
            }
        }
    }
}
