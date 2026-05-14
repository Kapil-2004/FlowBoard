using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_Board.DTOs;
using FlowBoard_Board.Models;
using FlowBoard_Board.Repositories;

namespace FlowBoard_Board.Services
{
    public class BoardServiceImpl : IBoardService
    {
        private readonly IBoardRepository _repository;

        public BoardServiceImpl(IBoardRepository repository)
        {
            _repository = repository;
        }

        public async Task<BoardDto> CreateBoardAsync(CreateBoardDto createDto, Guid currentUserId)
        {
            var board = new Board
            {
                WorkspaceId = createDto.WorkspaceId,
                Name = createDto.Name,
                Description = createDto.Description,
                Background = createDto.Background,
                Visibility = createDto.Visibility,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsClosed = false
            };

            var createdBoard = await _repository.CreateBoardAsync(board);

            // Add creator as ADMIN member
            var member = new BoardMember
            {
                BoardId = createdBoard.BoardId,
                UserId = currentUserId,
                Role = "ADMIN",
                AddedAt = DateTime.UtcNow
            };
            await _repository.AddMemberAsync(member);

            // Reload board to get members
            createdBoard = await _repository.FindByBoardIdAsync(createdBoard.BoardId);

            return MapToDto(createdBoard!);
        }

        public async Task<BoardDto?> GetBoardByIdAsync(int boardId, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) return null;

            // Simple authorization check: if private, must be member or creator
            if (board.Visibility == "PRIVATE" && board.CreatedById != currentUserId && !board.Members.Any(m => m.UserId == currentUserId))
            {
                throw new UnauthorizedAccessException("You do not have access to this board.");
            }

            return MapToDto(board);
        }

        public async Task<List<BoardDto>> GetBoardsByWorkspaceAsync(int workspaceId, Guid currentUserId)
        {
            var boards = await _repository.FindByWorkspaceIdAsync(workspaceId);
            
            // Filter by visibility and membership
            var visibleBoards = boards.Where(b => 
                b.Visibility == "PUBLIC" || 
                b.CreatedById == currentUserId || 
                b.Members.Any(m => m.UserId == currentUserId))
                .ToList();

            return visibleBoards.Select(MapToDto).ToList();
        }

        public async Task<List<BoardDto>> GetBoardsByMemberAsync(Guid userId)
        {
            var boards = await _repository.FindByMemberUserIdAsync(userId);
            return boards.Select(MapToDto).ToList();
        }

        public async Task<BoardDto> UpdateBoardAsync(int boardId, UpdateBoardDto updateDto, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) throw new KeyNotFoundException("Board not found.");

            if (!IsAdminOrCreator(board, currentUserId))
                throw new UnauthorizedAccessException("Only admins or the creator can update the board.");

            if (updateDto.Name != null) board.Name = updateDto.Name;
            if (updateDto.Description != null) board.Description = updateDto.Description;
            if (updateDto.Background != null) board.Background = updateDto.Background;
            if (updateDto.Visibility != null) board.Visibility = updateDto.Visibility;

            board.UpdatedAt = DateTime.UtcNow;

            var updatedBoard = await _repository.UpdateBoardAsync(board);
            return MapToDto(updatedBoard);
        }

        public async Task CloseBoardAsync(int boardId, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) throw new KeyNotFoundException("Board not found.");

            if (!IsAdminOrCreator(board, currentUserId))
                throw new UnauthorizedAccessException("Only admins or the creator can close the board.");

            board.IsClosed = true;
            board.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateBoardAsync(board);
        }

        public async Task DeleteBoardAsync(int boardId, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) throw new KeyNotFoundException("Board not found.");

            if (board.CreatedById != currentUserId)
                throw new UnauthorizedAccessException("Only the creator can delete the board.");

            await _repository.DeleteBoardAsync(boardId);
        }

        public async Task<BoardMemberDto> AddMemberAsync(int boardId, AddBoardMemberDto addDto, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) throw new KeyNotFoundException("Board not found.");

            if (!IsAdminOrCreator(board, currentUserId))
                throw new UnauthorizedAccessException("Only admins or the creator can add members.");

            var existingMember = await _repository.FindMemberAsync(boardId, addDto.UserId);
            if (existingMember != null) throw new InvalidOperationException("User is already a member of this board.");

            var member = new BoardMember
            {
                BoardId = boardId,
                UserId = addDto.UserId,
                Role = addDto.Role,
                AddedAt = DateTime.UtcNow
            };

            var addedMember = await _repository.AddMemberAsync(member);
            return new BoardMemberDto
            {
                BoardMemberId = addedMember.BoardMemberId,
                BoardId = addedMember.BoardId,
                UserId = addedMember.UserId,
                Role = addedMember.Role,
                AddedAt = addedMember.AddedAt
            };
        }

        public async Task RemoveMemberAsync(int boardId, Guid userIdToRemove, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) throw new KeyNotFoundException("Board not found.");

            var memberToRemove = await _repository.FindMemberAsync(boardId, userIdToRemove);
            if (memberToRemove == null) throw new KeyNotFoundException("Member not found on this board.");

            // A user can remove themselves, but otherwise must be an admin/creator
            if (userIdToRemove != currentUserId && !IsAdminOrCreator(board, currentUserId))
            {
                throw new UnauthorizedAccessException("Only admins or the creator can remove other members.");
            }

            if (board.CreatedById == userIdToRemove)
            {
                throw new InvalidOperationException("Cannot remove the creator from the board.");
            }

            await _repository.RemoveMemberAsync(memberToRemove.BoardMemberId);
        }

        public async Task UpdateMemberRoleAsync(int boardId, Guid userIdToUpdate, UpdateMemberRoleDto updateDto, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) throw new KeyNotFoundException("Board not found.");

            if (!IsAdminOrCreator(board, currentUserId))
                throw new UnauthorizedAccessException("Only admins or the creator can update member roles.");

            var memberToUpdate = await _repository.FindMemberAsync(boardId, userIdToUpdate);
            if (memberToUpdate == null) throw new KeyNotFoundException("Member not found on this board.");

            if (board.CreatedById == userIdToUpdate)
            {
                throw new InvalidOperationException("Cannot change the role of the creator.");
            }

            memberToUpdate.Role = updateDto.Role;
            await _repository.UpdateMemberRoleAsync(memberToUpdate);
        }

        public async Task<List<BoardMemberDto>> GetMembersAsync(int boardId, Guid currentUserId)
        {
            var board = await _repository.FindByBoardIdAsync(boardId);
            if (board == null) throw new KeyNotFoundException("Board not found.");

            if (board.Visibility == "PRIVATE" && board.CreatedById != currentUserId && !board.Members.Any(m => m.UserId == currentUserId))
            {
                throw new UnauthorizedAccessException("You do not have access to view members of this board.");
            }

            var members = await _repository.GetMembersAsync(boardId);
            return members.Select(m => new BoardMemberDto
            {
                BoardMemberId = m.BoardMemberId,
                BoardId = m.BoardId,
                UserId = m.UserId,
                Role = m.Role,
                AddedAt = m.AddedAt
            }).ToList();
        }

        // Admin Methods
        public async Task<List<BoardDto>> GetAllBoardsAsync()
        {
            var boards = await _repository.FindAllAsync();
            return boards.Select(MapToDto).ToList();
        }

        public async Task AdminDeleteBoardAsync(int id)
        {
            await _repository.DeleteBoardAsync(id);
        }

        private bool IsAdminOrCreator(Board board, Guid userId)
        {
            if (board.CreatedById == userId) return true;
            var member = board.Members.FirstOrDefault(m => m.UserId == userId);
            return member != null && member.Role == "ADMIN";
        }

        private BoardDto MapToDto(Board board)
        {
            return new BoardDto
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
                UpdatedAt = board.UpdatedAt,
                Members = board.Members.Select(m => new BoardMemberDto
                {
                    BoardMemberId = m.BoardMemberId,
                    BoardId = m.BoardId,
                    UserId = m.UserId,
                    Role = m.Role,
                    AddedAt = m.AddedAt
                }).ToList()
            };
        }
    }
}
