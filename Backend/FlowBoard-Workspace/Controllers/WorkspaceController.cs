using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlowBoard_Workspace.DTOs;
using FlowBoard_Workspace.Interfaces;

namespace FlowBoard_Workspace.Controllers
{
    [ApiController]
    [Route("api/workspaces")]
    [Authorize]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _workspaceService;

        public WorkspaceController(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token.");
            
            return Guid.Parse(userIdClaim);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkspaceCreateDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _workspaceService.CreateWorkspaceAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.WorkspaceId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _workspaceService.GetByIdAsync(id, userId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("owner")]
        public async Task<ActionResult<IEnumerable<WorkspaceResponseDto>>> GetByOwner()
        {
            try
            {
                var userId = GetUserId();
                var result = await _workspaceService.GetByOwnerAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("member")]
        public async Task<ActionResult<IEnumerable<WorkspaceResponseDto>>> GetByMember()
        {
            try
            {
                var userId = GetUserId();
                var result = await _workspaceService.GetByMemberAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WorkspaceCreateDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _workspaceService.UpdateWorkspaceAsync(id, dto, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetUserId();
                await _workspaceService.DeleteWorkspaceAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- Admin Endpoints ---

        [HttpGet("all")]
        [Authorize(Roles = "PlatformAdmin")]
        public async Task<IActionResult> GetAll()
        {
            // For simplicity, we'll use a new method in service or just fetch by a system user
            // But here we can just call the repository directly or a service method
            var result = await _workspaceService.GetAllAsync(); 
            return Ok(result);
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "PlatformAdmin")]
        public async Task<IActionResult> AdminDelete(int id)
        {
            await _workspaceService.AdminDeleteWorkspaceAsync(id);
            return NoContent();
        }

        // --- Membership Endpoints ---

        [HttpPost("{workspaceId}/members")]
        public async Task<IActionResult> AddMember(int workspaceId, [FromBody] WorkspaceMemberDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _workspaceService.AddMemberAsync(workspaceId, dto.UserId, dto.Role, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{workspaceId}/members/{userIdToRemove}")]
        public async Task<IActionResult> RemoveMember(int workspaceId, Guid userIdToRemove)
        {
            try
            {
                var userId = GetUserId();
                await _workspaceService.RemoveMemberAsync(workspaceId, userIdToRemove, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{workspaceId}/members/{memberUserId}")]
        public async Task<IActionResult> UpdateRole(int workspaceId, Guid memberUserId, [FromBody] string role)
        {
            try
            {
                var userId = GetUserId();
                await _workspaceService.UpdateMemberRoleAsync(workspaceId, memberUserId, role, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{workspaceId}/members")]
        public async Task<ActionResult<IEnumerable<WorkspaceMemberDto>>> GetMembers(int workspaceId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _workspaceService.GetMembersAsync(workspaceId, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
