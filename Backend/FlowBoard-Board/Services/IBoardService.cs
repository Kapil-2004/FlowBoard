using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_Board.DTOs;

namespace FlowBoard_Board.Services
{
    public interface IBoardService
    {
        Task<BoardDto> CreateBoardAsync(CreateBoardDto createDto, Guid currentUserId);
        Task<BoardDto?> GetBoardByIdAsync(int boardId, Guid currentUserId);
        Task<List<BoardDto>> GetBoardsByWorkspaceAsync(int workspaceId, Guid currentUserId);
        Task<List<BoardDto>> GetBoardsByMemberAsync(Guid userId);
        Task<BoardDto> UpdateBoardAsync(int boardId, UpdateBoardDto updateDto, Guid currentUserId);
        Task CloseBoardAsync(int boardId, Guid currentUserId);
        Task DeleteBoardAsync(int boardId, Guid currentUserId);

        Task<BoardMemberDto> AddMemberAsync(int boardId, AddBoardMemberDto addDto, Guid currentUserId);
        Task RemoveMemberAsync(int boardId, Guid userIdToRemove, Guid currentUserId);
        Task UpdateMemberRoleAsync(int boardId, Guid userIdToUpdate, UpdateMemberRoleDto updateDto, Guid currentUserId);
        Task<List<BoardMemberDto>> GetMembersAsync(int boardId, Guid currentUserId);
        
        // Admin Methods
        Task<List<BoardDto>> GetAllBoardsAsync();
        Task AdminDeleteBoardAsync(int id);
    }
}
