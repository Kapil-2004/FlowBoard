using FlowBoard_NotificationService.Models;

namespace FlowBoard_NotificationService.Repositories
{
    /// <summary>
    /// Repository abstraction for Notification persistence operations.
    /// All methods are async to avoid blocking the SignalR/MassTransit event loop.
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>Fetch all notifications for a given recipient (newest first).</summary>
        Task<List<Notification>> FindByRecipientId(string recipientId);

        /// <summary>Fetch notifications filtered by read state.</summary>
        Task<List<Notification>> FindByRecipientIdAndIsRead(string recipientId, bool isRead);

        /// <summary>Count unread (or read) notifications for badge display.</summary>
        Task<int> CountByRecipientIdAndIsRead(string recipientId, bool isRead);

        /// <summary>Find all notifications of a specific type (e.g. "DUE_DATE").</summary>
        Task<List<Notification>> FindByType(string type);

        /// <summary>Find all notifications linked to a card or board by RelatedId.</summary>
        Task<List<Notification>> FindByRelatedId(int relatedId);

        /// <summary>Persist a new notification.</summary>
        Task<Notification> Save(Notification notification);

        /// <summary>Persist multiple notifications at once (bulk broadcast).</summary>
        Task SaveAll(IEnumerable<Notification> notifications);

        /// <summary>Delete a single notification by its primary key.</summary>
        Task DeleteByNotificationId(int notificationId);

        /// <summary>Bulk-delete read or unread notifications for a recipient.</summary>
        Task DeleteByRecipientIdAndIsRead(string recipientId, bool isRead);

        /// <summary>Find a notification by its primary key; returns null if not found.</summary>
        Task<Notification?> FindById(int notificationId);

        /// <summary>Fetch every notification in the system (admin use only).</summary>
        Task<List<Notification>> FindAll();
    }
}
