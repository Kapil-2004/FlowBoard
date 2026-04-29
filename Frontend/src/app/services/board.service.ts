import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BoardResponseDto, BoardCreateDto, BoardUpdateDto, BoardMemberDto, AddBoardMemberDto } from '../models/board.models';

@Injectable({
  providedIn: 'root'
})
export class BoardService {
  private apiUrl = 'http://localhost:5003/api/boards';

  constructor(private http: HttpClient) { }

  createBoard(dto: BoardCreateDto): Observable<BoardResponseDto> {
    return this.http.post<BoardResponseDto>(this.apiUrl, dto);
  }

  getBoardById(id: number): Observable<BoardResponseDto> {
    return this.http.get<BoardResponseDto>(`${this.apiUrl}/${id}`);
  }

  getBoardsByWorkspace(workspaceId: number): Observable<BoardResponseDto[]> {
    return this.http.get<BoardResponseDto[]>(`${this.apiUrl}/workspace/${workspaceId}`);
  }

  getBoardsByMember(): Observable<BoardResponseDto[]> {
    return this.http.get<BoardResponseDto[]>(`${this.apiUrl}/member`);
  }

  updateBoard(id: number, dto: BoardUpdateDto): Observable<BoardResponseDto> {
    return this.http.put<BoardResponseDto>(`${this.apiUrl}/${id}`, dto);
  }

  closeBoard(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/close`, {});
  }

  deleteBoard(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  addMember(boardId: number, dto: AddBoardMemberDto): Observable<BoardMemberDto> {
    return this.http.post<BoardMemberDto>(`${this.apiUrl}/${boardId}/members`, dto);
  }

  removeMember(memberId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/members/${memberId}`);
  }

  updateMemberRole(memberId: number, role: string): Observable<BoardMemberDto> {
    return this.http.put<BoardMemberDto>(`${this.apiUrl}/members/${memberId}/role`, { role });
  }

  getMembers(boardId: number): Observable<BoardMemberDto[]> {
    return this.http.get<BoardMemberDto[]>(`${this.apiUrl}/${boardId}/members`);
  }
}
