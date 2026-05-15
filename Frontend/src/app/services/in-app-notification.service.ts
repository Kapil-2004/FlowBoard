import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AppNotification, CreateNotificationDto, SendBulkDto } from '../models/notification.models';

@Injectable({
  providedIn: 'root'
})
export class InAppNotificationService {
  private http = inject(HttpClient);
  private isProd = !window.location.hostname.includes('localhost');
  private base = this.isProd ? 'https://notification-service.onrender.com/api/notifications' : '/api/notifications';

  // ── Reactive state ─────────────────────────────────────────────
  readonly items     = signal<AppNotification[]>([]);
  readonly unreadCount = computed(() => this.items().filter(n => !n.isRead).length);
  readonly panelOpen = signal(false);

  // ── REST: fetch ────────────────────────────────────────────────
  loadForRecipient(recipientId: string): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(`${this.base}/recipient/${recipientId}`).pipe(
      tap(list => this.items.set(list))
    );
  }

  getUnreadCount(recipientId: string): Observable<number> {
    return this.http.get<number>(`${this.base}/unread-count/${recipientId}`);
  }

  // ── REST: state mutations ───────────────────────────────────────
  markAsRead(notificationId: number): Observable<void> {
    return this.http.put<void>(`${this.base}/${notificationId}/read`, {}).pipe(
      tap(() => {
        this.items.update(list =>
          list.map(n => n.notificationId === notificationId ? { ...n, isRead: true } : n)
        );
      })
    );
  }

  markAllRead(recipientId: string): Observable<void> {
    return this.http.put<void>(`${this.base}/recipient/${recipientId}/read-all`, {}).pipe(
      tap(() => this.items.update(list => list.map(n => ({ ...n, isRead: true }))))
    );
  }

  delete(notificationId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${notificationId}`).pipe(
      tap(() => this.items.update(list => list.filter(n => n.notificationId !== notificationId)))
    );
  }

  deleteRead(recipientId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/recipient/${recipientId}/read`).pipe(
      tap(() => this.items.update(list => list.filter(n => !n.isRead)))
    );
  }

  // ── REST: send ─────────────────────────────────────────────────
  send(dto: CreateNotificationDto): Observable<AppNotification> {
    return this.http.post<AppNotification>(`${this.base}/send`, dto);
  }

  sendBulk(dto: SendBulkDto): Observable<{ sent: number }> {
    return this.http.post<{ sent: number }>(`${this.base}/bulk`, dto);
  }

  // ── Panel helpers ──────────────────────────────────────────────
  togglePanel() { this.panelOpen.update(v => !v); }
  closePanel()  { this.panelOpen.set(false); }

  /** Push a notification received from the SignalR hub directly into the list. */
  pushLive(notification: AppNotification) {
    this.items.update(list => [notification, ...list]);
  }
}
