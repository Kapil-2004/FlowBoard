using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_LabelService.Models;
using FlowBoard_LabelService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard_LabelService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LabelController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public LabelController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        [HttpPost]
        public async Task<ActionResult<Label>> CreateLabel([FromBody] Label label)
        {
            var created = await _labelService.CreateLabel(label);
            return Ok(created);
        }

        [HttpGet("board/{boardId}")]
        public async Task<ActionResult<List<Label>>> GetByBoard(int boardId)
        {
            var labels = await _labelService.GetLabelsByBoard(boardId);
            return Ok(labels);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Label>> UpdateLabel(int id, [FromBody] Label label)
        {
            var updated = await _labelService.UpdateLabel(id, label);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLabel(int id)
        {
            var success = await _labelService.DeleteLabel(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("card/{cardId}/label/{labelId}")]
        public async Task<IActionResult> AddToCard(int cardId, int labelId)
        {
            await _labelService.AddLabelToCard(cardId, labelId);
            return Ok();
        }

        [HttpDelete("card/{cardId}/label/{labelId}")]
        public async Task<IActionResult> RemoveFromCard(int cardId, int labelId)
        {
            await _labelService.RemoveLabelFromCard(cardId, labelId);
            return Ok();
        }

        [HttpGet("card/{cardId}")]
        public async Task<ActionResult<List<Label>>> GetForCard(int cardId)
        {
            var labels = await _labelService.GetLabelsForCard(cardId);
            return Ok(labels);
        }
    }
}
