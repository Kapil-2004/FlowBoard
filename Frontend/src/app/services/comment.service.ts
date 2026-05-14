import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Comment, CreateCommentDto, UpdateCommentDto, Attachment } from '../models/comment.models';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private commentApiUrl = '/api/comments';
  private attachmentApiUrl = '/api/attachments';

  constructor(private http: HttpClient) {}

  // ── Comments ─────────────────────────────────────────────────────────────
  addComment(dto: CreateCommentDto): Observable<Comment> {
    return this.http.post<Comment>(this.commentApiUrl, dto);
  }

  getByCard(cardId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.commentApiUrl}/card/${cardId}`);
  }

  getReplies(commentId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.commentApiUrl}/${commentId}/replies`);
  }

  updateComment(id: number, dto: UpdateCommentDto): Observable<Comment> {
    return this.http.put<Comment>(`${this.commentApiUrl}/${id}`, dto);
  }

  deleteComment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.commentApiUrl}/${id}`);
  }

  getCommentCount(cardId: number): Observable<number> {
    return this.http.get<number>(`${this.commentApiUrl}/card/${cardId}/count`);
  }

  // ── Attachments ──────────────────────────────────────────────────────────
  addAttachment(attachment: Partial<Attachment>): Observable<Attachment> {
    return this.http.post<Attachment>(this.attachmentApiUrl, attachment);
  }

  getAttachmentsByCard(cardId: number): Observable<Attachment[]> {
    return this.http.get<Attachment[]>(`${this.attachmentApiUrl}/card/${cardId}`);
  }

  deleteAttachment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.attachmentApiUrl}/${id}`);
  }
}
