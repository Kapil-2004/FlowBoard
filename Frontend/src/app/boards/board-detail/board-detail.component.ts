import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BoardService } from '../../services/board.service';
import { ListService } from '../../services/list.service';
import { NotificationService } from '../../services/notification.service';
import { BoardResponseDto, BoardUpdateDto } from '../../models/board.models';
import { ListDto, CreateListDto, UpdateListDto } from '../../models/list.models';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './board-detail.component.html',
  styleUrls: ['./board-detail.component.css']
})
export class BoardDetailComponent implements OnInit {
  private boardService = inject(BoardService);
  private listService = inject(ListService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);

  board = signal<BoardResponseDto | null>(null);
  lists = signal<ListDto[]>([]);
  archivedLists = signal<ListDto[]>([]);
  allBoards = signal<BoardResponseDto[]>([]);

  showEditForm = false;
  showAddList = false;
  sidebarCollapsed = true;
  showArchivedPanel = false;
  showMoveModal = false;

  editBoard: BoardUpdateDto = { name: '', description: '', background: '', visibility: '' };
  newList: CreateListDto = { boardId: 0, name: '', color: '#6366F1' };

  // Inline editing
  editingListId: number | null = null;
  editingListName = '';
  editingColorId: number | null = null;
  editingColorValue = '';

  // Drag & drop
  draggedListId: number | null = null;
  dragOverListId: number | null = null;

  // Move list
  moveListId: number | null = null;
  moveTargetBoardId: number | null = null;

  ngOnInit() {
    this.loadBoard();
  }

  loadBoard() {
    const idStr = this.route.snapshot.paramMap.get('id');
    if (!idStr) return;
    const boardId = parseInt(idStr);
    this.boardService.getBoardById(boardId).subscribe({
      next: (board) => {
        this.board.set(board);
        this.editBoard = { ...board };
        this.loadLists(boardId);
      },
      error: () => this.notificationService.error('Failed to load board details')
    });
  }

  loadLists(boardId: number) {
    this.listService.getListsByBoard(boardId).subscribe({
      next: (lists) => this.lists.set(lists),
      error: () => this.notificationService.error('Failed to load board columns. Check service connection.')
    });
  }

  loadArchivedLists() {
    const boardId = this.board()?.boardId;
    if (!boardId) return;
    this.listService.getArchivedListsByBoard(boardId).subscribe({
      next: (lists) => this.archivedLists.set(lists),
      error: () => this.notificationService.error('Failed to load archived lists')
    });
  }

  // ── Create ──────────────────────────────────────────────────────────────────
  onCreateList() {
    if (!this.newList.name.trim()) return;
    this.newList.boardId = this.board()!.boardId;
    this.listService.createList(this.newList).subscribe({
      next: (list) => {
        this.lists.set([...this.lists(), list]);
        this.newList.name = '';
        this.showAddList = false;
        this.notificationService.success('List added');
      },
      error: () => this.notificationService.error('Failed to create list')
    });
  }

  // ── Inline Rename ───────────────────────────────────────────────────────────
  startEditName(list: ListDto) {
    this.editingListId = list.listId;
    this.editingListName = list.name;
  }

  saveEditName(list: ListDto) {
    if (!this.editingListName.trim() || this.editingListName === list.name) {
      this.editingListId = null;
      return;
    }
    const dto: UpdateListDto = { name: this.editingListName };
    this.listService.updateList(list.listId, dto).subscribe({
      next: (updated) => {
        this.lists.set(this.lists().map(l => l.listId === updated.listId ? updated : l));
        this.editingListId = null;
        this.notificationService.success('Column renamed');
      },
      error: () => this.notificationService.error('Failed to rename column')
    });
  }

  cancelEditName() { this.editingListId = null; }

  // ── Color Change ────────────────────────────────────────────────────────────
  startEditColor(list: ListDto) {
    this.editingColorId = list.listId;
    this.editingColorValue = list.color;
  }

  saveEditColor(list: ListDto) {
    if (this.editingColorValue === list.color) {
      this.editingColorId = null;
      return;
    }
    const dto: UpdateListDto = { color: this.editingColorValue };
    this.listService.updateList(list.listId, dto).subscribe({
      next: (updated) => {
        this.lists.set(this.lists().map(l => l.listId === updated.listId ? updated : l));
        this.editingColorId = null;
        this.notificationService.success('Color updated');
      },
      error: () => this.notificationService.error('Failed to update color')
    });
  }

  // ── Drag & Drop Reorder ─────────────────────────────────────────────────────
  onDragStart(event: DragEvent, listId: number) {
    this.draggedListId = listId;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/plain', listId.toString());
    }
  }

  onDragOver(event: DragEvent, listId: number) {
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
    this.dragOverListId = listId;
  }

  onDragLeave() { this.dragOverListId = null; }

  onDrop(event: DragEvent, targetListId: number) {
    event.preventDefault();
    this.dragOverListId = null;
    if (this.draggedListId === null || this.draggedListId === targetListId) {
      this.draggedListId = null;
      return;
    }
    const currentLists = [...this.lists()];
    const dragIdx = currentLists.findIndex(l => l.listId === this.draggedListId);
    const dropIdx = currentLists.findIndex(l => l.listId === targetListId);
    if (dragIdx === -1 || dropIdx === -1) return;

    const [moved] = currentLists.splice(dragIdx, 1);
    currentLists.splice(dropIdx, 0, moved);
    this.lists.set(currentLists);

    const orderedIds = currentLists.map(l => l.listId);
    const boardId = this.board()!.boardId;
    this.listService.reorderLists(boardId, { listIds: orderedIds }).subscribe({
      next: () => {
        this.notificationService.success('Columns reordered');
        this.loadLists(boardId);
      },
      error: () => {
        this.notificationService.error('Failed to reorder columns');
        this.loadLists(boardId);
      }
    });
    this.draggedListId = null;
  }

  onDragEnd() { this.draggedListId = null; this.dragOverListId = null; }

  // ── Archive / Unarchive ─────────────────────────────────────────────────────
  onArchiveList(listId: number) {
    this.listService.archiveList(listId).subscribe({
      next: () => {
        this.lists.set(this.lists().filter(l => l.listId !== listId));
        this.notificationService.success('Column archived');
      },
      error: () => this.notificationService.error('Failed to archive column')
    });
  }

  toggleArchivedPanel() {
    this.showArchivedPanel = !this.showArchivedPanel;
    if (this.showArchivedPanel) this.loadArchivedLists();
  }

  onUnarchiveList(listId: number) {
    this.listService.unarchiveList(listId).subscribe({
      next: () => {
        this.archivedLists.set(this.archivedLists().filter(l => l.listId !== listId));
        this.loadLists(this.board()!.boardId);
        this.notificationService.success('Column restored');
      },
      error: () => this.notificationService.error('Failed to restore column')
    });
  }

  // ── Delete ──────────────────────────────────────────────────────────────────
  onDeleteList(listId: number) {
    if (confirm('Permanently delete this column? This cannot be undone.')) {
      this.listService.deleteList(listId).subscribe({
        next: () => {
          this.lists.set(this.lists().filter(l => l.listId !== listId));
          this.notificationService.success('Column removed');
        },
        error: () => this.notificationService.error('Failed to delete column')
      });
    }
  }

  onDeleteArchivedList(listId: number) {
    if (confirm('Permanently delete this archived column?')) {
      this.listService.deleteList(listId).subscribe({
        next: () => {
          this.archivedLists.set(this.archivedLists().filter(l => l.listId !== listId));
          this.notificationService.success('Archived column deleted');
        },
        error: () => this.notificationService.error('Failed to delete archived column')
      });
    }
  }

  // ── Move List ───────────────────────────────────────────────────────────────
  openMoveModal(listId: number) {
    this.moveListId = listId;
    this.moveTargetBoardId = null;
    this.showMoveModal = true;
    this.boardService.getBoardsByMember().subscribe({
      next: (boards) => {
        this.allBoards.set(boards.filter(b => b.boardId !== this.board()!.boardId && !b.isClosed));
      }
    });
  }

  closeMoveModal() { this.showMoveModal = false; this.moveListId = null; }

  onMoveList() {
    if (!this.moveListId || !this.moveTargetBoardId) return;
    this.listService.moveList(this.moveListId, { targetBoardId: this.moveTargetBoardId }).subscribe({
      next: () => {
        this.lists.set(this.lists().filter(l => l.listId !== this.moveListId));
        this.closeMoveModal();
        this.notificationService.success('Column moved to another board');
      },
      error: () => this.notificationService.error('Failed to move column')
    });
  }

  // ── Board Operations ────────────────────────────────────────────────────────
  goBack() { window.history.back(); }
  toggleEditForm() { this.showEditForm = !this.showEditForm; }

  onUpdate() {
    const id = this.board()?.boardId;
    if (!id) return;
    this.boardService.updateBoard(id, this.editBoard).subscribe({
      next: (updated) => {
        this.board.set(updated);
        this.showEditForm = false;
        this.notificationService.success('Settings synchronized');
      }
    });
  }

  onDelete() {
    const id = this.board()?.boardId;
    if (!id) return;
    if (confirm('Delete this board permanently? This cannot be undone.')) {
      this.boardService.deleteBoard(id).subscribe({
        next: () => {
          this.notificationService.success('Board destroyed');
          this.goBack();
        }
      });
    }
  }
}
