import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ListDto,
  CreateListDto,
  UpdateListDto,
  ReorderListsDto,
  MoveListDto
} from '../models/list.models';

@Injectable({
  providedIn: 'root'
})
export class ListService {
  /** FlowBoard-ListService runs on port 5004 */
  private isProd = !window.location.hostname.includes('localhost');
  private apiUrl = this.isProd ? 'https://list-service.onrender.com/api/lists' : '/api/lists';

  constructor(private http: HttpClient) {}

  // ── Create ──────────────────────────────────────────────────────────────────

  createList(dto: CreateListDto): Observable<ListDto> {
    return this.http.post<ListDto>(this.apiUrl, dto);
  }

  // ── Read ────────────────────────────────────────────────────────────────────

  getListById(id: number): Observable<ListDto> {
    return this.http.get<ListDto>(`${this.apiUrl}/${id}`);
  }

  getListsByBoard(boardId: number): Observable<ListDto[]> {
    return this.http.get<ListDto[]>(`${this.apiUrl}/board/${boardId}`);
  }

  getArchivedListsByBoard(boardId: number): Observable<ListDto[]> {
    return this.http.get<ListDto[]>(`${this.apiUrl}/board/${boardId}/archived`);
  }

  // ── Update ──────────────────────────────────────────────────────────────────

  updateList(id: number, dto: UpdateListDto): Observable<ListDto> {
    return this.http.put<ListDto>(`${this.apiUrl}/${id}`, dto);
  }

  reorderLists(boardId: number, dto: ReorderListsDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/board/${boardId}/reorder`, dto);
  }

  moveList(id: number, dto: MoveListDto): Observable<ListDto> {
    return this.http.put<ListDto>(`${this.apiUrl}/${id}/move`, dto);
  }

  // ── Archive / Unarchive ─────────────────────────────────────────────────────

  archiveList(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/archive`, {});
  }

  unarchiveList(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/unarchive`, {});
  }

  // ── Delete ──────────────────────────────────────────────────────────────────

  deleteList(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
