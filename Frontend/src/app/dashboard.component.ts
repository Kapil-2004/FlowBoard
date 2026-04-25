import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { WorkspaceListComponent } from './workspaces/workspace-list/workspace-list.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, WorkspaceListComponent],
  template: `
    <div class="dashboard-container">
      <nav class="top-nav">
        <div class="logo">
          <span class="logo-icon">≋</span>
          <span class="logo-text">FlowBoard</span>
        </div>
        <div class="user-info" *ngIf="user()">
          <span class="user-name">{{ user()?.fullName }}</span>
          <button (click)="onLogout()" class="btn-logout">Logout</button>
        </div>
      </nav>

      <main class="content">
        <header class="page-header">
          <h1>Welcome, {{ user()?.fullName }}</h1>
          <p>You have successfully authenticated. This is your personal dashboard.</p>
        </header>

        <div class="grid">
          <div class="card">
            <h3>Profile Info</h3>
            <p><strong>Email:</strong> {{ user()?.email }}</p>
            <p><strong>Joined:</strong> {{ user()?.createdAt | date:'mediumDate' }}</p>
          </div>
        </div>

        <!-- Inject Workspace Feature -->
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
      gap: 16px;
    }

    .user-name {
      font-weight: 500;
      font-size: 14px;
    }

    .btn-logout {
      font-size: 13px;
      color: var(--color-text-secondary);
      padding: 6px 12px;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      transition: all var(--transition-fast);
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
      margin-bottom: 40px;
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 24px;
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
      font-size: 18px;
      margin-bottom: 16px;
    }

    .card p {
      font-size: 14px;
      color: var(--color-text-secondary);
      margin-bottom: 8px;
    }

    .placeholder {
      border: 2px dashed var(--color-border);
      background: transparent;
      box-shadow: none;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      text-align: center;
    }
  `]
})
export class DashboardComponent {
  private authService = inject(AuthService);
  user = this.authService.currentUser;

  constructor() { }

  onLogout() {
    this.authService.logout();
  }
}
