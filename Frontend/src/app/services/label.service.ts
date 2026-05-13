import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Label, Checklist, ChecklistItem, CreateLabelDto, CreateChecklistDto } from '../models/label.models';

@Injectable({
  providedIn: 'root'
})
export class LabelService {
  private http = inject(HttpClient);
  private labelUrl = 'http://localhost:5007/api/label';
  private checklistUrl = 'http://localhost:5007/api/checklist';

  // Labels
  createLabel(dto: CreateLabelDto): Observable<Label> {
    return this.http.post<Label>(this.labelUrl, dto);
  }

  getLabelsByBoard(boardId: number): Observable<Label[]> {
    return this.http.get<Label[]>(`${this.labelUrl}/board/${boardId}`);
  }

  updateLabel(id: number, dto: Partial<Label>): Observable<Label> {
    return this.http.put<Label>(`${this.labelUrl}/${id}`, dto);
  }

  deleteLabel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.labelUrl}/${id}`);
  }

  addLabelToCard(cardId: number, labelId: number): Observable<void> {
    return this.http.post<void>(`${this.labelUrl}/card/${cardId}/label/${labelId}`, {});
  }

  removeLabelFromCard(cardId: number, labelId: number): Observable<void> {
    return this.http.delete<void>(`${this.labelUrl}/card/${cardId}/label/${labelId}`);
  }

  getLabelsForCard(cardId: number): Observable<Label[]> {
    return this.http.get<Label[]>(`${this.labelUrl}/card/${cardId}`);
  }

  // Checklists
  createChecklist(dto: CreateChecklistDto): Observable<Checklist> {
    return this.http.post<Checklist>(this.checklistUrl, dto);
  }

  addItem(checklistId: number, item: Partial<ChecklistItem>): Observable<Checklist> {
    return this.http.post<Checklist>(`${this.checklistUrl}/${checklistId}/item`, item);
  }

  toggleItem(itemId: number): Observable<ChecklistItem> {
    return this.http.put<ChecklistItem>(`${this.checklistUrl}/item/${itemId}/toggle`, {});
  }

  deleteChecklist(id: number): Observable<void> {
    return this.http.delete<void>(`${this.checklistUrl}/${id}`);
  }

  getChecklistsByCard(cardId: number): Observable<Checklist[]> {
    return this.http.get<Checklist[]>(`${this.checklistUrl}/card/${cardId}`);
  }

  getProgress(cardId: number): Observable<number> {
    return this.http.get<number>(`${this.checklistUrl}/card/${cardId}/progress`);
  }
}
