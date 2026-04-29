using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FlowBoard_Workspace.Data;
using FlowBoard_Workspace.Interfaces;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly WorkspaceDbContext _context;

        public BoardRepository(WorkspaceDbContext context)
        {
            _context = context;
        }

        public async Task<Board?> FindByBoardIdAsync(int boardId)
        {
            return await _context.Boards
                .Include(b => b.Members)
                .FirstOrDefaultAsync(b => b.BoardId == boardId);
        }

        public async Task<IEnumerable<Board>> FindByWorkspaceIdAsync(int workspaceId)
        {
            return await _context.Boards
                .Where(b => b.WorkspaceId == workspaceId)
                .Include(b => b.Members)
                .ToListAsync();
        }

        public async Task<IEnumerable<Board>> FindByCreatedByIdAsync(Guid userId)
        {
            return await _context.Boards
                .Where(b => b.CreatedById == userId)
                .Include(b => b.Members)
                .ToListAsync();
        }

        public async Task<IEnumerable<Board>> FindByMemberUserIdAsync(Guid userId)
        {
            return await _context.Boards
                .Where(b => b.Members.Any(m => m.UserId == userId))
                .Include(b => b.Members)
                .ToListAsync();
        }

        public async Task<IEnumerable<Board>> FindByVisibilityAsync(string visibility)
        {
            return await _context.Boards
                .Where(b => b.Visibility == visibility)
                .Include(b => b.Members)
                .ToListAsync();
        }

        public async Task<int> CountByWorkspaceIdAsync(int workspaceId)
        {
            return await _context.Boards
                .CountAsync(b => b.WorkspaceId == workspaceId);
        }

        public async Task<IEnumerable<Board>> FindByIsClosedAsync(bool isClosed)
        {
            return await _context.Boards
                .Where(b => b.IsClosed == isClosed)
                .Include(b => b.Members)
                .ToListAsync();
        }

        public async Task<Board> CreateAsync(Board board)
        {
            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<Board> UpdateAsync(Board board)
        {
            _context.Boards.Update(board);
            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<bool> DeleteAsync(int boardId)
        {
            var board = await FindByBoardIdAsync(boardId);
            if (board == null) return false;

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Member operations
        public async Task<BoardMember?> FindBoardMemberAsync(int boardId, Guid userId)
        {
            return await _context.BoardMembers
                .FirstOrDefaultAsync(m => m.BoardId == boardId && m.UserId == userId);
        }

        public async Task<IEnumerable<BoardMember>> GetMembersAsync(int boardId)
        {
            return await _context.BoardMembers
                .Where(m => m.BoardId == boardId)
                .ToListAsync();
        }

        public async Task<BoardMember> AddMemberAsync(BoardMember member)
        {
            _context.BoardMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<bool> RemoveMemberAsync(int boardMemberId)
        {
            var member = await _context.BoardMembers.FindAsync(boardMemberId);
            if (member == null) return false;

            _context.BoardMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BoardMember> UpdateMemberRoleAsync(int boardMemberId, string role)
        {
            var member = await _context.BoardMembers.FindAsync(boardMemberId);
            if (member == null)
                throw new KeyNotFoundException($"Board member with ID {boardMemberId} not found.");

            member.Role = role;
            await _context.SaveChangesAsync();
            return member;
        }
    }
}
