import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CardDto,
  CreateCardDto,
  UpdateCardDto,
  MoveCardDto,
  ReorderCardsDto,
  AssignCardDto
} from '../models/card.models';

@Injectable({
  providedIn: 'root'
})
export class CardService {
  private apiUrl = 'http://localhost:5005/api/cards';

  constructor(private http: HttpClient) {}

  createCard(dto: CreateCardDto): Observable<CardDto> {
    return this.http.post<CardDto>(this.apiUrl, dto);
  }

  getCardById(id: number): Observable<CardDto> {
    return this.http.get<CardDto>(`${this.apiUrl}/${id}`);
  }

  getCardsByList(listId: number): Observable<CardDto[]> {
    return this.http.get<CardDto[]>(`${this.apiUrl}/list/${listId}`);
  }

  getCardsByBoard(boardId: number): Observable<CardDto[]> {
    return this.http.get<CardDto[]>(`${this.apiUrl}/board/${boardId}`);
  }

  updateCard(id: number, dto: UpdateCardDto): Observable<CardDto> {
    return this.http.put<CardDto>(`${this.apiUrl}/${id}`, dto);
  }

  moveCard(id: number, dto: MoveCardDto): Observable<CardDto> {
    return this.http.put<CardDto>(`${this.apiUrl}/${id}/move`, dto);
  }

  reorderCards(listId: number, dto: ReorderCardsDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/list/${listId}/reorder`, dto);
  }

  archiveCard(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/archive`, {});
  }

  unarchiveCard(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/unarchive`, {});
  }

  deleteCard(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  setAssignee(id: number, dto: AssignCardDto): Observable<CardDto> {
    return this.http.put<CardDto>(`${this.apiUrl}/${id}/assignee`, dto);
  }
}
