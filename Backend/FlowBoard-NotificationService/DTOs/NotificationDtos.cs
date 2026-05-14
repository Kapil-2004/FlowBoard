namespace FlowBoard_NotificationService.DTOs
{
    /// <summary>DTO for sending a bulk broadcast notification.</summary>
    public class SendBulkDto
    {
        public List<string> RecipientIds { get; set; } = new();
        public string Title   { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>DTO for creating a single targeted notification via the API.</summary>
    public class CreateNotificationDto
    {
        public string ActorId      { get; set; }
        public string RecipientId  { get; set; }
        public string Type      { get; set; } = string.Empty;
        public string Title     { get; set; } = string.Empty;
        public string Message   { get; set; } = string.Empty;
        public int RelatedId    { get; set; }
        public string RelatedType { get; set; } = "CARD";
    }

    /// <summary>Payload published by Comment/Card services over RabbitMQ.</summary>
    public class NotificationEvent
    {
        public string EventType  { get; set; } = string.Empty;  // "COMMENT" | "ASSIGNMENT" | "MOVE"
        public string ActorId       { get; set; }
        public string RecipientId   { get; set; }
        public int RelatedId     { get; set; }
        public string RelatedType { get; set; } = "CARD";
        public string Message    { get; set; } = string.Empty;
        public string Title      { get; set; } = string.Empty;
    }
}
