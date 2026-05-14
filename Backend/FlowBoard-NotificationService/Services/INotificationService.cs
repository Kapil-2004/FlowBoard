using FlowBoard_NotificationService.Models;

namespace FlowBoard_NotificationService.Services
{
    /// <summary>
    /// Service contract for all notification dispatch, retrieval, and lifecycle operations.
    /// Matches the INotificationService class diagram exactly.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Persist and push a single notification to the recipient.</summary>
        Task Send(Notification notification);

        /// <summary>Broadcast a message to a list of recipient IDs (admin bulk send).</summary>
        Task SendBulk(List<string> recipientIds, string title, string message);

        /// <summary>Mark a single notification as read and push updated badge count.</summary>
        Task MarkAsRead(int notificationId);

        /// <summary>Mark all notifications for a user as read.</summary>
        Task MarkAllRead(string recipientId);

        /// <summary>Delete all read notifications for a user (housekeeping).</summary>
        Task DeleteRead(string recipientId);

        /// <summary>Return all notifications (newest first) for a recipient.</summary>
        Task<List<Notification>> GetByRecipient(string recipientId);

        /// <summary>Return the unread badge count for a recipient.</summary>
        Task<int> GetUnreadCount(string recipientId);

        /// <summary>Delete a specific notification.</summary>
        Task DeleteNotification(int notificationId);

        /// <summary>Send an email via SendGrid.</summary>
        Task SendEmail(string toEmail, string subject, string htmlBody);

        /// <summary>Return every notification in the system (admin view).</summary>
        Task<List<Notification>> GetAll();
    }
}
