using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FlowBoard_Workspace.Data;
using FlowBoard_Workspace.Interfaces;
using FlowBoard_Workspace.Models;

namespace FlowBoard_Workspace.Repositories
{
    public class WorkspaceRepository : IWorkspaceRepository
    {
        private readonly WorkspaceDbContext _context;

        public WorkspaceRepository(WorkspaceDbContext context)
        {
            _context = context;
        }

        public async Task<Workspace> CreateWorkspaceAsync(Workspace workspace)
        {
            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task<Workspace?> GetByIdAsync(int workspaceId)
        {
            return await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.WorkspaceId == workspaceId);
        }

        public async Task<IEnumerable<Workspace>> GetByOwnerIdAsync(Guid ownerId)
        {
            return await _context.Workspaces
                .Where(w => w.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Workspace>> GetByMemberUserIdAsync(Guid userId)
        {
            return await _context.WorkspaceMembers
                .Where(wm => wm.UserId == userId)
                .Select(wm => wm.Workspace!)
                .ToListAsync();
        }

        public async Task<IEnumerable<Workspace>> GetByVisibilityAsync(string visibility)
        {
            return await _context.Workspaces
                .Where(w => w.Visibility == visibility)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAndOwnerIdAsync(string name, Guid ownerId)
        {
            return await _context.Workspaces
                .AnyAsync(w => w.Name == name && w.OwnerId == ownerId);
        }

        public async Task<int> CountByOwnerIdAsync(Guid ownerId)
        {
            return await _context.Workspaces
                .CountAsync(w => w.OwnerId == ownerId);
        }

        public async Task<Workspace> UpdateWorkspaceAsync(Workspace workspace)
        {
            _context.Workspaces.Update(workspace);
            await _context.SaveChangesAsync();
            return workspace;
        }

        public async Task DeleteWorkspaceAsync(Workspace workspace)
        {
            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
        }

        public async Task<WorkspaceMember> AddMemberAsync(WorkspaceMember member)
        {
            _context.WorkspaceMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task RemoveMemberAsync(WorkspaceMember member)
        {
            _context.WorkspaceMembers.Remove(member);
            await _context.SaveChangesAsync();
        }

        public async Task<WorkspaceMember?> GetMemberAsync(int workspaceId, Guid userId)
        {
            return await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);
        }

        public async Task<IEnumerable<WorkspaceMember>> GetMembersAsync(int workspaceId)
        {
            return await _context.WorkspaceMembers
                .Where(wm => wm.WorkspaceId == workspaceId)
                .ToListAsync();
        }

        public async Task<WorkspaceMember> UpdateMemberAsync(WorkspaceMember member)
        {
            _context.WorkspaceMembers.Update(member);
            await _context.SaveChangesAsync();
            return member;
        }
    }
}
