using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Interfaces
{
    public interface IBoardRepository
    {
        Task<Board?> FindByBoardIdAsync(int boardId);
        Task<IEnumerable<Board>> FindByWorkspaceIdAsync(int workspaceId);
        Task<IEnumerable<Board>> FindByCreatedByIdAsync(Guid userId);
        Task<IEnumerable<Board>> FindByMemberUserIdAsync(Guid userId);
        Task<IEnumerable<Board>> FindByVisibilityAsync(string visibility);
        Task<int> CountByWorkspaceIdAsync(int workspaceId);
        Task<IEnumerable<Board>> FindByIsClosedAsync(bool isClosed);
        Task<Board> CreateAsync(Board board);
        Task<Board> UpdateAsync(Board board);
        Task<bool> DeleteAsync(int boardId);
        Task SaveChangesAsync();

        // Member operations
        Task<BoardMember?> FindBoardMemberAsync(int boardId, Guid userId);
        Task<IEnumerable<BoardMember>> GetMembersAsync(int boardId);
        Task<BoardMember> AddMemberAsync(BoardMember member);
        Task<bool> RemoveMemberAsync(int boardMemberId);
        Task<BoardMember> UpdateMemberRoleAsync(int boardMemberId, string role);
    }
}
