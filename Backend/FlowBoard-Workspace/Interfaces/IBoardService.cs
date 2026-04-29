using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_Workspace.DTOs;

namespace FlowBoard_Workspace.Interfaces
{
    public interface IBoardService
    {
        Task<BoardResponseDto> CreateBoardAsync(int workspaceId, BoardCreateDto dto, Guid createdById);
        Task<BoardResponseDto> GetBoardByIdAsync(int boardId, Guid userId);
        Task<IEnumerable<BoardResponseDto>> GetBoardsByWorkspaceAsync(int workspaceId, Guid userId);
        Task<IEnumerable<BoardResponseDto>> GetBoardsByMemberAsync(Guid userId);
        Task<BoardResponseDto> UpdateBoardAsync(int boardId, BoardUpdateDto dto, Guid userId);
        Task<BoardResponseDto> CloseBoardAsync(int boardId, Guid userId);
        Task<bool> DeleteBoardAsync(int boardId, Guid userId);
        Task<BoardMemberDto> AddMemberAsync(int boardId, Guid userId, string role, Guid requestingUser);
        Task<bool> RemoveMemberAsync(int boardMemberId, Guid requestingUser);
        Task<BoardMemberDto> UpdateMemberRoleAsync(int boardMemberId, string role, Guid requestingUser);
        Task<IEnumerable<BoardMemberDto>> GetMembersAsync(int boardId, Guid userId);
    }
}
