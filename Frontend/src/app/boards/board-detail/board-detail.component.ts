import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BoardService } from '../../services/board.service';
import { ListService } from '../../services/list.service';
import { NotificationService } from '../../services/notification.service';
import { BoardResponseDto, BoardUpdateDto } from '../../models/board.models';
import { ListDto, CreateListDto, UpdateListDto } from '../../models/list.models';
import { CardService } from '../../services/card.service';
import { CardDto, CreateCardDto, UpdateCardDto, MoveCardDto } from '../../models/card.models';
import { CommentService } from '../../services/comment.service';
import { Comment, Attachment, CreateCommentDto } from '../../models/comment.models';
import { AuthService } from '../../services/auth.service';
import { LabelService } from '../../services/label.service';
import { Label, Checklist, ChecklistItem } from '../../models/label.models';
import { NotificationBellComponent } from '../../notifications/notification-bell/notification-bell.component';
import { BoardMembersComponent } from '../board-members/board-members.component';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent, BoardMembersComponent],
  templateUrl: './board-detail.component.html',
  styleUrls: ['./board-detail.component.css']
})
export class BoardDetailComponent implements OnInit {
  private boardService = inject(BoardService);
  private listService = inject(ListService);
  private notificationService = inject(NotificationService);
  private cardService = inject(CardService);
  private commentService = inject(CommentService);
  private labelService = inject(LabelService);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);

  board = signal<BoardResponseDto | null>(null);
  lists = signal<ListDto[]>([]);
  archivedLists = signal<ListDto[]>([]);
  allBoards = signal<BoardResponseDto[]>([]);
  cardsByList = signal<Record<number, CardDto[]>>({});

  showEditForm = false;
  showAddList = false;
  sidebarCollapsed = true;
  showArchivedPanel = false;
  showMoveModal = false;
  showMembersPanel = false;

  editBoard: BoardUpdateDto = { name: '', description: '', background: '', visibility: '' };
  newList: CreateListDto = { boardId: 0, name: '', color: '#6366F1' };

  showAddCard: Record<number, boolean> = {};
  newCardTitle: Record<number, string> = {};

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

  // Card Modal
  selectedCard: CardDto | null = null;
  cardComments: Comment[] = [];
  cardAttachments: Attachment[] = [];
  newCommentContent = '';
  newAttachmentUrl = '';
  newAttachmentName = '';

  // Label & Checklist
  boardLabels = signal<Label[]>([]);
  cardLabels: Label[] = [];                          // labels for the currently-open card
  cardLabelsMap: Record<number, Label[]> = {};        // cardId → labels, for board-view badges
  cardChecklists: Checklist[] = [];
  checklistProgress = 0;
  showLabelManager = false;
  newLabel = { name: '', color: '#6366F1' };
  newChecklistTitle = '';
  newItemText: Record<number, string> = {};

  // Card Drag & Drop
  draggedCard: CardDto | null = null;

  get currentUserId(): number {
    const user = this.authService.currentUser();
    if (user && user.id) {
      const parsed = parseInt(user.id, 10);
      if (!isNaN(parsed)) return parsed;
    }
    return 1;
  }

  closeMembersPanel() { this.showMembersPanel = false; }

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
        this.loadBoardLabels(boardId);
      },
      error: () => this.notificationService.error('Failed to load board details')
    });
  }

  loadLists(boardId: number) {
    this.listService.getListsByBoard(boardId).subscribe({
      next: (lists) => {
        this.lists.set(lists);
        this.loadCards(boardId);
      },
      error: () => this.notificationService.error('Failed to load board columns. Check service connection.')
    });
  }

  loadCards(boardId: number) {
    this.cardService.getCardsByBoard(boardId).subscribe({
      next: (cards) => {
        const grouped: Record<number, CardDto[]> = {};
        for (const list of this.lists()) {
          grouped[list.listId] = [];
        }
        for (const card of cards) {
          if (!grouped[card.listId]) grouped[card.listId] = [];
          grouped[card.listId].push(card);
        }
        this.cardsByList.set(grouped);
        // Pre-load labels for every card so board-view badges work
        this.loadAllCardLabels(cards.map(c => c.cardId));
      }
    });
  }

  loadAllCardLabels(cardIds: number[]) {
    cardIds.forEach(cardId => {
      this.labelService.getLabelsForCard(cardId).subscribe({
        next: labels => {
          this.cardLabelsMap = { ...this.cardLabelsMap, [cardId]: labels };
        }
      });
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

  // ── Create Card ─────────────────────────────────────────────────────────────
  openAddCard(listId: number) {
    this.showAddCard[listId] = true;
    this.newCardTitle[listId] = '';
  }

  closeAddCard(listId: number) {
    this.showAddCard[listId] = false;
  }

  onCreateCard(listId: number) {
    const title = this.newCardTitle[listId];
    if (!title?.trim()) return;
    const dto: CreateCardDto = {
      listId,
      boardId: this.board()!.boardId,
      title: title,
      priority: 'MEDIUM',
      status: 'TO_DO'
    };
    this.cardService.createCard(dto).subscribe({
      next: (card) => {
        const current = { ...this.cardsByList() };
        if (!current[listId]) current[listId] = [];
        current[listId] = [...current[listId], card];
        this.cardsByList.set(current);
        this.closeAddCard(listId);
      },
      error: () => this.notificationService.error('Failed to create card')
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

  // ── Card Drag & Drop ────────────────────────────────────────────────────────
  onCardDragStart(event: DragEvent, card: CardDto) {
    event.stopPropagation(); // Prevent list drag
    this.draggedCard = card;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/plain', card.cardId.toString());
    }
  }

  onCardDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
  }

  onCardDrop(event: DragEvent, targetListId: number, targetCard?: CardDto) {
    event.preventDefault();
    event.stopPropagation();
    
    if (!this.draggedCard || this.draggedCard.cardId === targetCard?.cardId) {
      this.draggedCard = null;
      return;
    }

    const cardId = this.draggedCard.cardId;
    const sourceListId = this.draggedCard.listId;
    
    const currentListCards = this.cardsByList()[sourceListId] || [];
    const targetListCards = this.cardsByList()[targetListId] || [];

    // Optimistic UI update
    const sourceIdx = currentListCards.findIndex(c => c.cardId === cardId);
    if (sourceIdx !== -1) {
      currentListCards.splice(sourceIdx, 1);
    }
    
    let targetIdx = targetListCards.length;
    if (targetCard) {
       targetIdx = targetListCards.findIndex(c => c.cardId === targetCard.cardId);
    }
    
    const movedCard = { ...this.draggedCard, listId: targetListId };
    
    if (sourceListId === targetListId) {
       currentListCards.splice(targetIdx, 0, movedCard);
       this.cardsByList.set({ ...this.cardsByList(), [sourceListId]: [...currentListCards] });
    } else {
       targetListCards.splice(targetIdx, 0, movedCard);
       this.cardsByList.set({
          ...this.cardsByList(),
          [sourceListId]: [...currentListCards],
          [targetListId]: [...targetListCards]
       });
    }

    // Call API
    const targetPosition = targetIdx; 
    const dto: MoveCardDto = { targetListId, targetPosition };
    
    this.cardService.moveCard(cardId, dto).subscribe({
      error: () => {
        this.notificationService.error('Failed to move card');
        this.loadCards(this.board()!.boardId); // rollback
      }
    });

    this.draggedCard = null;
  }

  onCardDragEnd() { this.draggedCard = null; }

  // ── Card Modal & Comments ───────────────────────────────────────────────────
  getListName(listId: number): string {
    return this.lists().find(l => l.listId === listId)?.name || 'Unknown List';
  }

  openCard(card: CardDto) {
    this.selectedCard = { ...card };
    this.loadCardDetails();
  }

  closeCardModal() {
    this.selectedCard = null;
    this.cardComments = [];
    this.cardAttachments = [];
    this.cardLabels = [];
    this.cardChecklists = [];
    this.checklistProgress = 0;
  }

  loadCardDetails() {
    if (!this.selectedCard) return;
    const cardId = this.selectedCard.cardId;
    
    this.commentService.getByCard(cardId).subscribe({
      next: comments => this.cardComments = comments
    });
    
    this.commentService.getAttachmentsByCard(cardId).subscribe({
      next: atts => this.cardAttachments = atts
    });

    this.labelService.getLabelsForCard(cardId).subscribe({
      next: labels => {
        this.cardLabels = labels;
        // also update map so board badges stay in sync
        this.cardLabelsMap = { ...this.cardLabelsMap, [cardId]: labels };
      }
    });

    this.labelService.getChecklistsByCard(cardId).subscribe({
      next: checklists => {
        this.cardChecklists = checklists;
        this.updateChecklistProgress();
      }
    });
  }

  updateCardDetails() {
    if (!this.selectedCard) return;
    const dto: UpdateCardDto = {
       title: this.selectedCard.title,
       description: this.selectedCard.description,
       priority: this.selectedCard.priority,
       status: this.selectedCard.status
    };
    this.cardService.updateCard(this.selectedCard.cardId, dto).subscribe({
       next: (updated) => {
         // Update card in local board state
         const listId = updated.listId;
         const currentCards = this.cardsByList()[listId] || [];
         const idx = currentCards.findIndex(c => c.cardId === updated.cardId);
         if (idx !== -1) {
            const newCards = [...currentCards];
            newCards[idx] = updated;
            this.cardsByList.set({ ...this.cardsByList(), [listId]: newCards });
         }
       }
    });
  }

  onDeleteCard() {
    if (!this.selectedCard) return;
    if (confirm('Delete this card permanently?')) {
      const cardId = this.selectedCard.cardId;
      const listId = this.selectedCard.listId;
      this.cardService.deleteCard(cardId).subscribe({
        next: () => {
           const listCards = this.cardsByList()[listId] || [];
           this.cardsByList.set({
              ...this.cardsByList(),
              [listId]: listCards.filter(c => c.cardId !== cardId)
           });
           this.closeCardModal();
           this.notificationService.success('Card deleted');
        }
      });
    }
  }

  // ── Comments & Attachments ──────────────────────────────────────────────────
  getAuthorInitial(authorId: number): string {
    return 'U'; // Mocked for now, in real app fetch user profile
  }

  onAddComment() {
    if (!this.selectedCard || !this.newCommentContent.trim()) return;
    const dto: CreateCommentDto = {
       cardId: this.selectedCard.cardId,
       authorId: this.currentUserId,
       content: this.newCommentContent.trim()
    };
    this.commentService.addComment(dto).subscribe({
       next: (comment) => {
          this.cardComments = [comment, ...this.cardComments];
          this.newCommentContent = '';
       }
    });
  }

  onDeleteComment(commentId: number) {
    if (confirm('Delete this comment?')) {
       this.commentService.deleteComment(commentId).subscribe({
          next: () => {
             this.cardComments = this.cardComments.filter(c => c.commentId !== commentId);
          }
       });
    }
  }

  // ── Label & Checklist Management ────────────────────────────────────────────
  loadBoardLabels(boardId: number) {
    this.labelService.getLabelsByBoard(boardId).subscribe({
      next: labels => this.boardLabels.set(labels)
    });
  }

  onCreateLabel() {
    if (!this.newLabel.name.trim()) return;
    this.labelService.createLabel({
      boardId: this.board()!.boardId,
      name: this.newLabel.name,
      color: this.newLabel.color
    }).subscribe({
      next: label => {
        this.boardLabels.set([...this.boardLabels(), label]);
        this.newLabel.name = '';
      }
    });
  }

  deleteLabel(labelId: number) {
    if (confirm('Delete this label permanently from the board?')) {
      this.labelService.deleteLabel(labelId).subscribe({
        next: () => {
          this.boardLabels.set(this.boardLabels().filter(l => l.labelId !== labelId));
          this.cardLabels = this.cardLabels.filter(l => l.labelId !== labelId);
        }
      });
    }
  }

  onToggleLabelOnCard(label: Label) {
    if (!this.selectedCard) return;
    const cardId = this.selectedCard.cardId;
    const isAttached = this.cardLabels.some(l => l.labelId === label.labelId);
    if (isAttached) {
      this.labelService.removeLabelFromCard(cardId, label.labelId).subscribe({
        next: () => {
          this.cardLabels = this.cardLabels.filter(l => l.labelId !== label.labelId);
          this.cardLabelsMap = { ...this.cardLabelsMap, [cardId]: this.cardLabels };
        }
      });
    } else {
      this.labelService.addLabelToCard(cardId, label.labelId).subscribe({
        next: () => {
          this.cardLabels = [...this.cardLabels, label];
          this.cardLabelsMap = { ...this.cardLabelsMap, [cardId]: this.cardLabels };
        }
      });
    }
  }

  onCreateChecklist() {
    if (!this.selectedCard || !this.newChecklistTitle.trim()) return;
    this.labelService.createChecklist({
      cardId: this.selectedCard.cardId,
      title: this.newChecklistTitle,
      position: this.cardChecklists.length
    }).subscribe({
      next: checklist => {
        this.cardChecklists = [...this.cardChecklists, checklist];
        this.newChecklistTitle = '';
        this.updateChecklistProgress();
      }
    });
  }

  onAddChecklistItem(checklistId: number) {
    const text = this.newItemText[checklistId];
    if (!text?.trim()) return;
    this.labelService.addItem(checklistId, { text, isCompleted: false }).subscribe({
      next: updatedChecklist => {
        const idx = this.cardChecklists.findIndex(c => c.checklistId === checklistId);
        if (idx !== -1) {
          const newChecklists = [...this.cardChecklists];
          newChecklists[idx] = updatedChecklist;
          this.cardChecklists = newChecklists;
        }
        this.newItemText[checklistId] = '';
        this.updateChecklistProgress();
      }
    });
  }

  onToggleChecklistItem(item: ChecklistItem) {
    this.labelService.toggleItem(item.itemId).subscribe({
      next: updatedItem => {
        item.isCompleted = updatedItem.isCompleted;
        this.updateChecklistProgress();
      }
    });
  }

  onDeleteChecklist(checklistId: number) {
    if (confirm('Delete this checklist?')) {
      this.labelService.deleteChecklist(checklistId).subscribe({
        next: () => {
          this.cardChecklists = this.cardChecklists.filter(c => c.checklistId !== checklistId);
          this.updateChecklistProgress();
        }
      });
    }
  }

  updateChecklistProgress() {
    const allItems = this.cardChecklists.flatMap(c => c.items);
    if (allItems.length === 0) {
      this.checklistProgress = 0;
      return;
    }
    const completed = allItems.filter(i => i.isCompleted).length;
    this.checklistProgress = Math.round((completed / allItems.length) * 100);
  }

  /** Used inside the card modal (selectedCard context) */
  isLabelOnCard(labelId: number): boolean {
    return this.cardLabels.some(l => l.labelId === labelId);
  }

  /** Used on the board Kanban card tiles */
  isLabelOnCardById(cardId: number, labelId: number): boolean {
    return (this.cardLabelsMap[cardId] || []).some(l => l.labelId === labelId);
  }

  onAddAttachment() {
    if (!this.selectedCard || !this.newAttachmentUrl.trim()) return;
    const att: Partial<Attachment> = {
       cardId: this.selectedCard.cardId,
       uploaderId: this.currentUserId,
       fileName: this.newAttachmentName || 'Link Attachment',
       fileUrl: this.newAttachmentUrl,
       fileType: 'link',
       sizeKb: 0
    };
    this.commentService.addAttachment(att).subscribe({
       next: (newAtt) => {
          this.cardAttachments = [...this.cardAttachments, newAtt];
          this.newAttachmentUrl = '';
          this.newAttachmentName = '';
       }
    });
  }

  onDeleteAttachment(attId: number) {
    if (confirm('Delete this attachment?')) {
       this.commentService.deleteAttachment(attId).subscribe({
          next: () => {
             this.cardAttachments = this.cardAttachments.filter(a => a.attachmentId !== attId);
          }
       });
    }
  }

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
