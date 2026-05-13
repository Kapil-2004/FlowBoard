export interface CardDto {
  cardId: number;
  listId: number;
  boardId: number;
  title: string;
  description?: string;
  position: number;
  priority: string; // LOW, MEDIUM, HIGH, CRITICAL
  status: string;   // TO_DO, IN_PROGRESS, IN_REVIEW, DONE
  dueDate?: string; // yyyy-MM-dd
  startDate?: string;
  assigneeId?: number;
  createdById: number;
  isArchived: boolean;
  coverColor?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCardDto {
  listId: number;
  boardId: number;
  title: string;
  description?: string;
  priority?: string;
  status?: string;
  dueDate?: string;
  startDate?: string;
  assigneeId?: number;
  coverColor?: string;
}

export interface UpdateCardDto {
  title?: string;
  description?: string;
  priority?: string;
  status?: string;
  dueDate?: string;
  startDate?: string;
  coverColor?: string;
}

export interface MoveCardDto {
  targetListId: number;
  targetPosition: number;
}

export interface ReorderCardsDto {
  cardIds: number[];
}

export interface AssignCardDto {
  assigneeId?: number;
}
