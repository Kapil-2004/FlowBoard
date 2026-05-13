using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlowBoard_ListService.DTOs;
using FlowBoard_ListService.Services;

namespace FlowBoard_ListService.Controllers
{
    /// <summary>
    /// REST controller for UC4 – List/Column-Service.
    /// Exposes /api/lists endpoints: create, read, update, reorder, archive,
    /// unarchive, move, and delete TaskLists.
    /// </summary>
    [ApiController]
    [Route("api/lists")]
    [Authorize]
    public class ListController : ControllerBase
    {
        private readonly IListService _listService;

        public ListController(IListService listService)
        {
            _listService = listService;
        }

        // ── POST /api/lists  (Create) ────────────────────────────────────────────

        /// <summary>Creates a new list on a board (appended at the right).</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateListDto createDto)
        {
            try
            {
                var list = await _listService.CreateListAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = list.ListId }, list);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── GET /api/lists/{id}  (Single list) ──────────────────────────────────

        /// <summary>Returns a single list by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var list = await _listService.GetListByIdAsync(id);
                if (list == null) return NotFound(new { message = "List not found." });
                return Ok(list);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── GET /api/lists/board/{boardId}  (All active lists for a board) ───────

        /// <summary>Returns all active (non-archived) lists for a board ordered by position.</summary>
        [HttpGet("board/{boardId}")]
        public async Task<ActionResult<List<ListDto>>> GetByBoard(int boardId)
        {
            try
            {
                var lists = await _listService.GetListsByBoardAsync(boardId);
                return Ok(lists);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── GET /api/lists/board/{boardId}/archived  (Archived lists) ────────────

        /// <summary>Returns all archived lists for a board.</summary>
        [HttpGet("board/{boardId}/archived")]
        public async Task<ActionResult<List<ListDto>>> GetArchivedByBoard(int boardId)
        {
            try
            {
                var lists = await _listService.GetArchivedListsAsync(boardId);
                return Ok(lists);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── PUT /api/lists/{id}  (Rename / recolour) ─────────────────────────────

        /// <summary>Renames or recolours an existing list.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateListDto updateDto)
        {
            try
            {
                var list = await _listService.UpdateListAsync(id, updateDto);
                return Ok(list);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── PUT /api/lists/board/{boardId}/reorder  (Drag-and-drop) ─────────────

        /// <summary>
        /// Atomically reorders all lists on a board.
        /// The body must contain the complete ordered list of ListIds.
        /// </summary>
        [HttpPut("board/{boardId}/reorder")]
        public async Task<IActionResult> Reorder(int boardId, [FromBody] ReorderListsDto reorderDto)
        {
            try
            {
                await _listService.ReorderListsAsync(boardId, reorderDto.ListIds);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── POST /api/lists/{id}/archive  (Archive) ──────────────────────────────

        /// <summary>Archives (soft-deletes) a list.</summary>
        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                await _listService.ArchiveListAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── POST /api/lists/{id}/unarchive  (Unarchive) ──────────────────────────

        /// <summary>Restores an archived list to active.</summary>
        [HttpPost("{id}/unarchive")]
        public async Task<IActionResult> Unarchive(int id)
        {
            try
            {
                await _listService.UnarchiveListAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── PUT /api/lists/{id}/move  (Move to another board) ────────────────────

        /// <summary>Moves a list to a different board.</summary>
        [HttpPut("{id}/move")]
        public async Task<IActionResult> Move(int id, [FromBody] MoveListDto moveDto)
        {
            try
            {
                var moved = await _listService.MoveListAsync(id, moveDto.TargetBoardId);
                return Ok(moved);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new { message });
            }
        }

        // ── DELETE /api/lists/{id}  (Hard delete) ────────────────────────────────

        /// <summary>Permanently removes a list.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _listService.DeleteListAsync(id);
                return NoContent();
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
