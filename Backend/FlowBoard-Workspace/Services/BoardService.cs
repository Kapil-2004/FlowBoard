using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_Workspace.DTOs;
using FlowBoard_Workspace.Interfaces;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Services
{
    public class BoardServiceImpl : IBoardService
    {
        private readonly IBoardRepository _repository;

        public BoardServiceImpl(IBoardRepository repository)
        {
            _repository = repository;
        }

        private BoardResponseDto MapToDto(Board board)
        {
            return new BoardResponseDto
            {
                BoardId = board.BoardId,
                WorkspaceId = board.WorkspaceId,
                Name = board.Name,
                Description = board.Description,
                Background = board.Background,
                Visibility = board.Visibility,
                CreatedById = board.CreatedById,
                IsClosed = board.IsClosed,
                CreatedAt = board.CreatedAt,
                UpdatedAt = board.UpdatedAt
            };
        }

        private BoardMemberDto MapToDto(BoardMember member)
        {
            return new BoardMemberDto
            {
                BoardMemberId = member.BoardMemberId,
                BoardId = member.BoardId,
                UserId = member.UserId,
                Role = member.Role,
                AddedAt = member.AddedAt
            };
        }

        private bool CanAccessBoard(Board board, Guid userId)
        {
            // Creator always has access
            if (board.CreatedById == userId) return true;

            // Check visibility
            if (board.Visibility == "PUBLIC") return true;

            // Check board members
            return board.Members.Any(m => m.UserId == userId);
        }

        public async Task<BoardResponseDto> CreateBoardAsync(int workspaceId, BoardCreateDto dto, Guid createdById)
        {
            var board = new Board
            {
                WorkspaceId = workspaceId,
                Name = dto.Name,
                Description = dto.Description,
                Background = dto.Background,
                Visibility = dto.Visibility,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBoard = await _repository.CreateAsync(board);
            
            // Add creator as ADMIN member
            var memberRole = new BoardMember
            {
                BoardId = createdBoard.BoardId,
                UserId = createdById,
                Role = "ADMIN"
            };
            await _repository.AddMemberAsync(memberRole);

            createdBoard = await _repository.FindByBoardIdAsync(createdBoard.BoardId) 
                ?? throw new Exception("Failed to retrieve created board.");
            
            return MapToDto(createdBoard);
        }

        public async Task<BoardResponseDto> GetBoardByIdAsync(int boardId, Guid userId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new KeyNotFoundException($"Board with ID {boardId} not found.");

            if (!CanAccessBoard(board, userId))
                throw new UnauthorizedAccessException("You do not have access to this board.");

            return MapToDto(board);
        }

        public async Task<IEnumerable<BoardResponseDto>> GetBoardsByWorkspaceAsync(int workspaceId, Guid userId)
        {
            var boards = await _repository.FindByWorkspaceIdAsync(workspaceId);
            var accessibleBoards = boards.Where(b => CanAccessBoard(b, userId));
            return accessibleBoards.Select(MapToDto);
        }

        public async Task<IEnumerable<BoardResponseDto>> GetBoardsByMemberAsync(Guid userId)
        {
            var boards = await _repository.FindByMemberUserIdAsync(userId);
            return boards.Select(MapToDto);
        }

        public async Task<BoardResponseDto> UpdateBoardAsync(int boardId, BoardUpdateDto dto, Guid userId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new KeyNotFoundException($"Board with ID {boardId} not found.");

            // Only creator or ADMIN members can update
            var isAdmin = board.CreatedById == userId || 
                         board.Members.Any(m => m.UserId == userId && m.Role == "ADMIN");
            
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only admins can update this board.");

            if (!string.IsNullOrEmpty(dto.Name)) board.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) board.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Background)) board.Background = dto.Background;
            if (!string.IsNullOrEmpty(dto.Visibility)) board.Visibility = dto.Visibility;
            
            board.UpdatedAt = DateTime.UtcNow;
            
            var updatedBoard = await _repository.UpdateAsync(board);
            updatedBoard = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new Exception("Failed to retrieve updated board.");
            
            return MapToDto(updatedBoard);
        }

        public async Task<BoardResponseDto> CloseBoardAsync(int boardId, Guid userId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new KeyNotFoundException($"Board with ID {boardId} not found.");

            // Only creator can close
            if (board.CreatedById != userId)
                throw new UnauthorizedAccessException("Only the board creator can close it.");

            board.IsClosed = true;
            board.UpdatedAt = DateTime.UtcNow;
            
            var closedBoard = await _repository.UpdateAsync(board);
            closedBoard = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new Exception("Failed to retrieve closed board.");
            
            return MapToDto(closedBoard);
        }

        public async Task<bool> DeleteBoardAsync(int boardId, Guid userId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new KeyNotFoundException($"Board with ID {boardId} not found.");

            // Only creator can delete
            if (board.CreatedById != userId)
                throw new UnauthorizedAccessException("Only the board creator can delete it.");

            return await _repository.DeleteAsync(boardId);
        }

        public async Task<BoardMemberDto> AddMemberAsync(int boardId, Guid userId, string role, Guid requestingUser)
        {
            var board = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new KeyNotFoundException($"Board with ID {boardId} not found.");

            // Only creator or ADMIN can add members
            var isAdmin = board.CreatedById == requestingUser || 
                         board.Members.Any(m => m.UserId == requestingUser && m.Role == "ADMIN");
            
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only admins can add members.");

            var existingMember = await _repository.FindBoardMemberAsync(boardId, userId);
            if (existingMember != null)
                throw new InvalidOperationException("User is already a member of this board.");

            var member = new BoardMember
            {
                BoardId = boardId,
                UserId = userId,
                Role = role
            };

            var addedMember = await _repository.AddMemberAsync(member);
            return MapToDto(addedMember);
        }

        public async Task<bool> RemoveMemberAsync(int boardMemberId, Guid requestingUser)
        {
            var member = await _repository.GetMembersAsync(0);
            var targetMember = member.FirstOrDefault(m => m.BoardMemberId == boardMemberId);
            
            if (targetMember == null)
                throw new KeyNotFoundException($"Board member with ID {boardMemberId} not found.");

            var board = await _repository.FindByBoardIdAsync(targetMember.BoardId) 
                ?? throw new KeyNotFoundException($"Board with ID {targetMember.BoardId} not found.");

            // Only creator or ADMIN can remove members
            var isAdmin = board.CreatedById == requestingUser || 
                         board.Members.Any(m => m.UserId == requestingUser && m.Role == "ADMIN");
            
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only admins can remove members.");

            return await _repository.RemoveMemberAsync(boardMemberId);
        }

        public async Task<BoardMemberDto> UpdateMemberRoleAsync(int boardMemberId, string role, Guid requestingUser)
        {
            var member = await _repository.GetMembersAsync(0);
            var targetMember = member.FirstOrDefault(m => m.BoardMemberId == boardMemberId);
            
            if (targetMember == null)
                throw new KeyNotFoundException($"Board member with ID {boardMemberId} not found.");

            var board = await _repository.FindByBoardIdAsync(targetMember.BoardId) 
                ?? throw new KeyNotFoundException($"Board with ID {targetMember.BoardId} not found.");

            // Only creator or ADMIN can update roles
            var isAdmin = board.CreatedById == requestingUser || 
                         board.Members.Any(m => m.UserId == requestingUser && m.Role == "ADMIN");
            
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only admins can update member roles.");

            var updatedMember = await _repository.UpdateMemberRoleAsync(boardMemberId, role);
            return MapToDto(updatedMember);
        }

        public async Task<IEnumerable<BoardMemberDto>> GetMembersAsync(int boardId, Guid userId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId) 
                ?? throw new KeyNotFoundException($"Board with ID {boardId} not found.");

            if (!CanAccessBoard(board, userId))
                throw new UnauthorizedAccessException("You do not have access to this board.");

            var members = await _repository.GetMembersAsync(boardId);
            return members.Select(MapToDto);
        }
    }
}
