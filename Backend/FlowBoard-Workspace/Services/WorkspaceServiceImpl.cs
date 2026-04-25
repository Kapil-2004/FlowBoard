using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowBoard_Workspace.DTOs;
using FlowBoard_Workspace.Interfaces;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Services
{
    public class WorkspaceServiceImpl : IWorkspaceService
    {
        private readonly IWorkspaceRepository _repository;

        public WorkspaceServiceImpl(IWorkspaceRepository repository)
        {
            _repository = repository;
        }

        private WorkspaceResponseDto MapToDto(Workspace workspace)
        {
            return new WorkspaceResponseDto
            {
                WorkspaceId = workspace.WorkspaceId,
                Name = workspace.Name,
                Description = workspace.Description,
                OwnerId = workspace.OwnerId,
                Visibility = workspace.Visibility,
                LogoUrl = workspace.LogoUrl,
                CreatedAt = workspace.CreatedAt,
                UpdatedAt = workspace.UpdatedAt
            };
        }

        private WorkspaceMemberDto MapToDto(WorkspaceMember member)
        {
            return new WorkspaceMemberDto
            {
                MemberId = member.MemberId,
                WorkspaceId = member.WorkspaceId,
                UserId = member.UserId,
                Role = member.Role,
                JoinedAt = member.JoinedAt
            };
        }

        public async Task<WorkspaceResponseDto> CreateWorkspaceAsync(WorkspaceCreateDto dto, Guid ownerId)
        {
            if (await _repository.ExistsByNameAndOwnerIdAsync(dto.Name, ownerId))
                throw new InvalidOperationException("Workspace with this name already exists for this owner.");

            var workspace = new Workspace
            {
                Name = dto.Name,
                Description = dto.Description,
                Visibility = dto.Visibility,
                LogoUrl = dto.LogoUrl,
                OwnerId = ownerId
            };

            var created = await _repository.CreateWorkspaceAsync(workspace);

            // Add owner as ADMIN member
            await _repository.AddMemberAsync(new WorkspaceMember
            {
                WorkspaceId = created.WorkspaceId,
                UserId = ownerId,
                Role = "ADMIN"
            });

            return MapToDto(created);
        }

        public async Task<WorkspaceResponseDto?> GetByIdAsync(int workspaceId, Guid requestingUserId)
        {
            var workspace = await _repository.GetByIdAsync(workspaceId);
            if (workspace == null) return null;

            if (workspace.Visibility == "PRIVATE")
            {
                var member = await _repository.GetMemberAsync(workspaceId, requestingUserId);
                if (member == null && workspace.OwnerId != requestingUserId)
                    throw new UnauthorizedAccessException("You do not have access to this workspace.");
            }

            return MapToDto(workspace);
        }

        public async Task<IEnumerable<WorkspaceResponseDto>> GetByOwnerAsync(Guid ownerId)
        {
            var workspaces = await _repository.GetByOwnerIdAsync(ownerId);
            return workspaces.Select(MapToDto);
        }

        public async Task<IEnumerable<WorkspaceResponseDto>> GetByMemberAsync(Guid userId)
        {
            var workspaces = await _repository.GetByMemberUserIdAsync(userId);
            return workspaces.Select(MapToDto);
        }

        public async Task<WorkspaceResponseDto> UpdateWorkspaceAsync(int workspaceId, WorkspaceCreateDto dto, Guid requestingUserId)
        {
            var workspace = await _repository.GetByIdAsync(workspaceId);
            if (workspace == null) throw new KeyNotFoundException("Workspace not found.");

            // Only Owner or Admin can update
            if (workspace.OwnerId != requestingUserId)
            {
                var member = await _repository.GetMemberAsync(workspaceId, requestingUserId);
                if (member == null || member.Role != "ADMIN")
                    throw new UnauthorizedAccessException("Only admins can update workspace.");
            }

            workspace.Name = dto.Name;
            workspace.Description = dto.Description;
            workspace.Visibility = dto.Visibility;
            workspace.LogoUrl = dto.LogoUrl;
            workspace.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateWorkspaceAsync(workspace);
            return MapToDto(updated);
        }

        public async Task DeleteWorkspaceAsync(int workspaceId, Guid requestingUserId)
        {
            var workspace = await _repository.GetByIdAsync(workspaceId);
            if (workspace == null) throw new KeyNotFoundException("Workspace not found.");

            // Only Owner can delete
            if (workspace.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the owner can delete the workspace.");

            await _repository.DeleteWorkspaceAsync(workspace);
        }

        public async Task<WorkspaceMemberDto> AddMemberAsync(int workspaceId, Guid userIdToAdd, string role, Guid requestingUserId)
        {
            var workspace = await _repository.GetByIdAsync(workspaceId);
            if (workspace == null) throw new KeyNotFoundException("Workspace not found.");

            // Only Owner or Admin can add
            if (workspace.OwnerId != requestingUserId)
            {
                var requester = await _repository.GetMemberAsync(workspaceId, requestingUserId);
                if (requester == null || requester.Role != "ADMIN")
                    throw new UnauthorizedAccessException("Only admins can add members.");
            }

            var existingMember = await _repository.GetMemberAsync(workspaceId, userIdToAdd);
            if (existingMember != null) throw new InvalidOperationException("User is already a member.");

            var newMember = new WorkspaceMember
            {
                WorkspaceId = workspaceId,
                UserId = userIdToAdd,
                Role = role
            };

            var added = await _repository.AddMemberAsync(newMember);
            return MapToDto(added);
        }

        public async Task RemoveMemberAsync(int workspaceId, Guid userIdToRemove, Guid requestingUserId)
        {
            var workspace = await _repository.GetByIdAsync(workspaceId);
            if (workspace == null) throw new KeyNotFoundException("Workspace not found.");

            if (workspace.OwnerId == userIdToRemove)
                throw new InvalidOperationException("Cannot remove the owner of the workspace.");

            // Only Owner, Admin, or the user themselves can remove
            if (requestingUserId != userIdToRemove && workspace.OwnerId != requestingUserId)
            {
                var requester = await _repository.GetMemberAsync(workspaceId, requestingUserId);
                if (requester == null || requester.Role != "ADMIN")
                    throw new UnauthorizedAccessException("Only admins can remove other members.");
            }

            var memberToRemove = await _repository.GetMemberAsync(workspaceId, userIdToRemove);
            if (memberToRemove == null) throw new KeyNotFoundException("Member not found.");

            await _repository.RemoveMemberAsync(memberToRemove);
        }

        public async Task UpdateMemberRoleAsync(int workspaceId, Guid memberUserId, string newRole, Guid requestingUserId)
        {
            var workspace = await _repository.GetByIdAsync(workspaceId);
            if (workspace == null) throw new KeyNotFoundException("Workspace not found.");

            // Only Owner or Admin can update roles
            if (workspace.OwnerId != requestingUserId)
            {
                var requester = await _repository.GetMemberAsync(workspaceId, requestingUserId);
                if (requester == null || requester.Role != "ADMIN")
                    throw new UnauthorizedAccessException("Only admins can update member roles.");
            }

            if (workspace.OwnerId == memberUserId)
                throw new InvalidOperationException("Cannot change the role of the workspace owner.");

            var memberToUpdate = await _repository.GetMemberAsync(workspaceId, memberUserId);
            if (memberToUpdate == null) throw new KeyNotFoundException("Member not found.");

            memberToUpdate.Role = newRole;
            await _repository.UpdateMemberAsync(memberToUpdate);
        }

        public async Task<IEnumerable<WorkspaceMemberDto>> GetMembersAsync(int workspaceId, Guid requestingUserId)
        {
            var workspace = await _repository.GetByIdAsync(workspaceId);
            if (workspace == null) throw new KeyNotFoundException("Workspace not found.");

            // Check access
            if (workspace.Visibility == "PRIVATE" && workspace.OwnerId != requestingUserId)
            {
                var member = await _repository.GetMemberAsync(workspaceId, requestingUserId);
                if (member == null) throw new UnauthorizedAccessException("You do not have access to this workspace.");
            }

            var members = await _repository.GetMembersAsync(workspaceId);
            return members.Select(MapToDto);
        }
    }
}
