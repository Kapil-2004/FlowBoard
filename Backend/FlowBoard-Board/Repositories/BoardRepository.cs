using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FlowBoard_Board.Data;
using FlowBoard_Board.Models;

namespace FlowBoard_Board.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly BoardDbContext _context;

        public BoardRepository(BoardDbContext context)
        {
            _context = context;
        }

        public async Task<Board?> FindByBoardIdAsync(int boardId)
        {
            return await _context.Boards
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.BoardId == boardId);
        }

        public async Task<List<Board>> FindByWorkspaceIdAsync(int workspaceId)
        {
            return await _context.Boards
                .Include(b => b.Members)
                .Where(b => b.WorkspaceId == workspaceId)
                .ToListAsync();
        }

        public async Task<List<Board>> FindByCreatedByIdAsync(Guid userId)
        {
            return await _context.Boards
                .Include(b => b.Members)
                .Where(b => b.CreatedById == userId)
                .ToListAsync();
        }

        public async Task<List<Board>> FindByMemberUserIdAsync(Guid userId)
        {
            return await _context.Boards
                .Include(b => b.Members)
                .Where(b => b.Members.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<List<Board>> FindByVisibilityAsync(string visibility)
        {
            return await _context.Boards
                .Include(b => b.Members)
                .Where(b => b.Visibility == visibility)
                .ToListAsync();
        }

        public async Task<int> CountByWorkspaceIdAsync(int workspaceId)
        {
            return await _context.Boards
                .CountAsync(b => b.WorkspaceId == workspaceId);
        }

        public async Task<List<Board>> FindByIsClosedAsync(bool isClosed)
        {
            return await _context.Boards
                .Include(b => b.Members)
                .Where(b => b.IsClosed == isClosed)
                .ToListAsync();
        }

        public async Task<List<Board>> FindAllAsync()
        {
            return await _context.Boards
                .Include(b => b.Members)
                .ToListAsync();
        }

        public async Task<Board> CreateBoardAsync(Board board)
        {
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<Board> UpdateBoardAsync(Board board)
        {
            _context.Boards.Update(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task DeleteBoardAsync(int boardId)
        {
            var board = await _context.Boards.FindAsync(boardId);
            if (board != null)
            {
                _context.Boards.Remove(board);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BoardMember> AddMemberAsync(BoardMember member)
        {
            _context.BoardMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task RemoveMemberAsync(int boardMemberId)
        {
            var member = await _context.BoardMembers.FindAsync(boardMemberId);
            if (member != null)
            {
                _context.BoardMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BoardMember?> FindMemberAsync(int boardId, Guid userId)
        {
            return await _context.BoardMembers
                .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);
        }

        public async Task<BoardMember> UpdateMemberRoleAsync(BoardMember member)
        {
            _context.BoardMembers.Update(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<List<BoardMember>> GetMembersAsync(int boardId)
        {
            return await _context.BoardMembers
                .Where(m => m.BoardId == boardId)
                .ToListAsync();
        }
    }
}
