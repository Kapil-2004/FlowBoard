export interface WorkspaceResponseDto {
  workspaceId: number;
  name: string;
  description: string;
  ownerId: string;
  visibility: string;
  logoUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface WorkspaceCreateDto {
  name: string;
  description: string;
  visibility: string;
  logoUrl?: string;
}

export interface WorkspaceMemberDto {
  memberId: number;
  workspaceId: number;
  userId: string;
  role: string;
  joinedAt: string;
}
