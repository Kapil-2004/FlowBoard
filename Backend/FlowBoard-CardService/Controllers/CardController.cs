using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_CardService.DTOs;
using FlowBoard_CardService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard_CardService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;

        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCardDto createDto)
        {
            try
            {
                var card = await _cardService.CreateCardAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = card.CardId }, card);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var card = await _cardService.GetCardByIdAsync(id);
                return Ok(card);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("list/{listId}")]
        public async Task<ActionResult<List<CardDto>>> GetByList(int listId)
        {
            try
            {
                var cards = await _cardService.GetCardsByListAsync(listId);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("board/{boardId}")]
        public async Task<ActionResult<List<CardDto>>> GetByBoard(int boardId)
        {
            try
            {
                var cards = await _cardService.GetCardsByBoardAsync(boardId);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("assignee/{assigneeId}")]
        public async Task<ActionResult<List<CardDto>>> GetByAssignee(int assigneeId)
        {
            try
            {
                var cards = await _cardService.GetCardsByAssigneeAsync(assigneeId);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<List<CardDto>>> GetOverdue()
        {
            try
            {
                var cards = await _cardService.GetOverdueCardsAsync();
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCardDto updateDto)
        {
            try
            {
                var card = await _cardService.UpdateCardAsync(id, updateDto);
                return Ok(card);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id}/move")]
        public async Task<IActionResult> Move(int id, [FromBody] MoveCardDto moveDto)
        {
            try
            {
                var card = await _cardService.MoveCardAsync(id, moveDto);
                return Ok(card);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("list/{listId}/reorder")]
        public async Task<IActionResult> Reorder(int listId, [FromBody] ReorderCardsDto reorderDto)
        {
            try
            {
                await _cardService.ReorderCardsAsync(listId, reorderDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                await _cardService.ArchiveCardAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost("{id}/unarchive")]
        public async Task<IActionResult> Unarchive(int id)
        {
            try
            {
                await _cardService.UnarchiveCardAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _cardService.DeleteCardAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id}/assignee")]
        public async Task<IActionResult> SetAssignee(int id, [FromBody] AssignCardDto assignDto)
        {
            try
            {
                var card = await _cardService.SetAssigneeAsync(id, assignDto);
                return Ok(card);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
