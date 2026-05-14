namespace FlowBoard_NotificationService.Models
{
    /// <summary>
    /// Core Notification entity — persisted to the notification database.
    /// Tracks in-app alerts for assignment, mentions, due-dates, moves and replies.
    /// </summary>
    public class Notification
    {
        public int NotificationId { get; set; }

        /// <summary>User that caused the notification (e.g. commenter, assigner).</summary>
        public string ActorId { get; set; }

        /// <summary>User that should receive the notification.</summary>
        public string RecipientId { get; set; }

        /// <summary>Notification category: ASSIGNMENT | MENTION | DUE_DATE | COMMENT | MOVE.</summary>
        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>CardId or BoardId — used for deep-linking.</summary>
        public int RelatedId { get; set; }

        /// <summary>"CARD" or "BOARD" — discriminator for deep-links.</summary>
        public string RelatedType { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper methods per class diagram
        public int GetNotificationId() => NotificationId;
        public string GetRecipientId()    => RecipientId;
        public string GetActorId()        => ActorId;
        public string GetNotificationType() => Type;
        public string GetMessage()     => Message;
        public int GetRelatedId()      => RelatedId;
        public string GetRelatedType() => RelatedType;
        public bool IsReadStatus()     => IsRead;
        public void SetRead(bool value) => IsRead = value;
        public DateTime GetCreatedAt() => CreatedAt;
        public override string ToString() =>
            $"[{Type}] {Title}: {Message} (recipient={RecipientId}, isRead={IsRead})";
    }
}
