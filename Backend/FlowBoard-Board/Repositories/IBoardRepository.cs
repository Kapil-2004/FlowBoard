using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_Board.Models;

namespace FlowBoard_Board.Repositories
{
    public interface IBoardRepository
    {
        Task<Board?> FindByBoardIdAsync(int boardId);
        Task<List<Board>> FindByWorkspaceIdAsync(int workspaceId);
        Task<List<Board>> FindByCreatedByIdAsync(Guid userId);
        Task<List<Board>> FindByMemberUserIdAsync(Guid userId);
        Task<List<Board>> FindByVisibilityAsync(string visibility);
        Task<int> CountByWorkspaceIdAsync(int workspaceId);
        Task<List<Board>> FindByIsClosedAsync(bool isClosed);

        Task<Board> CreateBoardAsync(Board board);
        Task<Board> UpdateBoardAsync(Board board);
        Task DeleteBoardAsync(int boardId);

        Task<BoardMember> AddMemberAsync(BoardMember member);
        Task RemoveMemberAsync(int boardMemberId);
        Task<BoardMember?> FindMemberAsync(int boardId, Guid userId);
        Task<BoardMember> UpdateMemberRoleAsync(BoardMember member);
        Task<List<BoardMember>> GetMembersAsync(int boardId);
    }
}
