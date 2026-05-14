import { Component, inject, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { InAppNotificationService } from '../../services/in-app-notification.service';
import { AuthService } from '../../services/auth.service';
import { AppNotification, NOTIFICATION_META } from '../../models/notification.models';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, DatePipe],
  template: `
    <div class="notif-wrapper" (click)="$event.stopPropagation()">

      <!-- Bell Button -->
      <button class="bell-btn" (click)="onToggle()" [class.active]="notifSvc.panelOpen()" title="Notifications">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/>
          <path d="M13.73 21a2 2 0 0 1-3.46 0"/>
        </svg>
        <span class="badge" *ngIf="notifSvc.unreadCount() > 0">
          {{ notifSvc.unreadCount() > 9 ? '9+' : notifSvc.unreadCount() }}
        </span>
      </button>

      <!-- Notification Panel -->
      <div class="notif-panel" *ngIf="notifSvc.panelOpen()">
        <div class="panel-header">
          <span class="panel-title">Notifications</span>
          <div class="panel-actions">
            <button class="action-link" (click)="onMarkAllRead()" *ngIf="notifSvc.unreadCount() > 0">
              Mark all read
            </button>
            <button class="action-link danger" (click)="onDeleteRead()" *ngIf="hasRead()">
              Clear read
            </button>
          </div>
        </div>

        <div class="notif-list">
          <div *ngIf="notifSvc.items().length === 0" class="empty-state">
            <span class="empty-icon">🔔</span>
            <p>You're all caught up!</p>
          </div>

          <div class="notif-item"
               *ngFor="let n of notifSvc.items()"
               [class.unread]="!n.isRead"
               (click)="onClickItem(n)">
            <div class="notif-accent" [style.background]="getMeta(n.type).accent"></div>
            <span class="notif-icon">{{ getMeta(n.type).icon }}</span>
            <div class="notif-body">
              <p class="notif-title">{{ n.title }}</p>
              <p class="notif-msg">{{ n.message }}</p>
              <span class="notif-time">{{ n.createdAt | date:'MMM d, h:mm a' }}</span>
            </div>
            <div class="notif-end">
              <span class="unread-dot" *ngIf="!n.isRead"></span>
              <button class="delete-notif" (click)="onDelete(n, $event)" title="Delete">×</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .notif-wrapper {
      position: relative;
    }

    .bell-btn {
      position: relative;
      width: 40px; height: 40px;
      display: flex; align-items: center; justify-content: center;
      background: transparent;
      border: 1px solid transparent;
      border-radius: var(--radius-sm, 8px);
      color: var(--color-text-secondary, #aaa);
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .bell-btn:hover, .bell-btn.active {
      background: rgba(255,255,255,0.06);
      border-color: rgba(255,255,255,0.1);
      color: var(--color-text, #fff);
    }

    .badge {
      position: absolute;
      top: 6px; right: 6px;
      min-width: 16px; height: 16px;
      background: #ef4444;
      color: #fff;
      font-size: 10px;
      font-weight: 700;
      border-radius: 10px;
      display: flex; align-items: center; justify-content: center;
      padding: 0 4px;
      pointer-events: none;
    }

    /* ── Panel ────────────────────────────────────────────────── */
    .notif-panel {
      position: absolute;
      top: calc(100% + 10px);
      right: 0;
      width: 360px;
      max-height: 480px;
      background: var(--bg-secondary, #1a1a1a);
      border: 1px solid rgba(255,255,255,0.08);
      border-radius: 12px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.5);
      display: flex;
      flex-direction: column;
      overflow: hidden;
      z-index: 1000;
      animation: fadeUp 0.2s ease-out;
    }

    @keyframes fadeUp {
      from { opacity: 0; transform: translateY(-8px); }
      to   { opacity: 1; transform: translateY(0); }
    }

    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      border-bottom: 1px solid rgba(255,255,255,0.06);
      flex-shrink: 0;
    }

    .panel-title {
      font-weight: 600;
      font-size: 14px;
      color: var(--color-text, #fff);
    }

    .panel-actions {
      display: flex;
      gap: 12px;
    }

    .action-link {
      background: none;
      border: none;
      font-size: 12px;
      color: var(--color-accent, #fff);
      cursor: pointer;
      opacity: 0.7;
      transition: opacity 0.2s;
    }
    .action-link:hover { opacity: 1; }
    .action-link.danger { color: #ef4444; }

    /* ── List ─────────────────────────────────────────────────── */
    .notif-list {
      overflow-y: auto;
      flex: 1;
    }
    .notif-list::-webkit-scrollbar { width: 4px; }
    .notif-list::-webkit-scrollbar-track { background: transparent; }
    .notif-list::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.1); border-radius: 4px; }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 48px 24px;
      gap: 12px;
      color: rgba(255,255,255,0.3);
    }
    .empty-icon { font-size: 32px; }
    .empty-state p { font-size: 14px; margin: 0; }

    /* ── Item ─────────────────────────────────────────────────── */
    .notif-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 14px 16px;
      cursor: pointer;
      position: relative;
      transition: background 0.15s;
      border-bottom: 1px solid rgba(255,255,255,0.04);
    }
    .notif-item:hover { background: rgba(255,255,255,0.04); }
    .notif-item.unread { background: rgba(99,102,241,0.05); }

    .notif-accent {
      position: absolute;
      left: 0; top: 0; bottom: 0;
      width: 3px;
      border-radius: 0 3px 3px 0;
    }

    .notif-icon {
      font-size: 18px;
      flex-shrink: 0;
      width: 28px;
      text-align: center;
    }

    .notif-body {
      flex: 1;
      min-width: 0;
    }
    .notif-title {
      font-size: 13px;
      font-weight: 600;
      color: var(--color-text, #fff);
      margin: 0 0 4px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .notif-msg {
      font-size: 12px;
      color: rgba(255,255,255,0.5);
      margin: 0 0 4px;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .notif-time {
      font-size: 11px;
      color: rgba(255,255,255,0.3);
    }

    .notif-end {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      flex-shrink: 0;
    }

    .unread-dot {
      width: 8px; height: 8px;
      background: #6366f1;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .delete-notif {
      background: none;
      border: none;
      color: rgba(255,255,255,0.2);
      font-size: 16px;
      line-height: 1;
      cursor: pointer;
      padding: 0;
      transition: color 0.2s;
    }
    .delete-notif:hover { color: #ef4444; }
  `]
})
export class NotificationBellComponent implements OnInit {
  notifSvc = inject(InAppNotificationService);
  private authSvc = inject(AuthService);

  ngOnInit() {
    const user = this.authSvc.currentUser();
    if (user?.id) {
      this.notifSvc.loadForRecipient(user.id).subscribe();
    }
  }

  onToggle() { this.notifSvc.togglePanel(); }

  onMarkAllRead() {
    const user = this.authSvc.currentUser();
    if (user?.id) this.notifSvc.markAllRead(user.id).subscribe();
  }

  onDeleteRead() {
    const user = this.authSvc.currentUser();
    if (user?.id) this.notifSvc.deleteRead(user.id).subscribe();
  }

  onClickItem(n: AppNotification) {
    if (!n.isRead) this.notifSvc.markAsRead(n.notificationId).subscribe();
  }

  onDelete(n: AppNotification, e: Event) {
    e.stopPropagation();
    this.notifSvc.delete(n.notificationId).subscribe();
  }

  hasRead(): boolean { return this.notifSvc.items().some(n => n.isRead); }

  getMeta(type: string) {
    return NOTIFICATION_META[type] ?? { icon: '🔔', accent: '#6b7280' };
  }

  @HostListener('document:click')
  onDocumentClick() { this.notifSvc.closePanel(); }
}
