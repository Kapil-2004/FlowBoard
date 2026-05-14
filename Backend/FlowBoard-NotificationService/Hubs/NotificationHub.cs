using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlowBoard_NotificationService.Hubs
{
    /// <summary>
    /// SignalR hub for real-time notification badge counts and live panel updates.
    /// Each connected user joins a group keyed by their userId so the server
    /// can push targeted messages.
    ///
    /// Client connects with:  /hubs/notifications?userId={id}
    /// Client methods pushed: "BadgeCount" (int), "NewNotification" (Notification)
    /// </summary>
    [AllowAnonymous]   // Auth handled at the HTTP/WS upgrade layer via JWT query-string
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // The client passes ?userId=xxx in the WebSocket upgrade URL
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation("NotificationHub: user {UserId} connected (conn={ConnId})", userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
