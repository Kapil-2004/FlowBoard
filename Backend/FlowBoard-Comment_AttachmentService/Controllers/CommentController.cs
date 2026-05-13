using FlowBoard_Comment_AttachmentService.DTOs;
using FlowBoard_Comment_AttachmentService.Models;
using FlowBoard_Comment_AttachmentService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlowBoard_Comment_AttachmentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
        {
            var comment = new Comment
            {
                CardId = dto.CardId,
                AuthorId = dto.AuthorId,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId
            };
            var created = await _commentService.AddCommentAsync(comment);
            return CreatedAtAction(nameof(GetCommentById), new { id = created.CommentId }, created);
        }

        [HttpGet("card/{cardId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetByCard(int cardId)
        {
            var comments = await _commentService.GetByCardAsync(cardId);
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpGet("{id}/replies")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetReplies(int id)
        {
            var replies = await _commentService.GetRepliesAsync(id);
            return Ok(replies);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto dto)
        {
            try
            {
                var updated = await _commentService.UpdateCommentAsync(id, dto.Content);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            await _commentService.DeleteCommentAsync(id);
            return NoContent();
        }

        [HttpGet("card/{cardId}/count")]
        public async Task<ActionResult<int>> GetCommentCount(int cardId)
        {
            var count = await _commentService.GetCommentCountAsync(cardId);
            return Ok(count);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttachmentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public AttachmentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAttachment([FromBody] Attachment attachment)
        {
            var created = await _commentService.AddAttachmentAsync(attachment);
            return Ok(created);
        }

        [HttpGet("card/{cardId}")]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachmentsByCard(int cardId)
        {
            var attachments = await _commentService.GetAttachmentsByCardAsync(cardId);
            return Ok(attachments);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            await _commentService.DeleteAttachmentAsync(id);
            return NoContent();
        }
    }
}
