import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { WorkspaceService } from '../../services/workspace.service';
import { NotificationService } from '../../services/notification.service';
import { UserService, UserSearchDto } from '../../services/user.service';
import { WorkspaceResponseDto, WorkspaceCreateDto, WorkspaceMemberDto } from '../../models/workspace.models';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-workspace-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="workspace-container">
      <header class="header">
        <div class="header-left">
          <h1 class="title">Workspaces</h1>
          <p class="subtitle">Manage your teams and projects</p>
        </div>
        <button class="btn-primary" (click)="toggleCreateForm()">
          <span class="icon">+</span>
          New Workspace
        </button>
      </header>

      <div *ngIf="showCreateForm" class="modal-overlay" (click)="toggleCreateForm()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Create Workspace</h3>
            <button class="btn-close" (click)="toggleCreateForm()">×</button>
          </div>
          <form (ngSubmit)="onCreate()">
            <div class="form-group">
              <label>Workspace Name</label>
              <input type="text" [(ngModel)]="newWorkspace.name" name="name" required placeholder="e.g. Engineering Team" />
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea [(ngModel)]="newWorkspace.description" name="description" placeholder="What is this workspace for?" rows="3"></textarea>
            </div>
            <div class="form-group">
              <label>Visibility</label>
              <select [(ngModel)]="newWorkspace.visibility" name="visibility">
                <option value="PRIVATE">Private</option>
                <option value="PUBLIC">Public</option>
              </select>
            </div>
            <div class="form-footer">
              <button type="button" class="btn-ghost" (click)="toggleCreateForm()">Cancel</button>
              <button type="submit" class="btn-primary" [disabled]="!newWorkspace.name">Create Workspace</button>
            </div>
          </form>
        </div>
      </div>

      <div class="workspace-grid">
        <div class="workspace-card" *ngFor="let ws of workspaces()">
          <div class="card-content">
            <div class="card-header">
              <h3 class="ws-name">{{ ws.name }}</h3>
              <span class="badge" [class.private]="ws.visibility === 'PRIVATE'">{{ ws.visibility | lowercase }}</span>
            </div>
            <p class="ws-desc">{{ ws.description || 'Collaborate with your team members in this shared space.' }}</p>
            <div class="card-footer">
              <span class="meta-item">Updated {{ ws.updatedAt | date:'mediumDate' }}</span>
              <div class="card-actions">
                <button class="action-btn" (click)="toggleMembers(ws.workspaceId)" title="Members">👥</button>
                <button class="btn-view" [routerLink]="['/workspaces', ws.workspaceId, 'boards']">
                  View Boards <span class="arrow">→</span>
                </button>
              </div>
            </div>
          </div>

          <!-- Quick Member View Popover -->
          <div *ngIf="managedWorkspaceId === ws.workspaceId" class="members-popover" (click)="$event.stopPropagation()">
            <div class="popover-header">
              <h4>Members</h4>
              <button (click)="managedWorkspaceId = null" class="btn-close-sm">×</button>
            </div>
            <ul class="member-list">
              <li *ngFor="let member of workspaceMembers">
                <div class="member-info">
                  <span class="member-name">UID: {{ member.userId | slice:0:8 }}</span>
                  <span class="member-role">{{ member.role }}</span>
                </div>
                <button class="btn-remove-sm" (click)="onRemoveMember(ws.workspaceId, member.userId)">✕</button>
              </li>
            </ul>
            <div class="popover-search">
              <input type="text" placeholder="Invite user..." 
                     (input)="onSearchInput($event)"
                     [(ngModel)]="searchQuery" />
              <div class="search-dropdown" *ngIf="searchResults.length > 0">
                <div *ngFor="let user of searchResults" class="search-item" (click)="onAddMember(ws.workspaceId, user.userId)">
                  <span class="user-name">{{ user.fullName }}</span>
                  <span class="user-email">{{ user.email }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="empty-state" *ngIf="workspaces().length === 0 && !showCreateForm">
          <div class="empty-icon">🏢</div>
          <h3>No workspaces found</h3>
          <p>Create a workspace to group your boards and team members.</p>
          <button class="btn-primary" (click)="toggleCreateForm()">+ Create Workspace</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .workspace-container { padding: 40px; max-width: 1400px; margin: 0 auto; animation: fadeIn var(--transition-slow); }
    .header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 48px; }
    .title { font-family: var(--font-heading); font-size: 32px; font-weight: 700; letter-spacing: -0.02em; margin-bottom: 4px; }
    .subtitle { color: var(--color-text-secondary); font-size: 14px; }

    .btn-primary { background-color: #fff; color: #000; padding: 12px 24px; border-radius: var(--radius-md); font-weight: 600; font-size: 14px; display: flex; align-items: center; gap: 8px; transition: all var(--transition-fast); }
    .btn-primary:hover { background-color: #f0f0f0; transform: translateY(-2px); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-ghost { color: var(--color-text-secondary); padding: 12px 24px; font-weight: 500; }

    .workspace-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 32px; }
    .workspace-card { background-color: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-lg); padding: 32px; position: relative; transition: all var(--transition-normal); display: flex; flex-direction: column; }
    .workspace-card:hover { border-color: rgba(255, 255, 255, 0.3); transform: translateY(-4px); box-shadow: var(--shadow-md); }

    .card-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 16px; }
    .ws-name { font-family: var(--font-heading); font-size: 20px; font-weight: 700; margin: 0; }
    .badge { font-size: 10px; font-weight: 700; text-transform: uppercase; padding: 4px 8px; border-radius: 4px; background: var(--color-accent-light); color: var(--color-text-secondary); border: 1px solid var(--color-border); }
    .badge.private { border-color: var(--color-border); }

    .ws-desc { font-size: 14px; color: var(--color-text-secondary); line-height: 1.6; margin-bottom: 32px; flex: 1; }
    .card-footer { display: flex; justify-content: space-between; align-items: center; padding-top: 24px; border-top: 1px solid var(--color-border); }
    .meta-item { font-size: 11px; color: var(--color-text-placeholder); }

    .card-actions { display: flex; align-items: center; gap: 16px; }
    .action-btn { color: var(--color-text-secondary); font-size: 18px; transition: color 0.2s; }
    .action-btn:hover { color: #fff; }

    .btn-view { color: #fff; font-size: 13px; font-weight: 600; display: flex; align-items: center; gap: 8px; transition: color 0.2s; }
    .btn-view:hover { color: var(--color-text-secondary); }
    .btn-view .arrow { transition: transform 0.2s; }
    .btn-view:hover .arrow { transform: translateX(4px); }

    .modal-overlay { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.85); backdrop-filter: blur(10px); z-index: 1000; display: flex; align-items: center; justify-content: center; }
    .modal-content { background: var(--color-surface); width: 100%; max-width: 500px; border-radius: var(--radius-lg); border: 1px solid var(--color-border); padding: 40px; animation: modalSlide 0.3s ease-out; }
    .modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
    .modal-header h3 { font-family: var(--font-heading); font-size: 24px; font-weight: 700; }
    .btn-close { font-size: 28px; color: var(--color-text-secondary); }

    .members-popover { position: absolute; top: 100%; right: 0; width: 300px; background: var(--color-surface-hover); border: 1px solid var(--color-border); border-radius: var(--radius-md); padding: 20px; margin-top: 12px; box-shadow: var(--shadow-lg); z-index: 100; animation: fadeIn 0.2s ease-out; }
    .popover-header { display: flex; justify-content: space-between; margin-bottom: 16px; }
    .popover-header h4 { font-size: 14px; font-weight: 700; }
    .btn-close-sm { font-size: 18px; color: var(--color-text-secondary); }

    .member-list { list-style: none; margin-bottom: 16px; max-height: 120px; overflow-y: auto; }
    .member-list li { display: flex; justify-content: space-between; align-items: center; padding: 8px 0; border-bottom: 1px solid var(--color-border); }
    .member-info { display: flex; flex-direction: column; gap: 2px; }
    .member-name { font-size: 12px; font-weight: 600; }
    .member-role { font-size: 10px; color: var(--color-text-placeholder); text-transform: uppercase; }
    .btn-remove-sm { color: var(--color-error); font-size: 12px; opacity: 0.6; }
    .btn-remove-sm:hover { opacity: 1; }

    .popover-search input { width: 100%; background: var(--color-bg); border: 1px solid var(--color-border); padding: 10px; border-radius: 4px; font-size: 12px; color: #fff; }
    .search-dropdown { position: absolute; top: 100%; left: 20px; right: 20px; background: var(--color-surface); border: 1px solid var(--color-border); max-height: 150px; overflow-y: auto; z-index: 101; }
    .search-item { padding: 10px; cursor: pointer; border-bottom: 1px solid var(--color-border); }
    .search-item:hover { background: var(--color-accent-light); }
    .user-name { font-size: 12px; font-weight: 600; display: block; }
    .user-email { font-size: 10px; color: var(--color-text-secondary); }

    .form-group { margin-bottom: 24px; }
    label { display: block; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; color: var(--color-text-secondary); margin-bottom: 10px; }
    input[type="text"], textarea, select { width: 100%; background: var(--color-bg); border: 1px solid var(--color-border); padding: 14px; border-radius: 4px; color: #fff; font-size: 14px; }
    input:focus, textarea:focus, select:focus { outline: none; border-color: #fff; }
    .form-footer { display: flex; justify-content: flex-end; gap: 16px; margin-top: 40px; }

    .empty-state { grid-column: 1 / -1; padding: 100px 0; text-align: center; }
    .empty-icon { font-size: 64px; margin-bottom: 24px; opacity: 0.3; }
    .empty-state h3 { font-size: 24px; margin-bottom: 12px; }
    .empty-state p { color: var(--color-text-secondary); margin-bottom: 32px; max-width: 400px; margin-left: auto; margin-right: auto; }

    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
    @keyframes modalSlide { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
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
