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
    [Route("api/boards")]
    [Authorize]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token.");
            
            return Guid.Parse(userIdClaim);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBoard([FromQuery] int workspaceId, [FromBody] BoardCreateDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.CreateBoardAsync(workspaceId, dto, userId);
                return CreatedAtAction(nameof(GetBoardById), new { id = result.BoardId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBoardById(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.GetBoardByIdAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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

        [HttpGet("workspace/{workspaceId}")]
        public async Task<ActionResult<IEnumerable<BoardResponseDto>>> GetBoardsByWorkspace(int workspaceId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.GetBoardsByWorkspaceAsync(workspaceId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("member")]
        public async Task<ActionResult<IEnumerable<BoardResponseDto>>> GetBoardsByMember()
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.GetBoardsByMemberAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(int id, [FromBody] BoardUpdateDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.UpdateBoardAsync(id, dto, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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

        [HttpPut("{id}/close")]
        public async Task<IActionResult> CloseBoard(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.CloseBoardAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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
        public async Task<IActionResult> DeleteBoard(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.DeleteBoardAsync(id, userId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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

        [HttpPost("{boardId}/members")]
        public async Task<IActionResult> AddMember(int boardId, [FromBody] AddBoardMemberDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.AddMemberAsync(boardId, dto.UserId, dto.Role, userId);
                return CreatedAtAction(nameof(GetBoardById), new { id = boardId }, result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("members/{memberId}")]
        public async Task<IActionResult> RemoveMember(int memberId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.RemoveMemberAsync(memberId, userId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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

        [HttpPut("members/{memberId}/role")]
        public async Task<IActionResult> UpdateMemberRole(int memberId, [FromBody] UpdateBoardMemberRoleDto dto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.UpdateMemberRoleAsync(memberId, dto.Role, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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

        [HttpGet("{boardId}/members")]
        public async Task<ActionResult<IEnumerable<BoardMemberDto>>> GetMembers(int boardId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _boardService.GetMembersAsync(boardId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
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

    public class AddBoardMemberDto
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = "MEMBER";
    }

    public class UpdateBoardMemberRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }
}
