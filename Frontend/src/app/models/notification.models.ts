export interface AppNotification {
  notificationId: number;
  actorId: number;
  recipientId: number;
  type: 'ASSIGNMENT' | 'MENTION' | 'DUE_DATE' | 'COMMENT' | 'MOVE' | 'SYSTEM';
  title: string;
  message: string;
  relatedId: number;
  relatedType: 'CARD' | 'BOARD';
  isRead: boolean;
  createdAt: string;
}

export interface CreateNotificationDto {
  actorId: number;
  recipientId: number;
  type: string;
  title: string;
  message: string;
  relatedId: number;
  relatedType: string;
}

export interface SendBulkDto {
  recipientIds: number[];
  title: string;
  message: string;
}

/** Icon and accent colour per notification type */
export const NOTIFICATION_META: Record<string, { icon: string; accent: string }> = {
  ASSIGNMENT: { icon: '👤', accent: '#6366f1' },
  MENTION:    { icon: '@',  accent: '#f59e0b' },
  DUE_DATE:   { icon: '⏰', accent: '#ef4444' },
  COMMENT:    { icon: '💬', accent: '#3b82f6' },
  MOVE:       { icon: '🔀', accent: '#10b981' },
  SYSTEM:     { icon: '📢', accent: '#8b5cf6' },
};
