using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Interfaces
{
    public interface IWorkspaceRepository
    {
        Task<Workspace> CreateWorkspaceAsync(Workspace workspace);
        Task<Workspace?> GetByIdAsync(int workspaceId);
        Task<IEnumerable<Workspace>> GetByOwnerIdAsync(Guid ownerId);
        Task<IEnumerable<Workspace>> GetByMemberUserIdAsync(Guid userId);
        Task<IEnumerable<Workspace>> GetByVisibilityAsync(string visibility);
        Task<bool> ExistsByNameAndOwnerIdAsync(string name, Guid ownerId);
        Task<int> CountByOwnerIdAsync(Guid ownerId);
        Task<IEnumerable<Workspace>> GetAllAsync();
        
        Task<Workspace> UpdateWorkspaceAsync(Workspace workspace);
        Task DeleteWorkspaceAsync(Workspace workspace);

        // Membership
        Task<WorkspaceMember> AddMemberAsync(WorkspaceMember member);
        Task RemoveMemberAsync(WorkspaceMember member);
        Task<WorkspaceMember?> GetMemberAsync(int workspaceId, Guid userId);
        Task<IEnumerable<WorkspaceMember>> GetMembersAsync(int workspaceId);
        Task<WorkspaceMember> UpdateMemberAsync(WorkspaceMember member);
    }
}
