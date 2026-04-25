import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WorkspaceService } from '../../services/workspace.service';
import { NotificationService } from '../../services/notification.service';
import { UserService, UserSearchDto } from '../../services/user.service';
import { WorkspaceResponseDto, WorkspaceCreateDto, WorkspaceMemberDto } from '../../models/workspace.models';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-workspace-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="workspace-section">
      <div class="header-actions">
        <h2>Your Workspaces</h2>
        <button class="btn-primary" (click)="toggleCreateForm()">+ New Workspace</button>
      </div>

      <div *ngIf="showCreateForm" class="create-form card">
        <h3>Create Workspace</h3>
        <form (ngSubmit)="onCreate()">
          <div class="form-group">
            <label>Name</label>
            <input type="text" [(ngModel)]="newWorkspace.name" name="name" required placeholder="e.g. Engineering Team" />
          </div>
          <div class="form-group">
            <label>Description</label>
            <input type="text" [(ngModel)]="newWorkspace.description" name="description" placeholder="Optional description" />
          </div>
          <div class="form-actions">
            <button type="submit" class="btn-primary" [disabled]="!newWorkspace.name">Create</button>
            <button type="button" class="btn-secondary" (click)="toggleCreateForm()">Cancel</button>
          </div>
        </form>
      </div>

      <div class="grid">
        <div class="card workspace-card" *ngFor="let ws of workspaces()">
          <div class="ws-header">
            <h3>{{ ws.name }}</h3>
            <span class="badge" [class.private]="ws.visibility === 'PRIVATE'">{{ ws.visibility }}</span>
          </div>
          <p>{{ ws.description || 'No description provided.' }}</p>
          <div class="ws-footer">
            <small>Updated: {{ ws.updatedAt | date:'shortDate' }}</small>
            <div class="actions">
              <button class="btn-icon" (click)="toggleMembers(ws.workspaceId)" title="Manage Members">👥</button>
              <button class="btn-link">View Board →</button>
            </div>
          </div>

          <!-- Member Management Section -->
          <div *ngIf="managedWorkspaceId === ws.workspaceId" class="members-section">
            <h4>Workspace Members</h4>
            <ul class="member-list">
              <li *ngFor="let member of workspaceMembers">
                <span>{{ member.userId | slice:0:8 }}... ({{ member.role }})</span>
                <button class="btn-remove" (click)="onRemoveMember(ws.workspaceId, member.userId)">✕</button>
              </li>
            </ul>

            <div class="add-member">
              <input type="text" placeholder="Search user by name/email..." 
                     (input)="onSearchInput($event)"
                     [(ngModel)]="searchQuery" />
              
              <div class="search-results" *ngIf="searchResults.length > 0">
                <div *ngFor="let user of searchResults" class="search-item" (click)="onAddMember(ws.workspaceId, user.userId)">
                  <span>{{ user.fullName }}</span>
                  <small>{{ user.email }}</small>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="card placeholder" *ngIf="workspaces().length === 0 && !showCreateForm">
          <p>You don't have any workspaces yet.</p>
          <button class="btn-link" (click)="toggleCreateForm()">Create one now</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .workspace-section {
      margin-top: 32px;
    }

    .header-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }

    .header-actions h2 {
      font-family: var(--font-heading);
      font-size: 24px;
      margin: 0;
    }

    .btn-primary {
      background-color: var(--color-primary);
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: var(--radius-sm);
      cursor: pointer;
      font-weight: 500;
      transition: background var(--transition-fast);
    }

    .btn-primary:hover {
      background-color: var(--color-primary-dark);
    }
    
    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-secondary {
      background-color: transparent;
      color: var(--color-text-secondary);
      border: 1px solid var(--color-border);
      padding: 8px 16px;
      border-radius: var(--radius-sm);
      cursor: pointer;
      font-weight: 500;
      margin-left: 8px;
    }

    .btn-link {
      background: none;
      border: none;
      color: var(--color-primary);
      font-weight: 500;
      cursor: pointer;
      padding: 0;
    }

    .create-form {
      margin-bottom: 24px;
      animation: slideDown 0.3s ease-out;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-group label {
      display: block;
      margin-bottom: 8px;
      font-size: 14px;
      font-weight: 500;
    }

    .form-group input {
      width: 100%;
      padding: 10px;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background-color: var(--color-bg);
      color: var(--color-text);
    }

    .form-actions {
      margin-top: 24px;
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

    .workspace-card {
      display: flex;
      flex-direction: column;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

    .workspace-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--shadow-md);
    }

    .ws-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 12px;
    }

    .ws-header h3 {
      font-family: var(--font-heading);
      font-size: 18px;
      margin: 0;
    }

    .badge {
      font-size: 10px;
      padding: 4px 8px;
      border-radius: 12px;
      background: var(--color-border);
      color: var(--color-text-secondary);
      font-weight: bold;
      letter-spacing: 0.5px;
    }

    .badge.private {
      background: rgba(239, 68, 68, 0.1);
      color: var(--color-error);
    }

    .workspace-card p {
      color: var(--color-text-secondary);
      font-size: 14px;
      flex: 1;
      margin-bottom: 16px;
    }

    .ws-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-top: 1px solid var(--color-border);
      padding-top: 16px;
      margin-top: auto;
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

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }

    .ws-footer .actions {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .btn-icon {
      background: none;
      border: none;
      font-size: 18px;
      cursor: pointer;
      padding: 4px;
      border-radius: 4px;
      transition: background 0.2s;
    }

    .btn-icon:hover {
      background: rgba(255, 255, 255, 0.05);
    }

    .members-section {
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid var(--color-border);
      animation: slideDown 0.2s ease-out;
    }

    .members-section h4 {
      font-size: 14px;
      margin-bottom: 12px;
      color: var(--color-text-secondary);
    }

    .member-list {
      list-style: none;
      padding: 0;
      margin: 0 0 16px 0;
    }

    .member-list li {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-size: 13px;
      padding: 6px 0;
    }

    .btn-remove {
      background: none;
      border: none;
      color: var(--color-error);
      cursor: pointer;
      font-size: 12px;
      opacity: 0.6;
    }

    .btn-remove:hover {
      opacity: 1;
    }

    .add-member {
      position: relative;
    }

    .add-member input {
      width: 100%;
      font-size: 12px;
      padding: 8px;
    }

    .search-results {
      position: absolute;
      bottom: 100%;
      left: 0;
      right: 0;
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      max-height: 200px;
      overflow-y: auto;
      z-index: 10;
      box-shadow: var(--shadow-lg);
    }

    .search-item {
      padding: 8px 12px;
      cursor: pointer;
      display: flex;
      flex-direction: column;
    }

    .search-item:hover {
      background: rgba(255, 255, 255, 0.05);
    }

    .search-item span {
      font-size: 13px;
      font-weight: 500;
    }

    .search-item small {
      font-size: 11px;
      color: var(--color-text-secondary);
    }
  `]
})
export class WorkspaceListComponent implements OnInit {
  private workspaceService = inject(WorkspaceService);
  private notificationService = inject(NotificationService);
  private userService = inject(UserService);
  
  workspaces = this.workspaceService.workspaces;

  showCreateForm = false;
  newWorkspace: WorkspaceCreateDto = { name: '', description: '', visibility: 'PRIVATE' };

  managedWorkspaceId: number | null = null;
  workspaceMembers: WorkspaceMemberDto[] = [];
  
  searchQuery = '';
  searchResults: UserSearchDto[] = [];
  private searchSubject = new Subject<string>();

  ngOnInit() {
    this.workspaceService.loadWorkspaces().subscribe();

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => query.length >= 2 ? this.userService.searchUsers(query) : of([]))
    ).subscribe(results => this.searchResults = results);
  }

  toggleCreateForm() {
    this.showCreateForm = !this.showCreateForm;
    if (!this.showCreateForm) {
      this.newWorkspace = { name: '', description: '', visibility: 'PRIVATE' };
    }
  }

  onCreate() {
    if (!this.newWorkspace.name) return;
    this.workspaceService.createWorkspace(this.newWorkspace).subscribe({
      next: () => {
        this.notificationService.success('Workspace created successfully!');
        this.toggleCreateForm();
      },
      error: (err) => {
        this.notificationService.error(err.error?.message || 'Failed to create workspace');
        console.error('Failed to create workspace', err);
      }
    });
  }

  toggleMembers(workspaceId: number) {
    if (this.managedWorkspaceId === workspaceId) {
      this.managedWorkspaceId = null;
      this.workspaceMembers = [];
    } else {
      this.managedWorkspaceId = workspaceId;
      this.loadMembers(workspaceId);
    }
    this.searchQuery = '';
    this.searchResults = [];
  }

  loadMembers(workspaceId: number) {
    this.workspaceService.getMembers(workspaceId).subscribe(members => {
      this.workspaceMembers = members;
    });
  }

  onSearchInput(event: any) {
    this.searchSubject.next((event.target as HTMLInputElement).value);
  }

  onAddMember(workspaceId: number, userId: string) {
    this.workspaceService.addMember(workspaceId, userId).subscribe({
      next: () => {
        this.notificationService.success('Member added successfully!');
        this.loadMembers(workspaceId);
        this.searchQuery = '';
        this.searchResults = [];
      },
      error: (err) => this.notificationService.error(err.error?.message || 'Failed to add member')
    });
  }

  onRemoveMember(workspaceId: number, userId: string) {
    if (!confirm('Are you sure you want to remove this member?')) return;
    
    this.workspaceService.removeMember(workspaceId, userId).subscribe({
      next: () => {
        this.notificationService.success('Member removed');
        this.loadMembers(workspaceId);
      },
      error: (err) => this.notificationService.error(err.error?.message || 'Failed to remove member')
    });
  }
}
