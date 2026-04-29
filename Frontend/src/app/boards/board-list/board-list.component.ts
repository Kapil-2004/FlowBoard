import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BoardService } from '../../services/board.service';
import { NotificationService } from '../../services/notification.service';
import { UserService, UserSearchDto } from '../../services/user.service';
import { BoardResponseDto, BoardCreateDto, BoardMemberDto } from '../../models/board.models';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-board-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="board-container">
      <header class="header">
        <div class="header-left">
          <h1 class="title">Boards</h1>
          <p class="subtitle">Organize and track your team's progress</p>
        </div>
        <button class="btn-primary" (click)="toggleCreateForm()">
          <span class="icon">+</span>
          Create New Board
        </button>
      </header>

      <div *ngIf="showCreateForm" class="modal-overlay" (click)="toggleCreateForm()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Create a new board</h3>
            <button class="btn-close" (click)="toggleCreateForm()">×</button>
          </div>
          <form (ngSubmit)="onCreate()" class="modal-form">
            <div class="form-group">
              <label>Board Name</label>
              <input type="text" [(ngModel)]="newBoard.name" name="name" required placeholder="e.g. Q2 Roadmap" />
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea [(ngModel)]="newBoard.description" name="description" placeholder="What is this board for?" rows="3"></textarea>
            </div>
            <div class="form-row">
              <div class="form-group half">
                <label>Accent Color</label>
                <div class="color-picker-wrapper">
                  <input type="color" [(ngModel)]="newBoard.background" name="background" />
                  <span class="color-hex">{{ newBoard.background }}</span>
                </div>
              </div>
              <div class="form-group half">
                <label>Visibility</label>
                <select [(ngModel)]="newBoard.visibility" name="visibility">
                  <option value="PRIVATE">Private</option>
                  <option value="PUBLIC">Public</option>
                </select>
              </div>
            </div>
            <div class="form-footer">
              <button type="button" class="btn-secondary" (click)="toggleCreateForm()">Cancel</button>
              <button type="submit" class="btn-primary" [disabled]="!newBoard.name">Create Board</button>
            </div>
          </form>
        </div>
      </div>

      <div class="board-grid">
        <div class="board-card" *ngFor="let board of boards()">
          <div class="card-link" [routerLink]="['/boards', board.boardId]">
            <div class="board-accent" [style.background]="board.background"></div>
            <div class="card-content">
              <div class="card-header">
                <h3 class="board-name">{{ board.name }}</h3>
                <div class="badges">
                  <span class="badge" [class.private]="board.visibility === 'PRIVATE'">{{ board.visibility | lowercase }}</span>
                  <span *ngIf="board.isClosed" class="badge closed">closed</span>
                </div>
              </div>
              <p class="board-desc">{{ board.description || 'No description provided.' }}</p>
              <div class="card-footer">
                <span class="date">{{ board.createdAt | date:'mediumDate' }}</span>
                <div class="card-actions" (click)="$event.stopPropagation()">
                  <button class="action-btn" (click)="toggleMembers(board.boardId)" [class.active]="managedBoardId === board.boardId" title="Members">
                    👥 <span class="count">{{ board.members.length }}</span>
                  </button>
                  <button *ngIf="!board.isClosed" class="action-btn" (click)="onCloseBoard(board.boardId)" title="Close">🔒</button>
                  <button class="action-btn danger" (click)="onDeleteBoard(board.boardId)" title="Delete">🗑️</button>
                </div>
              </div>
            </div>
          </div>

          <!-- Quick Member View Popover -->
          <div *ngIf="managedBoardId === board.boardId" class="members-popover" (click)="$event.stopPropagation()">
            <div class="popover-header">
              <h4>Board Members</h4>
              <button (click)="managedBoardId = null" class="btn-close-sm">×</button>
            </div>
            <ul class="member-list">
              <li *ngFor="let member of boardMembers">
                <div class="member-info">
                  <span class="member-name">UID: {{ member.userId | slice:0:8 }}</span>
                  <select [(ngModel)]="member.role" (change)="onUpdateMemberRole(member.boardMemberId, member.role)" class="role-select">
                    <option value="OBSERVER">Observer</option>
                    <option value="MEMBER">Member</option>
                    <option value="ADMIN">Admin</option>
                  </select>
                </div>
                <button class="btn-remove-sm" (click)="onRemoveMember(member.boardMemberId)" title="Remove Member">✕</button>
              </li>
            </ul>
            <div class="popover-search">
              <input type="text" placeholder="Invite users..." 
                     (input)="onSearchInput($event)"
                     [(ngModel)]="searchQuery" />
              <div class="search-dropdown" *ngIf="searchResults.length > 0">
                <div *ngFor="let user of searchResults" class="search-item" (click)="onAddMember(board.boardId, user.userId)">
                  <span class="user-name">{{ user.fullName }}</span>
                  <span class="user-email">{{ user.email }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="empty-state" *ngIf="boards().length === 0 && !showCreateForm">
          <div class="empty-icon">≋</div>
          <h3>No boards found</h3>
          <p>Create your first board to start tracking your team's progress.</p>
          <button class="btn-primary" (click)="toggleCreateForm()">+ New Board</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .board-container {
      padding: 64px 40px;
      max-width: 1400px;
      margin: 0 auto;
      animation: fadeIn var(--transition-slow);
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      margin-bottom: 56px;
    }

    .title {
      font-family: var(--font-heading);
      font-size: 40px;
      font-weight: 800;
      letter-spacing: -0.03em;
      margin-bottom: 8px;
    }

    .subtitle {
      color: var(--color-text-secondary);
      font-size: 16px;
    }

    /* Buttons */
    .btn-primary {
      background-color: var(--color-accent);
      color: var(--color-bg);
      padding: 14px 28px;
      border-radius: var(--radius-md);
      font-weight: 700;
      font-size: 15px;
      display: flex;
      align-items: center;
      gap: 10px;
      box-shadow: 0 4px 12px rgba(255, 255, 255, 0.1);
    }

    .btn-primary:hover:not(:disabled) {
      background-color: var(--color-accent-hover);
      transform: translateY(-2px);
      box-shadow: 0 8px 20px rgba(255, 255, 255, 0.15);
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-secondary {
      color: var(--color-text-primary);
      background: var(--color-surface-hover);
      padding: 14px 28px;
      border-radius: var(--radius-md);
      font-weight: 600;
    }

    .btn-secondary:hover {
      background: var(--color-border);
    }

    /* Grid & Cards */
    .board-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
      gap: 32px;
    }

    .board-card {
      position: relative;
    }

    .card-link {
      background-color: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      overflow: hidden;
      transition: all var(--transition-normal);
      cursor: pointer;
      display: flex;
      flex-direction: column;
      height: 100%;
      text-decoration: none;
      color: inherit;
    }

    .card-link:hover {
      border-color: rgba(255, 255, 255, 0.3);
      transform: translateY(-6px);
      box-shadow: var(--shadow-lg), var(--shadow-glow);
      background: var(--color-surface-hover);
    }

    .board-accent {
      height: 4px;
      width: 100%;
      opacity: 0.8;
    }

    .card-content {
      padding: 28px;
      display: flex;
      flex-direction: column;
      flex: 1;
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 16px;
    }

    .board-name {
      font-family: var(--font-heading);
      font-size: 20px;
      font-weight: 700;
      margin: 0;
      letter-spacing: -0.01em;
    }

    .badges {
      display: flex;
      gap: 8px;
    }

    .badge {
      font-size: 9px;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      padding: 4px 8px;
      border-radius: 4px;
      background: rgba(255, 255, 255, 0.05);
      color: var(--color-text-secondary);
      border: 1px solid var(--color-border);
    }

    .badge.closed {
      background: var(--color-error-light);
      color: var(--color-error);
      border-color: rgba(239, 68, 68, 0.2);
    }

    .board-desc {
      font-size: 14px;
      color: var(--color-text-secondary);
      margin-bottom: 32px;
      line-height: 1.6;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      flex: 1;
    }

    .card-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: auto;
      padding-top: 20px;
      border-top: 1px solid var(--color-border);
    }

    .date {
      font-size: 12px;
      color: var(--color-text-placeholder);
      font-weight: 500;
    }

    .card-actions {
      display: flex;
      gap: 4px;
    }

    .action-btn {
      width: 36px;
      height: 36px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-md);
      font-size: 16px;
      color: var(--color-text-secondary);
      transition: all var(--transition-fast);
    }

    .action-btn:hover, .action-btn.active {
      background: rgba(255, 255, 255, 0.08);
      color: var(--color-text-primary);
    }

    .action-btn.danger:hover {
      background: var(--color-error-light);
      color: var(--color-error);
    }

    .action-btn .count {
      font-size: 11px;
      margin-left: 4px;
      font-weight: 700;
    }

    /* Modal */
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.4);
      backdrop-filter: var(--glass-blur);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
      animation: fadeIn var(--transition-fast);
    }

    .modal-content {
      background: var(--glass-bg);
      backdrop-filter: blur(24px);
      width: 100%;
      max-width: 520px;
      border-radius: var(--radius-lg);
      border: 1px solid var(--glass-border);
      padding: 40px;
      box-shadow: var(--shadow-lg);
      animation: modalSlideUp var(--transition-normal);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
    }

    .modal-header h3 {
      font-family: var(--font-heading);
      font-size: 24px;
      font-weight: 800;
      letter-spacing: -0.02em;
    }

    .btn-close {
      font-size: 28px;
      color: var(--color-text-secondary);
      line-height: 1;
    }

    .btn-close:hover { color: #fff; }

    .modal-form .form-group {
      margin-bottom: 24px;
    }

    .modal-form .form-row {
      display: flex;
      gap: 20px;
      margin-bottom: 24px;
    }

    .color-picker-wrapper {
      display: flex;
      align-items: center;
      gap: 12px;
      background: var(--color-bg);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      padding: 6px 12px;
    }

    input[type="color"] {
      border: none;
      width: 28px;
      height: 28px;
      padding: 0;
      background: none;
      cursor: pointer;
    }

    .color-hex {
      font-family: 'JetBrains Mono', monospace;
      font-size: 13px;
      color: var(--color-text-secondary);
      font-weight: 500;
    }

    .form-footer {
      display: flex;
      justify-content: flex-end;
      gap: 16px;
      margin-top: 40px;
    }

    /* Popover */
    .members-popover {
      position: absolute;
      bottom: 80px;
      right: 28px;
      width: 320px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      padding: 24px;
      box-shadow: var(--shadow-lg);
      z-index: 100;
      animation: popoverIn var(--transition-normal);
    }

    @keyframes popoverIn {
      from { opacity: 0; transform: translateY(10px) scale(0.95); }
      to { opacity: 1; transform: translateY(0) scale(1); }
    }

    .popover-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .popover-header h4 { font-size: 14px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; }

    .btn-close-sm { font-size: 20px; color: var(--color-text-placeholder); }
    .btn-close-sm:hover { color: #fff; }

    .member-list {
      list-style: none;
      max-height: 200px;
      overflow-y: auto;
      margin-bottom: 20px;
    }

    .member-list li {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid var(--color-border);
    }

    .member-list li:last-child { border-bottom: none; }

    .member-info { display: flex; flex-direction: column; gap: 6px; }
    .member-name { font-size: 12px; font-weight: 600; color: var(--color-text-primary); }

    .role-select { padding: 4px 8px; font-size: 11px; width: auto; font-weight: 600; }

    .btn-remove-sm { font-size: 12px; color: var(--color-error); padding: 4px; border-radius: 4px; }
    .btn-remove-sm:hover { background: var(--color-error-light); }

    .popover-search { position: relative; }
    .popover-search input { font-size: 13px; padding: 10px 14px; }

    .search-dropdown {
      position: absolute;
      bottom: calc(100% + 8px);
      left: 0;
      right: 0;
      background: var(--color-surface-hover);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      max-height: 160px;
      overflow-y: auto;
      box-shadow: var(--shadow-lg);
    }

    .search-item {
      padding: 12px 16px;
      cursor: pointer;
      display: flex;
      flex-direction: column;
      gap: 2px;
      border-bottom: 1px solid var(--color-border);
    }

    .search-item:last-child { border-bottom: none; }
    .search-item:hover { background: rgba(255,255,255,0.05); }
    .user-name { font-size: 13px; font-weight: 600; }
    .user-email { font-size: 11px; color: var(--color-text-secondary); }

    /* Empty State */
    .empty-state {
      grid-column: 1 / -1;
      padding: 120px 0;
      text-align: center;
      background: var(--color-surface);
      border: 1px dashed var(--color-border);
      border-radius: var(--radius-lg);
    }

    .empty-icon { font-size: 64px; margin-bottom: 24px; opacity: 0.3; }
    .empty-state h3 { font-size: 24px; font-weight: 700; margin-bottom: 12px; }
    .empty-state p { color: var(--color-text-secondary); margin-bottom: 32px; max-width: 400px; margin-left: auto; margin-right: auto; }

    /* Animations */
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes modalSlideUp {
      from { transform: translateY(40px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
  `]

})
export class BoardListComponent implements OnInit {
  private boardService = inject(BoardService);
  private notificationService = inject(NotificationService);
  private userService = inject(UserService);
  private route = inject(ActivatedRoute);

  boards = signal<BoardResponseDto[]>([]);
  showCreateForm = false;
  managedBoardId: number | null = null;
  boardMembers: BoardMemberDto[] = [];
  searchResults: UserSearchDto[] = [];
  searchQuery = '';

  newBoard: BoardCreateDto = {
    name: '',
    description: '',
    background: '#FFFFFF',
    visibility: 'PRIVATE'
  };

  private searchSubject = new Subject<string>();

  ngOnInit() {
    this.loadBoards();
    this.setupSearch();
  }

  private setupSearch() {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => this.userService.searchUsers(query))
    ).subscribe(results => {
      this.searchResults = results;
    });
  }

  loadBoards() {
    const workspaceId = this.route.snapshot.paramMap.get('workspaceId');
    
    if (workspaceId) {
      this.boardService.getBoardsByWorkspace(parseInt(workspaceId)).subscribe({
        next: (data) => {
          this.boards.set(data);
        },
        error: (err) => {
          this.notificationService.error('Failed to load boards');
        }
      });
    } else {
      this.boardService.getBoardsByMember().subscribe({
        next: (data) => {
          console.log('Member boards loaded:', data);
          this.boards.set(data);
        },
        error: (err) => {
          console.error('Failed to load member boards', err);
          this.notificationService.error('Failed to load boards');
        }
      });
    }
  }

  toggleCreateForm() {
    this.showCreateForm = !this.showCreateForm;
  }

  onCreate() {
    const workspaceId = this.route.snapshot.paramMap.get('workspaceId');
    if (!workspaceId) {
      this.notificationService.error('Workspace not found');
      return;
    }

    const dto: BoardCreateDto = {
      ...this.newBoard,
      workspaceId: parseInt(workspaceId)
    };

    this.boardService.createBoard(dto).subscribe({
      next: (board) => {
        console.log('Board created successfully:', board);
        this.boards.set([...this.boards(), board]);
        this.resetForm();
        this.notificationService.success('Board created successfully');
      },
      error: (err) => {
        console.error('Failed to create board', err);
        this.notificationService.error('Failed to create board');
      }
    });
  }

  toggleMembers(boardId: number) {
    if (this.managedBoardId === boardId) {
      this.managedBoardId = null;
    } else {
      this.managedBoardId = boardId;
      this.loadMembers(boardId);
    }
  }

  loadMembers(boardId: number) {
    this.boardService.getMembers(boardId).subscribe({
      next: (members) => {
        this.boardMembers = members;
      },
      error: (err) => this.notificationService.error('Failed to load members')
    });
  }

  onSearchInput(event: Event) {
    const query = (event.target as HTMLInputElement).value;
    if (query.length > 0) {
      this.searchSubject.next(query);
    } else {
      this.searchResults = [];
    }
  }

  onAddMember(boardId: number, userId: string) {
    this.boardService.addMember(boardId, { userId, role: 'MEMBER' }).subscribe({
      next: () => {
        this.loadMembers(boardId);
        this.searchQuery = '';
        this.searchResults = [];
        this.notificationService.success('Member added');
      },
      error: (err) => this.notificationService.error('Failed to add member')
    });
  }

  onRemoveMember(memberId: number) {
    this.boardService.removeMember(memberId).subscribe({
      next: () => {
        this.boardMembers = this.boardMembers.filter(m => m.boardMemberId !== memberId);
        this.notificationService.success('Member removed');
      },
      error: (err) => this.notificationService.error('Failed to remove member')
    });
  }

  onUpdateMemberRole(memberId: number, role: string) {
    this.boardService.updateMemberRole(memberId, role).subscribe({
      next: () => {
        this.notificationService.success('Member role updated');
      },
      error: (err) => this.notificationService.error('Failed to update member role')
    });
  }

  onCloseBoard(boardId: number) {
    if (confirm('Are you sure you want to close this board?')) {
      this.boardService.closeBoard(boardId).subscribe({
        next: () => {
          this.loadBoards();
          this.notificationService.success('Board closed');
        },
        error: (err) => this.notificationService.error('Failed to close board')
      });
    }
  }

  onDeleteBoard(boardId: number) {
    if (confirm('Are you sure you want to delete this board?')) {
      this.boardService.deleteBoard(boardId).subscribe({
        next: () => {
          this.boards.set(this.boards().filter(b => b.boardId !== boardId));
          this.notificationService.success('Board deleted');
        },
        error: (err) => this.notificationService.error('Failed to delete board')
      });
    }
  }

  private resetForm() {
    this.newBoard = {
      name: '',
      description: '',
      background: '#FFFFFF',
      visibility: 'PRIVATE'
    };
    this.showCreateForm = false;
  }
}
