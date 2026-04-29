export interface BoardResponseDto {
  boardId: number;
  workspaceId: number;
  name: string;
  description: string;
  background: string;
  visibility: string;
  createdById: string;
  isClosed: boolean;
  createdAt: Date;
  updatedAt: Date;
  members: BoardMemberDto[];
}

export interface BoardCreateDto {
  workspaceId?: number;
  name: string;
  description: string;
  background: string;
  visibility: string;
}

export interface BoardUpdateDto {
  name?: string;
  description?: string;
  background?: string;
  visibility?: string;
}

export interface BoardMemberDto {
  boardMemberId: number;
  boardId: number;
  userId: string;
  role: string; // OBSERVER, MEMBER, ADMIN
  addedAt: Date;
}

export interface AddBoardMemberDto {
  userId: string;
  role: string;
}
