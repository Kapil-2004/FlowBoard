using FlowBoard_NotificationService.Data;
using FlowBoard_NotificationService.Hubs;
using FlowBoard_NotificationService.Models;
using FlowBoard_NotificationService.Repositories;
using FlowBoard_NotificationService.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowBoard_NotificationService.Tests
{
    /// <summary>
    /// Unit tests for NotificationServiceImpl using in-memory EF Core database,
    /// mocked SignalR hub context, and mocked configuration.
    /// </summary>
    public class NotificationServiceTests
    {
        // ── Helpers ──────────────────────────────────────────────────
        private static NotificationDbContext BuildDbContext()
        {
            var opts = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new NotificationDbContext(opts);
        }

        private static NotificationServiceImpl BuildService(NotificationDbContext context)
        {
            var repo   = new NotificationRepository(context);
            var hub    = new Mock<IHubContext<NotificationHub>>();
            var config = new Mock<IConfiguration>();
            var logger = new Mock<ILogger<NotificationServiceImpl>>();

            // Mock SignalR chain: hub.Clients.Group(...).SendAsync(...)
            var mockClients = new Mock<IHubClients>();
            var mockGroup   = new Mock<IClientProxy>();
            hub.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockGroup.Object);
            mockGroup
                .Setup(g => g.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            return new NotificationServiceImpl(repo, hub.Object, config.Object, logger.Object);
        }

        // ── Tests ─────────────────────────────────────────────────────

        [Fact]
        public async Task Send_ShouldPersistNotification()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            var notification = new Notification
            {
                ActorId     = 1,
                RecipientId = 2,
                Type        = "ASSIGNMENT",
                Title       = "You were assigned a card",
                Message     = "Actor 1 assigned card to you",
                RelatedId   = 10,
                RelatedType = "CARD"
            };

            await service.Send(notification);

            Assert.Single(ctx.Notifications);
            Assert.Equal("ASSIGNMENT", ctx.Notifications.First().Type);
        }

        [Fact]
        public async Task GetUnreadCount_ShouldReturnOnlyUnread()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            ctx.Notifications.AddRange(
                new Notification { RecipientId = 5, Type = "COMMENT", Title = "New comment", Message = "A", IsRead = false },
                new Notification { RecipientId = 5, Type = "COMMENT", Title = "Old comment", Message = "B", IsRead = true },
                new Notification { RecipientId = 5, Type = "MENTION", Title = "You were mentioned", Message = "C", IsRead = false }
            );
            await ctx.SaveChangesAsync();

            var count = await service.GetUnreadCount(5);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task MarkAsRead_ShouldFlipIsReadFlag()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            var n = new Notification { RecipientId = 3, Type = "MOVE", Title = "Card moved", Message = "Card moved to Done", IsRead = false };
            ctx.Notifications.Add(n);
            await ctx.SaveChangesAsync();

            await service.MarkAsRead(n.NotificationId);

            var updated = ctx.Notifications.First();
            Assert.True(updated.IsRead);
        }

        [Fact]
        public async Task MarkAllRead_ShouldClearAllUnreadForRecipient()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            ctx.Notifications.AddRange(
                new Notification { RecipientId = 7, Type = "COMMENT", Title = "A", Message = "A", IsRead = false },
                new Notification { RecipientId = 7, Type = "MENTION", Title = "B", Message = "B", IsRead = false },
                new Notification { RecipientId = 9, Type = "MOVE",    Title = "C", Message = "C", IsRead = false }
            );
            await ctx.SaveChangesAsync();

            await service.MarkAllRead(7);

            var remaining = ctx.Notifications.Where(n => n.RecipientId == 7 && !n.IsRead).Count();
            Assert.Equal(0, remaining);

            // Recipient 9 should remain unread
            Assert.Equal(1, ctx.Notifications.Count(n => n.RecipientId == 9 && !n.IsRead));
        }

        [Fact]
        public async Task DeleteNotification_ShouldRemoveFromDb()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            var n = new Notification { RecipientId = 4, Type = "DUE_DATE", Title = "Due soon", Message = "Card due in 1h" };
            ctx.Notifications.Add(n);
            await ctx.SaveChangesAsync();

            await service.DeleteNotification(n.NotificationId);

            Assert.Empty(ctx.Notifications);
        }

        [Fact]
        public async Task SendBulk_ShouldCreateOneNotificationPerRecipient()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            var recipients = new List<int> { 1, 2, 3, 4, 5 };
            await service.SendBulk(recipients, "System Alert", "Scheduled maintenance tonight.");

            Assert.Equal(5, ctx.Notifications.Count());
            Assert.All(ctx.Notifications, n => Assert.Equal("System Alert", n.Title));
        }

        [Fact]
        public async Task DeleteRead_ShouldOnlyRemoveReadNotifications()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            ctx.Notifications.AddRange(
                new Notification { RecipientId = 6, Type = "COMMENT", Title = "R1", Message = "R1", IsRead = true },
                new Notification { RecipientId = 6, Type = "COMMENT", Title = "R2", Message = "R2", IsRead = true },
                new Notification { RecipientId = 6, Type = "MENTION", Title = "U1", Message = "U1", IsRead = false }
            );
            await ctx.SaveChangesAsync();

            await service.DeleteRead(6);

            Assert.Single(ctx.Notifications);
            Assert.False(ctx.Notifications.First().IsRead);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEveryNotification()
        {
            using var ctx = BuildDbContext();
            var service   = BuildService(ctx);

            ctx.Notifications.AddRange(
                new Notification { RecipientId = 1, Type = "ASSIGNMENT", Title = "A", Message = "A" },
                new Notification { RecipientId = 2, Type = "MENTION",    Title = "B", Message = "B" }
            );
            await ctx.SaveChangesAsync();

            var all = await service.GetAll();
            Assert.Equal(2, all.Count);
        }
    }
}
