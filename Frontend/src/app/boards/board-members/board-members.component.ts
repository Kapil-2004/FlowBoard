import { Component, inject, OnInit, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

interface BoardMemberDto {
  boardMemberId: number;
  boardId: number;
  userId: string;
  role: string;      // OBSERVER | MEMBER | ADMIN
  addedAt: string;
  fullName?: string;
  email?: string;
}

interface UserSearchDto {
  userId: string;
  fullName: string;
  email: string;
  avatarUrl: string | null;
}

@Component({
  selector: 'app-board-members',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="members-panel">
      <div class="panel-header">
        <h3>👥 Board Members</h3>
        <button class="btn-close" (click)="onClosePanel()">✕</button>
      </div>

      <!-- Your role badge -->
      <div class="your-role" *ngIf="yourRole()">
        Your board role: <span class="role-badge role-{{ yourRole()!.toLowerCase() }}">{{ yourRole() }}</span>
      </div>

      <!-- Add Member (BoardAdmin / PlatformAdmin only) -->
      <div class="add-member-section" *ngIf="canManage()">
        <div class="section-title">Add Member</div>
        <div class="search-row">
          <input
            id="member-search-input"
            class="input"
            type="text"
            placeholder="Search user by name or email..."
            [(ngModel)]="searchQuery"
            (input)="onSearchInput()"
          />
        </div>

        <!-- Search results -->
        <div class="search-results" *ngIf="searchResults().length > 0">
          <div
            class="search-result-item"
            *ngFor="let u of searchResults()"
            (click)="onSelectUser(u)"
          >
            <div class="avatar">{{ getInitials(u.fullName) }}</div>
            <div>
              <div class="result-name">{{ u.fullName }}</div>
              <div class="result-email">{{ u.email }}</div>
            </div>
          </div>
        </div>

        <!-- Selected user + role picker -->
        <div class="selected-user" *ngIf="selectedUser()">
          <div class="selected-info">
            <div class="avatar">{{ getInitials(selectedUser()!.fullName) }}</div>
            <div>
              <div class="result-name">{{ selectedUser()!.fullName }}</div>
              <div class="result-email">{{ selectedUser()!.email }}</div>
            </div>
            <button class="btn-clear" (click)="clearSelection()">✕</button>
          </div>
          <div class="role-row">
            <label>Board Role:</label>
            <select id="add-member-role" class="role-select" [(ngModel)]="addRole">
              <option value="OBSERVER">Observer (view only)</option>
              <option value="MEMBER">Member (full access)</option>
              <option value="ADMIN">Admin (manage board)</option>
            </select>
            <button id="btn-add-member" class="btn-add" (click)="onAddMember()" [disabled]="addingMember()">
              {{ addingMember() ? 'Adding...' : 'Add to Board' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Toast -->
      <div class="toast" *ngIf="toast()" [class]="'toast ' + toastType()">{{ toast() }}</div>
      <div class="error-msg" *ngIf="error()">{{ error() }}</div>
      <div class="loading" *ngIf="loading()">Loading members...</div>

      <!-- Members List -->
      <div class="section-title" style="margin-top: 16px;">Current Members</div>
      <div class="members-list" *ngIf="!loading()">
        <div class="member-row" *ngFor="let m of members()">
          <div class="avatar">{{ getInitials(m.fullName ?? m.userId) }}</div>
          <div class="member-info">
            <div class="member-name">{{ m.fullName ?? 'Unknown User' }}</div>
            <div class="member-email">{{ m.email ?? m.userId }}</div>
          </div>

          <!-- Role change (admin only) -->
          <select
            *ngIf="canManage() && !isCurrentUser(m)"
            [id]="'board-role-' + m.boardMemberId"
            class="role-select-sm"
            [ngModel]="m.role"
            (ngModelChange)="onChangeMemberRole(m, $event)"
          >
            <option value="OBSERVER">Observer</option>
            <option value="MEMBER">Member</option>
            <option value="ADMIN">Admin</option>
          </select>

          <span *ngIf="!canManage() || isCurrentUser(m)" class="role-badge role-{{ m.role.toLowerCase() }}">
            {{ m.role }}
          </span>

          <!-- Remove (admin only, can't remove self) -->
          <button
            *ngIf="canManage() && !isCurrentUser(m)"
            class="btn-remove"
            [id]="'remove-member-' + m.boardMemberId"
            (click)="onRemoveMember(m)"
            title="Remove from board"
          >🗑</button>

          <span *ngIf="isCurrentUser(m)" class="you-badge">You</span>
        </div>

        <div class="empty" *ngIf="members().length === 0">No members yet.</div>
      </div>
    </div>
  `,
  styles: [`
    .members-panel {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: 10px;
      padding: 20px;
      width: 360px;
      max-height: 80vh;
      overflow-y: auto;
      box-shadow: var(--shadow-lg);
    }

    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 14px;
    }
    .panel-header h3 { font-size: 16px; font-weight: 700; margin: 0; }
    .btn-close {
      background: transparent;
      border: 1px solid var(--color-border);
      border-radius: 4px;
      color: var(--color-text-secondary);
      padding: 4px 8px;
      cursor: pointer;
      font-size: 12px;
    }
    .btn-close:hover { color: var(--color-error); border-color: var(--color-error); }

    .your-role {
      font-size: 12px;
      color: var(--color-text-secondary);
      margin-bottom: 14px;
    }

    .add-member-section {
      background: var(--color-bg);
      border: 1px solid var(--color-border);
      border-radius: 8px;
      padding: 14px;
      margin-bottom: 16px;
    }

    .section-title {
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: var(--color-text-secondary);
      margin-bottom: 10px;
    }

    .input {
      width: 100%;
      padding: 8px 12px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: 6px;
      color: var(--color-text);
      font-size: 13px;
      box-sizing: border-box;
    }

    .search-results {
      margin-top: 8px;
      border: 1px solid var(--color-border);
      border-radius: 6px;
      overflow: hidden;
      max-height: 160px;
      overflow-y: auto;
    }

    .search-result-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px 12px;
      cursor: pointer;
      transition: background 0.15s;
    }
    .search-result-item:hover { background: var(--color-bg); }

    .selected-user {
      margin-top: 10px;
    }
    .selected-info {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 10px;
    }
    .btn-clear {
      background: transparent;
      border: none;
      color: var(--color-text-secondary);
      cursor: pointer;
      font-size: 12px;
      margin-left: auto;
    }

    .role-row {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .role-row label { font-size: 12px; color: var(--color-text-secondary); white-space: nowrap; }

    .role-select {
      flex: 1;
      padding: 6px 8px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: 5px;
      color: var(--color-text);
      font-size: 12px;
    }
    .role-select-sm {
      padding: 4px 8px;
      background: var(--color-bg);
      border: 1px solid var(--color-border);
      border-radius: 5px;
      color: var(--color-text);
      font-size: 12px;
    }

    .btn-add {
      padding: 6px 12px;
      background: var(--color-accent);
      color: white;
      border: none;
      border-radius: 5px;
      font-size: 12px;
      font-weight: 600;
      cursor: pointer;
      white-space: nowrap;
    }
    .btn-add:disabled { opacity: 0.5; cursor: not-allowed; }

    .avatar {
      width: 30px;
      height: 30px;
      border-radius: 50%;
      background: linear-gradient(135deg, #6366f1, #8b5cf6);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 11px;
      font-weight: 700;
      color: white;
      flex-shrink: 0;
    }

    .result-name  { font-size: 13px; font-weight: 500; color: var(--color-text); }
    .result-email { font-size: 11px; color: var(--color-text-secondary); }

    .toast { padding: 8px 12px; border-radius: 6px; font-size: 12px; margin-bottom: 10px; }
    .toast.success { background: rgba(34,197,94,0.15); color: #22c55e; }
    .toast.error   { background: rgba(239,68,68,0.12); color: #ef4444; }
    .error-msg { color: #ef4444; font-size: 12px; margin-bottom: 8px; }
    .loading   { color: var(--color-text-secondary); font-size: 13px; padding: 10px 0; }

    .members-list { display: flex; flex-direction: column; gap: 6px; }

    .member-row {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px 10px;
      border-radius: 7px;
      background: var(--color-bg);
      border: 1px solid var(--color-border);
    }

    .member-info { flex: 1; min-width: 0; }
    .member-name  { font-size: 13px; font-weight: 500; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .member-email { font-size: 11px; color: var(--color-text-secondary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

    .role-badge {
      padding: 3px 8px;
      border-radius: 12px;
      font-size: 10px;
      font-weight: 600;
      white-space: nowrap;
    }
    .role-observer { background: rgba(107,114,128,0.2); color: #9ca3af; }
    .role-member   { background: rgba(59,130,246,0.15); color: #3b82f6; }
    .role-admin    { background: rgba(168,85,247,0.15); color: #a855f7; }

    .btn-remove {
      background: transparent;
      border: 1px solid rgba(239,68,68,0.2);
      color: #ef4444;
      border-radius: 4px;
      padding: 3px 7px;
      font-size: 11px;
      cursor: pointer;
      flex-shrink: 0;
    }
    .btn-remove:hover { background: rgba(239,68,68,0.1); }

    .you-badge {
      background: rgba(99,102,241,0.15);
      color: #6366f1;
      border-radius: 10px;
      padding: 2px 8px;
      font-size: 10px;
      font-weight: 600;
    }

    .empty { text-align: center; color: var(--color-text-secondary); font-size: 13px; padding: 12px; }
  `]
})
export class BoardMembersComponent implements OnInit {
  @Input() boardId!: number;
  @Input() onClose!: () => void;

  private http     = inject(HttpClient);
  private authSvc  = inject(AuthService);

  members       = signal<BoardMemberDto[]>([]);
  yourRole      = signal<string | null>(null);
  loading       = signal(true);
  error         = signal('');
  toast         = signal('');
  toastType     = signal<'success'|'error'>('success');
  searchResults = signal<UserSearchDto[]>([]);
  selectedUser  = signal<UserSearchDto | null>(null);
  addingMember  = signal(false);

  searchQuery = '';
  addRole     = 'MEMBER';

  private searchTimer: any;
  private BOARD_API  = 'http://localhost:5003/api/boards';
  private AUTH_API   = 'http://localhost:5001/api/auth';

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.authSvc.getToken()}` });
  }

  get currentUserId(): string | undefined {
    return this.authSvc.currentUser()?.userId ?? this.authSvc.currentUser()?.id;
  }

  canManage(): boolean {
    const role = this.yourRole();
    const platformRole = this.authSvc.currentUser()?.role;
    return role === 'ADMIN' || platformRole === 'PlatformAdmin' || platformRole === 'BoardAdmin';
  }

  isCurrentUser(m: BoardMemberDto): boolean {
    return m.userId === this.currentUserId;
  }

  ngOnInit() { this.loadMembers(); }

  loadMembers() {
    this.loading.set(true);
    this.http.get<BoardMemberDto[]>(`${this.BOARD_API}/${this.boardId}/members`, { headers: this.headers() })
      .subscribe({
        next: members => {
          this.members.set(members);
          const mine = members.find(m => m.userId === this.currentUserId);
          this.yourRole.set(mine?.role ?? null);
          this.loading.set(false);
        },
        error: e => { this.error.set('Failed to load members.'); this.loading.set(false); }
      });
  }

  onSearchInput() {
    clearTimeout(this.searchTimer);
    if (this.searchQuery.length < 2) { this.searchResults.set([]); return; }
    this.searchTimer = setTimeout(() => {
      this.http.get<any>(`${this.AUTH_API}/users/search?q=${encodeURIComponent(this.searchQuery)}`, { headers: this.headers() })
        .subscribe({
          next: r => this.searchResults.set(r.data ?? []),
          error: () => this.searchResults.set([])
        });
    }, 300);
  }

  onSelectUser(u: UserSearchDto) {
    this.selectedUser.set(u);
    this.searchQuery = u.fullName;
    this.searchResults.set([]);
  }

  clearSelection() {
    this.selectedUser.set(null);
    this.searchQuery = '';
  }

  onAddMember() {
    const user = this.selectedUser();
    if (!user) return;
    this.addingMember.set(true);
    this.http.post<BoardMemberDto>(
      `${this.BOARD_API}/${this.boardId}/members`,
      { userId: user.userId, role: this.addRole },
      { headers: this.headers() }
    ).subscribe({
      next: m => {
        this.members.update(list => [...list, { ...m, fullName: user.fullName, email: user.email }]);
        this.clearSelection();
        this.addingMember.set(false);
        this.showToast(`${user.fullName} added as ${this.addRole}`, 'success');
      },
      error: e => {
        this.addingMember.set(false);
        this.showToast(e?.error?.message ?? 'Failed to add member.', 'error');
      }
    });
  }

  onChangeMemberRole(m: BoardMemberDto, newRole: string) {
    if (newRole === m.role) return;
    this.http.put(
      `${this.BOARD_API}/${this.boardId}/members/${m.userId}`,
      { role: newRole },
      { headers: this.headers() }
    ).subscribe({
      next: () => {
        this.members.update(list => list.map(x => x.boardMemberId === m.boardMemberId ? { ...x, role: newRole } : x));
        this.showToast(`Role changed to ${newRole}`, 'success');
      },
      error: e => this.showToast(e?.error?.message ?? 'Failed to update role.', 'error')
    });
  }

  onRemoveMember(m: BoardMemberDto) {
    const name = m.fullName ?? 'this user';
    if (!confirm(`Remove ${name} from this board?`)) return;
    this.http.delete(
      `${this.BOARD_API}/${this.boardId}/members/${m.userId}`,
      { headers: this.headers() }
    ).subscribe({
      next: () => {
        this.members.update(list => list.filter(x => x.boardMemberId !== m.boardMemberId));
        this.showToast(`${name} removed.`, 'success');
      },
      error: e => this.showToast(e?.error?.message ?? 'Failed to remove.', 'error')
    });
  }

  onClosePanel() { if (this.onClose) this.onClose(); }

  getInitials(name: string): string {
    return (name ?? '?').split(' ').map(p => p[0]).join('').toUpperCase().slice(0, 2);
  }

  private showToast(msg: string, type: 'success'|'error') {
    this.toast.set(msg);
    this.toastType.set(type);
    setTimeout(() => this.toast.set(''), 3000);
  }
}
