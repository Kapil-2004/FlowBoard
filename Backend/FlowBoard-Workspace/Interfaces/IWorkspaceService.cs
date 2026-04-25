using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_Workspace.DTOs;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Interfaces
{
    public interface IWorkspaceService
    {
        Task<WorkspaceResponseDto> CreateWorkspaceAsync(WorkspaceCreateDto dto, Guid ownerId);
        Task<WorkspaceResponseDto?> GetByIdAsync(int workspaceId, Guid requestingUserId);
        Task<IEnumerable<WorkspaceResponseDto>> GetByOwnerAsync(Guid ownerId);
        Task<IEnumerable<WorkspaceResponseDto>> GetByMemberAsync(Guid userId);
        Task<WorkspaceResponseDto> UpdateWorkspaceAsync(int workspaceId, WorkspaceCreateDto dto, Guid requestingUserId);
        Task DeleteWorkspaceAsync(int workspaceId, Guid requestingUserId);

        Task<WorkspaceMemberDto> AddMemberAsync(int workspaceId, Guid userIdToAdd, string role, Guid requestingUserId);
        Task RemoveMemberAsync(int workspaceId, Guid userIdToRemove, Guid requestingUserId);
        Task UpdateMemberRoleAsync(int workspaceId, Guid memberUserId, string newRole, Guid requestingUserId);
        Task<IEnumerable<WorkspaceMemberDto>> GetMembersAsync(int workspaceId, Guid requestingUserId);
    }
}
