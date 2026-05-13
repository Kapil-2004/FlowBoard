export interface Comment {
  commentId: number;
  cardId: number;
  authorId: number;
  content: string;
  parentCommentId?: number;
  createdAt: Date;
  updatedAt: Date;
  replies?: Comment[];
}

export interface CreateCommentDto {
  cardId: number;
  authorId: number;
  content: string;
  parentCommentId?: number;
}

export interface UpdateCommentDto {
  content: string;
}

export interface Attachment {
  attachmentId: number;
  cardId: number;
  uploaderId: number;
  fileName: string;
  fileUrl: string;
  fileType: string;
  sizeKb: number;
  uploadedAt: Date;
}
