import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface UserSearchDto {
  userId: string;
  fullName: string;
  email: string;
  avatarUrl?: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private API_BASE = 'http://localhost:5001/api/auth';

  searchUsers(query: string): Observable<UserSearchDto[]> {
    return this.http.get<ApiResponse<UserSearchDto[]>>(`${this.API_BASE}/users/search?q=${query}`).pipe(
      map(res => res.data)
    );
  }
}
