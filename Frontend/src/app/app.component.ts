import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NotificationService } from './services/notification.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <router-outlet></router-outlet>
    
    <!-- Toast Notifications -->
    <div class="toast-container">
      <div *ngFor="let n of notifications()" class="toast" [class]="n.type">
        {{ n.message }}
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 24px;
      right: 24px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .toast {
      padding: 12px 24px;
      border-radius: var(--radius-sm);
      color: white;
      font-weight: 500;
      box-shadow: var(--shadow-md);
      animation: slideIn 0.3s ease-out;
      min-width: 250px;
    }

    .toast.success { background-color: var(--color-success); border-left: 4px solid rgba(255,255,255,0.3); }
    .toast.error { background-color: var(--color-error); border-left: 4px solid rgba(255,255,255,0.3); }
    .toast.info { background-color: var(--color-info); border-left: 4px solid rgba(255,255,255,0.3); }

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class AppComponent {
  notificationService = inject(NotificationService);
  notifications = this.notificationService.notifications;
  title = 'FlowBoard-Web';
}
