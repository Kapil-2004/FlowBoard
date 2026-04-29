import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BoardService } from '../../services/board.service';
import { NotificationService } from '../../services/notification.service';
import { BoardResponseDto, BoardUpdateDto } from '../../models/board.models';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="board-detail-wrapper" *ngIf="board()">
      <div class="board-top-bar">
        <div class="bar-left">
          <button class="back-btn" (click)="goBack()">
            <span class="arrow">←</span>
            Back
          </button>
          <div class="board-meta">
            <div class="title-row">
              <h1 class="board-title">{{ board()!.name }}</h1>
              <div class="board-tags">
                <span class="tag" [class.private]="board()!.visibility === 'PRIVATE'">
                  {{ board()!.visibility | lowercase }}
                </span>
                <span *ngIf="board()!.isClosed" class="tag closed">closed</span>
              </div>
            </div>
            <p class="workspace-name">Sprint Workspace</p>
          </div>
        </div>
        <div class="bar-right">
          <button class="btn-icon-text" (click)="toggleEditForm()">
            <span>✏️</span> Edit Board
          </button>
          <button *ngIf="!board()!.isClosed" class="btn-icon-text" (click)="onClose()">
            <span>🔒</span> Close
          </button>
          <button class="btn-icon-text danger" (click)="onDelete()">
            <span>🗑️</span> Delete
          </button>
        </div>
      </div>

      <main class="board-main">
        <aside class="board-sidebar">
          <div class="sidebar-section">
            <h3 class="section-title">Overview</h3>
            <div class="info-card">
              <div class="info-group">
                <label>Description</label>
                <p class="desc-text">{{ board()!.description || 'No description provided.' }}</p>
              </div>
              <div class="info-grid">
                <div class="info-group">
                  <label>Created</label>
                  <p>{{ board()!.createdAt | date:'MMM d, y' }}</p>
                </div>
                <div class="info-group">
                  <label>Status</label>
                  <div class="status-indicator">
                    <span class="dot" [class.active]="!board()!.isClosed"></span>
                    {{ board()!.isClosed ? 'Archived' : 'Active' }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="sidebar-section">
            <h3 class="section-title">Members</h3>
            <div class="members-stack">
              <div class="member-avatar" *ngFor="let member of board()!.members" [title]="member.userId">
                {{ member.role[0] }}
              </div>
              <button class="add-member-btn">+</button>
            </div>
          </div>
        </aside>

        <section class="board-content-area">
          <div *ngIf="showEditForm" class="edit-overlay" (click)="toggleEditForm()">
            <div class="edit-modal" (click)="$event.stopPropagation()">
              <div class="modal-header">
                <div class="modal-title-group">
                  <h3>Board Settings</h3>
                  <p>Customize your board's appearance and visibility</p>
                </div>
                <button class="close-modal" (click)="toggleEditForm()">×</button>
              </div>
              <form (ngSubmit)="onUpdate()" class="premium-form">
                <div class="form-group">
                  <label>Name</label>
                  <input type="text" [(ngModel)]="editBoard.name" name="name" placeholder="Board name" />
                </div>
                <div class="form-group">
                  <label>Description</label>
                  <textarea [(ngModel)]="editBoard.description" name="description" rows="3" placeholder="What's this board about?"></textarea>
                </div>
                <div class="form-row">
                  <div class="form-group half">
                    <label>Accent Color</label>
                    <div class="custom-color-picker">
                      <input type="color" [(ngModel)]="editBoard.background" name="background" />
                      <span class="hex-val">{{ editBoard.background }}</span>
                    </div>
                  </div>
                  <div class="form-group half">
                    <label>Visibility</label>
                    <select [(ngModel)]="editBoard.visibility" name="visibility">
                      <option value="PRIVATE">Private</option>
                      <option value="PUBLIC">Public</option>
                    </select>
                  </div>
                </div>
                <div class="modal-footer">
                  <button type="button" class="btn-secondary" (click)="toggleEditForm()">Cancel</button>
                  <button type="submit" class="btn-primary">Save Changes</button>
                </div>
              </form>
            </div>
          </div>

          <div class="kanban-wrapper">
            <div class="kanban-header">
              <h2>Task Lists</h2>
              <button class="btn-add-list">+ Add List</button>
            </div>
            
            <div class="empty-kanban-state">
              <div class="empty-art">≋</div>
              <h3>Ready to organize?</h3>
              <p>Create task lists to start managing your workflow in this board.</p>
              <button class="btn-primary">+ Create First List</button>
            </div>
          </div>
        </section>
      </main>
    </div>

    <div *ngIf="!board()" class="loader-container">
      <div class="loader-spinner"></div>
      <p>Synchronizing data...</p>
    </div>
  `,
  styles: [`
    .board-detail-wrapper {
      display: flex;
      flex-direction: column;
      height: 100vh;
      background: var(--color-bg);
      animation: fadeIn var(--transition-normal);
    }

    /* Top Bar */
    .board-top-bar {
      height: 90px;
      padding: 0 40px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: var(--color-surface);
      border-bottom: 1px solid var(--color-border);
      z-index: 10;
    }

    .bar-left {
      display: flex;
      align-items: center;
      gap: 40px;
    }

    .back-btn {
      color: var(--color-text-secondary);
      font-size: 14px;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px 16px;
      border-radius: var(--radius-md);
      transition: all var(--transition-fast);
    }

    .back-btn:hover { background: var(--color-surface-hover); color: #fff; }
    .arrow { font-size: 20px; }

    .board-meta { display: flex; flex-direction: column; gap: 4px; }
    .title-row { display: flex; align-items: center; gap: 16px; }
    .board-title {
      font-family: var(--font-heading);
      font-size: 28px;
      font-weight: 800;
      letter-spacing: -0.03em;
      margin: 0;
    }

    .workspace-name { font-size: 13px; color: var(--color-text-placeholder); font-weight: 500; }

    .board-tags { display: flex; gap: 8px; }
    .tag {
      font-size: 10px;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      padding: 4px 10px;
      border-radius: 4px;
      background: rgba(255, 255, 255, 0.05);
      color: var(--color-text-secondary);
      border: 1px solid var(--color-border);
    }

    .tag.closed { background: var(--color-error-light); color: var(--color-error); border-color: rgba(239, 68, 68, 0.2); }

    .bar-right { display: flex; gap: 12px; }

    .btn-icon-text {
      padding: 10px 20px;
      border-radius: var(--radius-md);
      font-size: 14px;
      font-weight: 700;
      background: var(--color-surface-hover);
      color: var(--color-text-primary);
      display: flex;
      align-items: center;
      gap: 10px;
      border: 1px solid var(--color-border);
    }

    .btn-icon-text:hover { border-color: rgba(255,255,255,0.4); background: var(--color-border); }
    .btn-icon-text.danger:hover { color: var(--color-error); border-color: var(--color-error); background: var(--color-error-light); }

    /* Layout */
    .board-main {
      flex: 1;
      display: flex;
      overflow: hidden;
    }

    .board-sidebar {
      width: 360px;
      padding: 40px;
      background: var(--color-surface);
      border-right: 1px solid var(--color-border);
      overflow-y: auto;
      display: flex;
      flex-direction: column;
      gap: 48px;
    }

    .section-title {
      font-family: var(--font-heading);
      font-size: 13px;
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.1em;
      color: var(--color-text-placeholder);
      margin-bottom: 24px;
    }

    .info-card {
      background: var(--color-bg);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      padding: 24px;
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }

    .desc-text { font-size: 14px; color: var(--color-text-secondary); line-height: 1.7; }

    .status-indicator { display: flex; align-items: center; gap: 8px; font-size: 14px; font-weight: 600; }
    .dot { width: 10px; height: 10px; border-radius: 50%; background: var(--color-text-placeholder); }
    .dot.active { background: #4ade80; box-shadow: 0 0 15px rgba(74, 222, 128, 0.4); }

    .members-stack { display: flex; gap: -8px; }
    .member-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: var(--color-surface-hover);
      border: 2px solid var(--color-surface);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 800;
      font-size: 14px;
      color: var(--color-accent);
      margin-right: -12px;
      transition: transform 0.2s;
    }
    .member-avatar:hover { transform: translateY(-4px); z-index: 2; }

    .add-member-btn {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: var(--color-bg);
      border: 2px dashed var(--color-border);
      color: var(--color-text-placeholder);
      font-size: 20px;
      margin-left: 16px;
    }
    .add-member-btn:hover { border-color: #fff; color: #fff; }

    .board-content-area {
      flex: 1;
      padding: 48px;
      overflow-y: auto;
      background: var(--color-bg);
    }

    /* Kanban Area */
    .kanban-wrapper {
      max-width: 1200px;
      margin: 0 auto;
    }

    .kanban-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 40px;
    }

    .kanban-header h2 { font-family: var(--font-heading); font-size: 24px; font-weight: 700; }

    .btn-add-list {
      background: #fff;
      color: #000;
      padding: 10px 24px;
      border-radius: var(--radius-md);
      font-size: 14px;
      font-weight: 700;
    }

    .empty-kanban-state {
      height: 500px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      background: var(--color-surface);
      border: 2px dashed var(--color-border);
      border-radius: var(--radius-lg);
    }

    .empty-art { font-size: 80px; margin-bottom: 24px; opacity: 0.2; }
    .empty-kanban-state h3 { font-size: 24px; font-weight: 700; margin-bottom: 12px; }
    .empty-kanban-state p { color: var(--color-text-secondary); margin-bottom: 40px; max-width: 380px; }

    /* Modal Styling */
    .edit-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.4);
      backdrop-filter: var(--glass-blur);
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .edit-modal {
      background: var(--glass-bg);
      backdrop-filter: blur(24px);
      width: 100%;
      max-width: 560px;
      padding: 48px;
      border-radius: var(--radius-lg);
      border: 1px solid var(--glass-border);
      box-shadow: var(--shadow-lg);
      animation: modalUp 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .modal-header { display: flex; justify-content: space-between; margin-bottom: 40px; }
    .modal-title-group h3 { font-size: 28px; font-weight: 800; letter-spacing: -0.02em; margin-bottom: 4px; }
    .modal-title-group p { font-size: 14px; color: var(--color-text-secondary); }

    .close-modal { font-size: 32px; color: var(--color-text-placeholder); line-height: 1; }
    .close-modal:hover { color: #fff; }

    .premium-form .form-group { margin-bottom: 24px; }
    .premium-form .form-row { display: flex; gap: 24px; margin-bottom: 24px; }
    .premium-form .form-group.half { flex: 1; }

    .custom-color-picker {
      display: flex;
      align-items: center;
      gap: 16px;
      background: var(--color-bg);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      padding: 8px 16px;
    }

    .custom-color-picker input { width: 32px; height: 32px; padding: 0; border: none; cursor: pointer; background: none; }
    .hex-val { font-family: 'JetBrains Mono', monospace; font-size: 14px; color: var(--color-text-secondary); }

    .modal-footer { display: flex; justify-content: flex-end; gap: 20px; margin-top: 48px; }
    .btn-secondary { color: var(--color-text-primary); font-weight: 700; padding: 12px 24px; }
    .btn-primary { background: #fff; color: #000; padding: 14px 32px; border-radius: var(--radius-md); font-weight: 700; font-size: 15px; box-shadow: 0 4px 12px rgba(255,255,255,0.2); }
    .btn-primary:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(255,255,255,0.3); }

    /* Loader */
    .loader-container {
      height: 100vh;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 24px;
      color: var(--color-text-secondary);
    }

    .loader-spinner {
      width: 50px;
      height: 50px;
      border: 4px solid var(--color-border);
      border-top-color: #fff;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin { to { transform: rotate(360deg); } }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
    @keyframes modalUp { from { opacity: 0; transform: translateY(40px); } to { opacity: 1; transform: translateY(0); } }
  `]


})
export class BoardDetailComponent implements OnInit {
  private boardService = inject(BoardService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);

  board = signal<BoardResponseDto | null>(null);
  showEditForm = false;
  editBoard: BoardUpdateDto = {
    name: '',
    description: '',
    background: '',
    visibility: ''
  };

  ngOnInit() {
    this.loadBoard();
  }

  loadBoard() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.boardService.getBoardById(parseInt(id)).subscribe({
      next: (board) => {
        this.board.set(board);
        this.editBoard = {
          name: board.name,
          description: board.description,
          background: board.background,
          visibility: board.visibility
        };
      },
      error: (err) => {
        this.notificationService.error('Failed to load board');
      }
    });
  }

  goBack() {
    window.history.back();
  }

  toggleEditForm() {
    this.showEditForm = !this.showEditForm;
  }

  onUpdate() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.boardService.updateBoard(parseInt(id), this.editBoard).subscribe({
      next: (updated) => {
        this.board.set(updated);
        this.showEditForm = false;
        this.notificationService.success('Board updated');
      },
      error: (err) => {
        this.notificationService.error('Failed to update board');
      }
    });
  }

  onClose() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    if (confirm('Are you sure you want to close this board?')) {
      this.boardService.closeBoard(parseInt(id)).subscribe({
        next: () => {
          this.loadBoard();
          this.notificationService.success('Board closed');
        },
        error: (err) => {
          this.notificationService.error('Failed to close board');
        }
      });
    }
  }

  onDelete() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    if (confirm('Are you sure you want to delete this board? This action cannot be undone.')) {
      this.boardService.deleteBoard(parseInt(id)).subscribe({
        next: () => {
          this.notificationService.success('Board deleted');
          window.history.back();
        },
        error: (err) => {
          this.notificationService.error('Failed to delete board');
        }
      });
    }
  }
}
