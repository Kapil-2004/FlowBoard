// UC4 – List/Column-Service frontend models
// Matches the DTOs from FlowBoard-ListService

export interface ListDto {
  listId: number;
  boardId: number;
  name: string;
  position: number;
  color: string;
  isArchived: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateListDto {
  boardId: number;
  name: string;
  color?: string;
}

export interface UpdateListDto {
  name?: string;
  color?: string;
}

export interface ReorderListsDto {
  listIds: number[];
}

export interface MoveListDto {
  targetBoardId: number;
}
