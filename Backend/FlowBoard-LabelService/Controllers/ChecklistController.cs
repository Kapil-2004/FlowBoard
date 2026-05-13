using System.Collections.Generic;
using System.Threading.Tasks;
using FlowBoard_LabelService.Models;
using FlowBoard_LabelService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowBoard_LabelService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChecklistController : ControllerBase
    {
        private readonly ILabelService _labelService;

        public ChecklistController(ILabelService labelService)
        {
            _labelService = labelService;
        }

        [HttpPost]
        public async Task<ActionResult<Checklist>> CreateChecklist([FromBody] Checklist checklist)
        {
            var created = await _labelService.CreateChecklist(checklist);
            return Ok(created);
        }

        [HttpPost("{id}/item")]
        public async Task<ActionResult<Checklist>> AddItem(int id, [FromBody] ChecklistItem item)
        {
            var updated = await _labelService.AddItem(id, item);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPut("item/{itemId}/toggle")]
        public async Task<ActionResult<ChecklistItem>> ToggleItem(int itemId)
        {
            var item = await _labelService.ToggleItem(itemId);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChecklist(int id)
        {
            var success = await _labelService.DeleteChecklist(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("card/{cardId}")]
        public async Task<ActionResult<List<Checklist>>> GetByCard(int cardId)
        {
            var checklists = await _labelService.GetChecklistsByCard(cardId);
            return Ok(checklists);
        }

        [HttpGet("card/{cardId}/progress")]
        public async Task<ActionResult<double>> GetProgress(int cardId)
        {
            var progress = await _labelService.GetChecklistProgress(cardId);
            return Ok(progress);
        }
    }
}
