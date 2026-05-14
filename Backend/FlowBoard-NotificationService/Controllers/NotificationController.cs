using FlowBoard_NotificationService.DTOs;
using FlowBoard_NotificationService.Models;
using FlowBoard_NotificationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowBoard_NotificationService.Controllers
{
    /// <summary>
    /// REST controller exposing all /api/notifications endpoints.
    /// Requires JWT bearer auth on all routes.
    /// </summary>
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/notifications/recipient/{recipientId}
        // Returns all notifications for a user (newest first)
        // ──────────────────────────────────────────────────────────
        [HttpGet("recipient/{recipientId}")]
        public async Task<ActionResult<List<Notification>>> GetByRecipient(string recipientId)
        {
            var result = await _service.GetByRecipient(recipientId);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/notifications/unread-count/{recipientId}
        // Returns the integer badge count of unread notifications
        // ──────────────────────────────────────────────────────────
        [HttpGet("unread-count/{recipientId}")]
        public async Task<ActionResult<int>> GetUnreadCount(string recipientId)
        {
            var count = await _service.GetUnreadCount(recipientId);
            return Ok(count);
        }

        // ──────────────────────────────────────────────────────────
        // PUT /api/notifications/{id}/read
        // Mark a single notification as read
        // ──────────────────────────────────────────────────────────
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsRead(id);
            return NoContent();
        }

        // ──────────────────────────────────────────────────────────
        // PUT /api/notifications/recipient/{recipientId}/read-all
        // Mark ALL notifications as read for a recipient
        // ──────────────────────────────────────────────────────────
        [HttpPut("recipient/{recipientId}/read-all")]
        public async Task<IActionResult> MarkAllRead(string recipientId)
        {
            await _service.MarkAllRead(recipientId);
            return NoContent();
        }

        // ──────────────────────────────────────────────────────────
        // DELETE /api/notifications/{id}
        // Hard-delete a single notification
        // ──────────────────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteNotification(id);
            return NoContent();
        }

        // ──────────────────────────────────────────────────────────
        // DELETE /api/notifications/recipient/{recipientId}/read
        // Delete all read notifications for housekeeping
        // ──────────────────────────────────────────────────────────
        [HttpDelete("recipient/{recipientId}/read")]
        public async Task<IActionResult> DeleteRead(string recipientId)
        {
            await _service.DeleteRead(recipientId);
            return NoContent();
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/notifications/send
        // Create and dispatch a single notification
        // ──────────────────────────────────────────────────────────
        [HttpPost("send")]
        public async Task<ActionResult<Notification>> Send([FromBody] CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                ActorId     = dto.ActorId,
                RecipientId = dto.RecipientId,
                Type        = dto.Type,
                Title       = dto.Title,
                Message     = dto.Message,
                RelatedId   = dto.RelatedId,
                RelatedType = dto.RelatedType
            };
            await _service.Send(notification);
            return Ok(notification);
        }

        // ──────────────────────────────────────────────────────────
        // POST /api/notifications/bulk
        // Admin broadcast to multiple recipients
        // ──────────────────────────────────────────────────────────
        [HttpPost("bulk")]
        public async Task<IActionResult> SendBulk([FromBody] SendBulkDto dto)
        {
            await _service.SendBulk(dto.RecipientIds, dto.Title, dto.Message);
            return Ok(new { sent = dto.RecipientIds.Count });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/notifications/all   (admin only in a real system)
        // Returns every notification in the database
        // ──────────────────────────────────────────────────────────
        [HttpGet("all")]
        public async Task<ActionResult<List<Notification>>> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }
    }
}
