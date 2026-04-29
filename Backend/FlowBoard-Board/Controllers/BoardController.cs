using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlowBoard_Board.DTOs;
using FlowBoard_Board.Services;

namespace FlowBoard_Board.Controllers
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

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid User ID in token.");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBoardDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var board = await _boardService.CreateBoardAsync(createDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = board.BoardId }, board);
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
                var userId = GetCurrentUserId();
                var board = await _boardService.GetBoardByIdAsync(id, userId);
                if (board == null) return NotFound(new { message = "Board not found" });
                return Ok(board);
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
        public async Task<IActionResult> GetByWorkspace(int workspaceId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var boards = await _boardService.GetBoardsByWorkspaceAsync(workspaceId, userId);
                return Ok(boards);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("member")]
        public async Task<IActionResult> GetByMember()
        {
            try
            {
                var userId = GetCurrentUserId();
                var boards = await _boardService.GetBoardsByMemberAsync(userId);
                return Ok(boards);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBoardDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var board = await _boardService.UpdateBoardAsync(id, updateDto, userId);
                return Ok(board);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/close")]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _boardService.CloseBoardAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                var userId = GetCurrentUserId();
                await _boardService.DeleteBoardAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Member Endpoints
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddBoardMemberDto addDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var member = await _boardService.AddMemberAsync(id, addDto, userId);
                return Ok(member);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        [HttpDelete("{id}/members/{userIdToRemove}")]
        public async Task<IActionResult> RemoveMember(int id, Guid userIdToRemove)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _boardService.RemoveMemberAsync(id, userIdToRemove, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        [HttpPut("{id}/members/{userIdToUpdate}")]
        public async Task<IActionResult> UpdateMemberRole(int id, Guid userIdToUpdate, [FromBody] UpdateMemberRoleDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _boardService.UpdateMemberRoleAsync(id, userIdToUpdate, updateDto, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembers(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var members = await _boardService.GetMembersAsync(id, userId);
                return Ok(members);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
