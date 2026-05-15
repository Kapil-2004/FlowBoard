import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { WorkspaceResponseDto, WorkspaceCreateDto, WorkspaceMemberDto } from '../models/workspace.models';

const isProd = !window.location.hostname.includes('localhost');
const API_BASE = isProd ? 'https://workspace-service.onrender.com/api/workspaces' : '/api/workspaces';

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private _workspaces = signal<WorkspaceResponseDto[]>([]);
  readonly workspaces = this._workspaces.asReadonly();

  constructor(private http: HttpClient) {}

  loadWorkspaces(): Observable<WorkspaceResponseDto[]> {
    return this.http.get<WorkspaceResponseDto[]>(`${API_BASE}/member`).pipe(
      tap(data => this._workspaces.set(data))
    );
  }

  createWorkspace(payload: WorkspaceCreateDto): Observable<WorkspaceResponseDto> {
    return this.http.post<WorkspaceResponseDto>(API_BASE, payload).pipe(
      tap(newWorkspace => {
        this._workspaces.update(workspaces => [...workspaces, newWorkspace]);
      })
    );
  }

  getWorkspace(id: number): Observable<WorkspaceResponseDto> {
    return this.http.get<WorkspaceResponseDto>(`${API_BASE}/${id}`);
  }

  getMembers(workspaceId: number): Observable<WorkspaceMemberDto[]> {
    return this.http.get<WorkspaceMemberDto[]>(`${API_BASE}/${workspaceId}/members`);
  }

  addMember(workspaceId: number, userId: string, role: string = 'MEMBER'): Observable<WorkspaceMemberDto> {
    return this.http.post<WorkspaceMemberDto>(`${API_BASE}/${workspaceId}/members`, { userId, role });
  }

  removeMember(workspaceId: number, userId: string): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/${workspaceId}/members/${userId}`);
  }
}
