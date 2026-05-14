using FlowBoard_NotificationService.Data;
using FlowBoard_NotificationService.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard_NotificationService.Repositories
{
    /// <summary>
    /// EF Core implementation of INotificationRepository backed by PostgreSQL.
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> FindByRecipientId(string recipientId)
            => await _context.Notifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<List<Notification>> FindByRecipientIdAndIsRead(string recipientId, bool isRead)
            => await _context.Notifications
                .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<int> CountByRecipientIdAndIsRead(string recipientId, bool isRead)
            => await _context.Notifications
                .CountAsync(n => n.RecipientId == recipientId && n.IsRead == isRead);

        public async Task<List<Notification>> FindByType(string type)
            => await _context.Notifications
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<List<Notification>> FindByRelatedId(int relatedId)
            => await _context.Notifications
                .Where(n => n.RelatedId == relatedId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<Notification> Save(Notification notification)
        {
            if (notification.NotificationId == 0)
                _context.Notifications.Add(notification);
            else
                _context.Notifications.Update(notification);

            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task SaveAll(IEnumerable<Notification> notifications)
        {
            // For already-tracked entities (updates), just save changes.
            // For new entities, add them first then save.
            foreach (var n in notifications)
            {
                if (_context.Entry(n).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    _context.Notifications.Add(n);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByNotificationId(int notificationId)
        {
            var n = await _context.Notifications.FindAsync(notificationId);
            if (n != null)
            {
                _context.Notifications.Remove(n);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByRecipientIdAndIsRead(string recipientId, bool isRead)
        {
            var toDelete = await _context.Notifications
                .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
                .ToListAsync();
            _context.Notifications.RemoveRange(toDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<Notification?> FindById(int notificationId)
            => await _context.Notifications.FindAsync(notificationId);

        public async Task<List<Notification>> FindAll()
            => await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
    }
}
