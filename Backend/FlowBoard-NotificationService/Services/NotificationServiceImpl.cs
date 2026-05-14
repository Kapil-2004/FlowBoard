using FlowBoard_NotificationService.Hubs;
using FlowBoard_NotificationService.Models;
using FlowBoard_NotificationService.Repositories;
using Microsoft.AspNetCore.SignalR;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FlowBoard_NotificationService.Services
{
    /// <summary>
    /// Core notification orchestrator: persists notifications, pushes live badge counts
    /// via SignalR, and dispatches emails via SendGrid.
    /// </summary>
    public class NotificationServiceImpl : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationServiceImpl> _logger;

        public NotificationServiceImpl(
            INotificationRepository repo,
            IHubContext<NotificationHub> hub,
            IConfiguration config,
            ILogger<NotificationServiceImpl> logger)
        {
            _repo   = repo;
            _hub    = hub;
            _config = config;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Send(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            await _repo.Save(notification);

            // Push real-time badge count to the specific recipient's SignalR group
            var unread = await _repo.CountByRecipientIdAndIsRead(notification.RecipientId, false);
            await _hub.Clients
                .Group($"user_{notification.RecipientId}")
                .SendAsync("BadgeCount", unread);

            // Also push the full notification object for live panel updates
            await _hub.Clients
                .Group($"user_{notification.RecipientId}")
                .SendAsync("NewNotification", notification);
        }

        /// <inheritdoc/>
        public async Task SendBulk(List<string> recipientIds, string title, string message)
        {
            var notifications = recipientIds.Select(id => new Notification
            {
                RecipientId = id,
                ActorId     = "0",        // system-generated
                Type        = "SYSTEM",
                Title       = title,
                Message     = message,
                RelatedType = "BOARD",
                RelatedId   = 0,
                CreatedAt   = DateTime.UtcNow
            }).ToList();

            await _repo.SaveAll(notifications);

            // Notify each recipient via SignalR
            foreach (var n in notifications)
            {
                var unread = await _repo.CountByRecipientIdAndIsRead(n.RecipientId, false);
                await _hub.Clients
                    .Group($"user_{n.RecipientId}")
                    .SendAsync("BadgeCount", unread);
            }
        }

        /// <inheritdoc/>
        public async Task MarkAsRead(int notificationId)
        {
            var n = await _repo.FindById(notificationId);
            if (n == null) return;

            n.IsRead = true;
            await _repo.Save(n);   // triggers SaveChanges via EF tracking

            var unread = await _repo.CountByRecipientIdAndIsRead(n.RecipientId, false);
            await _hub.Clients
                .Group($"user_{n.RecipientId}")
                .SendAsync("BadgeCount", unread);
        }

        /// <inheritdoc/>
        public async Task MarkAllRead(string recipientId)
        {
            var unreadList = await _repo.FindByRecipientIdAndIsRead(recipientId, false);
            foreach (var n in unreadList)
                n.IsRead = true;

            await _repo.SaveAll(unreadList);

            await _hub.Clients
                .Group($"user_{recipientId}")
                .SendAsync("BadgeCount", 0);
        }

        /// <inheritdoc/>
        public async Task DeleteRead(string recipientId)
            => await _repo.DeleteByRecipientIdAndIsRead(recipientId, true);

        /// <inheritdoc/>
        public async Task<List<Notification>> GetByRecipient(string recipientId)
            => await _repo.FindByRecipientId(recipientId);

        /// <inheritdoc/>
        public async Task<int> GetUnreadCount(string recipientId)
            => await _repo.CountByRecipientIdAndIsRead(recipientId, false);

        /// <inheritdoc/>
        public async Task DeleteNotification(int notificationId)
            => await _repo.DeleteByNotificationId(notificationId);

        /// <inheritdoc/>
        public async Task SendEmail(string toEmail, string subject, string htmlBody)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("SendGrid API key not configured — email not sent to {Email}", toEmail);
                return;
            }

            var client  = new SendGridClient(apiKey);
            var from    = new EmailAddress(_config["SendGrid:FromEmail"] ?? "noreply@flowboard.app", "FlowBoard");
            var to      = new EmailAddress(toEmail);
            var msg     = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);

            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
                _logger.LogError("SendGrid failed for {Email}: HTTP {Status}", toEmail, response.StatusCode);
        }

        /// <inheritdoc/>
        public async Task<List<Notification>> GetAll()
            => await _repo.FindAll();
    }
}
