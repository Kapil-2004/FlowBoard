using FlowBoard_NotificationService.DTOs;
using FlowBoard_NotificationService.Models;
using FlowBoard_NotificationService.Services;
using MassTransit;

namespace FlowBoard_NotificationService.Consumers
{
    /// <summary>
    /// MassTransit consumer that listens on the RabbitMQ "notification-events" queue.
    /// When the Comment-Service or Card-Service publishes a NotificationEvent message,
    /// this consumer persists the notification and pushes a live SignalR update.
    /// </summary>
    public class NotificationEventConsumer : IConsumer<NotificationEvent>
    {
        private readonly INotificationService _service;
        private readonly ILogger<NotificationEventConsumer> _logger;

        public NotificationEventConsumer(INotificationService service, ILogger<NotificationEventConsumer> logger)
        {
            _service = service;
            _logger  = logger;
        }

        public async Task Consume(ConsumeContext<NotificationEvent> context)
        {
            var evt = context.Message;
            _logger.LogInformation("Consumed notification event: type={Type}, actor={Actor}, recipient={Recipient}",
                evt.EventType, evt.ActorId, evt.RecipientId);

            var notification = new Notification
            {
                ActorId     = evt.ActorId,
                RecipientId = evt.RecipientId,
                Type        = evt.EventType,
                Title       = evt.Title,
                Message     = evt.Message,
                RelatedId   = evt.RelatedId,
                RelatedType = evt.RelatedType
            };

            await _service.Send(notification);
        }
    }
}
