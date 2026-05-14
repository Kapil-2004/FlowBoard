import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { WorkspaceListComponent } from './workspaces/workspace-list/workspace-list.component';
import { NotificationBellComponent } from './notifications/notification-bell/notification-bell.component';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, WorkspaceListComponent, NotificationBellComponent, AdminPanelComponent],
  template: `
    <div class="dashboard-container">
      <nav class="top-nav">
        <div class="logo">
          <span class="logo-icon">≋</span>
          <span class="logo-text">FlowBoard</span>
        </div>
        <div class="user-info" *ngIf="user()">
          <app-notification-bell></app-notification-bell>

          <!-- Admin Panel Toggle — only visible to PlatformAdmin -->
          <button
            *ngIf="isPlatformAdmin()"
            id="btn-admin-panel"
            class="btn-admin"
            [class.active]="showAdmin"
            (click)="showAdmin = !showAdmin"
          >🛡️ Admin Panel</button>

          <div class="user-avatar">{{ getInitials(user()!.fullName) }}</div>
          <div class="user-meta">
            <span class="user-name">{{ user()?.fullName }}</span>
            <span class="user-role" [class]="'role-' + (user()?.role ?? 'member').toLowerCase()">
              {{ user()?.role ?? 'Member' }}
            </span>
          </div>
          <button (click)="onLogout()" class="btn-logout">Logout</button>
        </div>
      </nav>

      <main class="content">
        <header class="page-header">
          <h1>Welcome, {{ user()?.fullName }}</h1>
          <p>{{ isPlatformAdmin() ? 'You have full platform administrator access.' : 'You have successfully authenticated. This is your personal dashboard.' }}</p>
        </header>

        <div class="grid">
          <div class="card profile-card">
            <h3>👤 Profile Info</h3>
            <p><strong>Email:</strong> {{ user()?.email }}</p>
            <p><strong>Joined:</strong> {{ user()?.createdAt | date:'mediumDate' }}</p>
            <p><strong>Role:</strong>
              <span class="role-badge" [class]="'role-' + (user()?.role ?? 'member').toLowerCase()">
                {{ user()?.role ?? 'Member' }}
              </span>
            </p>
            <p><strong>Status:</strong>
              <span class="status-badge active">Active</span>
            </p>
          </div>

          <!-- Quick Stats Card — admin only -->
          <div class="card info-card" *ngIf="isPlatformAdmin()">
            <h3>⚡ Quick Access</h3>
            <p>As a Platform Admin, you can:</p>
            <ul class="feature-list">
              <li>🛡️ View and manage all platform users</li>
              <li>🔑 Change user roles (Member / Board Admin / Platform Admin)</li>
              <li>⏸ Suspend or reactivate accounts</li>
              <li>🗑 Delete user accounts permanently</li>
            </ul>
            <button id="btn-open-admin" class="btn-open-admin" (click)="showAdmin = true">
              Open Admin Panel →
            </button>
          </div>
        </div>

        <!-- Admin Panel (Platform Admin Only) -->
        <div *ngIf="isPlatformAdmin()">
          <app-admin-panel
            *ngIf="showAdmin"
            (onClose)="showAdmin = false"
          ></app-admin-panel>
        </div>

        <!-- Workspace Section -->
        <app-workspace-list></app-workspace-list>
      </main>
    </div>
  `,
  styles: [`
    .dashboard-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: var(--color-bg);
    }

    .top-nav {
      height: 64px;
      padding: 0 40px;
      background-color: var(--color-surface);
      border-bottom: 1px solid var(--color-border);
      display: flex;
      justify-content: space-between;
      align-items: center;
      position: sticky;
      top: 0;
      z-index: 100;
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .logo-icon {
      font-size: 20px;
      color: var(--color-accent);
      font-weight: bold;
    }

    .logo-text {
      font-family: var(--font-heading);
      font-size: 18px;
      font-weight: 600;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .user-avatar {
      width: 34px;
      height: 34px;
      border-radius: 50%;
      background: linear-gradient(135deg, #6366f1, #8b5cf6);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 13px;
      font-weight: 700;
      color: white;
    }

    .user-meta {
      display: flex;
      flex-direction: column;
      gap: 1px;
    }

    .user-name {
      font-weight: 500;
      font-size: 13px;
      line-height: 1.2;
    }

    .user-role {
      font-size: 10px;
      font-weight: 600;
      padding: 1px 6px;
      border-radius: 10px;
      width: fit-content;
    }

    .role-member        { background: rgba(59,130,246,0.15); color: #3b82f6; }
    .role-boardadmin    { background: rgba(168,85,247,0.15); color: #a855f7; }
    .role-platformadmin { background: rgba(245,158,11,0.15); color: #f59e0b; }

    .btn-admin {
      padding: 7px 14px;
      background: rgba(245,158,11,0.12);
      border: 1px solid rgba(245,158,11,0.3);
      border-radius: 6px;
      color: #f59e0b;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }
    .btn-admin:hover, .btn-admin.active {
      background: rgba(245,158,11,0.25);
    }

    .btn-logout {
      font-size: 13px;
      color: var(--color-text-secondary);
      padding: 6px 12px;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      transition: all var(--transition-fast);
      background: transparent;
      cursor: pointer;
    }
    .btn-logout:hover {
      background-color: var(--color-error-light);
      color: var(--color-error);
      border-color: rgba(239, 68, 68, 0.2);
    }

    .content {
      flex: 1;
      padding: 40px;
      max-width: 1200px;
      margin: 0 auto;
      width: 100%;
    }

    .page-header h1 {
      font-family: var(--font-heading);
      font-size: 32px;
      margin-bottom: 8px;
    }
    .page-header p {
      color: var(--color-text-secondary);
      margin-bottom: 32px;
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
      margin-bottom: 28px;
    }

    .card {
      background-color: var(--color-surface);
      padding: 24px;
      border-radius: var(--radius-lg);
      border: 1px solid var(--color-border);
      box-shadow: var(--shadow-sm);
    }

    .card h3 {
      font-family: var(--font-heading);
      font-size: 16px;
      margin-bottom: 14px;
    }

    .card p {
      font-size: 13px;
      color: var(--color-text-secondary);
      margin-bottom: 8px;
    }

    .role-badge {
      display: inline-block;
      padding: 2px 10px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 600;
    }

    .status-badge {
      display: inline-block;
      padding: 2px 10px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 600;
    }
    .status-badge.active { background: rgba(34,197,94,0.15); color: #22c55e; }

    .feature-list {
      padding-left: 4px;
      list-style: none;
      margin: 10px 0 16px;
    }
    .feature-list li {
      font-size: 13px;
      color: var(--color-text-secondary);
      padding: 4px 0;
    }

    .btn-open-admin {
      padding: 8px 16px;
      background: rgba(245,158,11,0.15);
      border: 1px solid rgba(245,158,11,0.3);
      border-radius: 6px;
      color: #f59e0b;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      width: 100%;
      transition: all 0.2s;
    }
    .btn-open-admin:hover { background: rgba(245,158,11,0.25); }
  `]
})
export class DashboardComponent {
  private authService = inject(AuthService);
  user    = this.authService.currentUser;
  showAdmin = false;

  isPlatformAdmin(): boolean {
    return this.user()?.role === 'PlatformAdmin';
  }

  getInitials(name: string): string {
    return (name ?? '?').split(' ').map((p: string) => p[0]).join('').toUpperCase().slice(0, 2);
  }

  onLogout() {
    this.authService.logout();
  }
}
