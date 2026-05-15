import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, UserDto } from '../models/auth.models';
import { AuthService } from './auth.service';

export interface AdminDashboardDto {
  totalUsers: number;
  activeUsers: number;
  suspendedUsers: number;
  memberCount: number;
  boardAdminCount: number;
  platformAdminCount: number;
  totalWorkspaces: number;
  totalBoards: number;
  totalCards: number;
  overdueCards: number;
}

export interface PlatformAnalyticsDto {
  totalUsers: number;
  activeUsers: number;
  newUsersThisMonth: number;
  newUsersThisWeek: number;
  roleDistribution: Record<string, number>;
  recentLogins: Array<{ email: string; role: string; lastLoginAt: string }>;
}

export interface AuditLogDto {
  timestamp: string;
  action: string;
  detail: string;
  actor: string;
}

export interface ActivityReportDto {
  generatedAt: string;
  generatedBy: string;
  totalUsers: number;
  activeUsers: number;
  suspendedUsers: number;
  recentSignups: any[];
  auditSummary: string;
}

const isProd = !window.location.hostname.includes('localhost');
const ADMIN_BASE = isProd ? 'https://auth-service.onrender.com/api/admin' : '/api/admin';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http    = inject(HttpClient);
  private authSvc = inject(AuthService);

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.authSvc.getToken()}` });
  }

  // ── Users ──────────────────────────────────────────────────────────────────
  getDashboard():                                  Observable<ApiResponse<AdminDashboardDto>>   { return this.http.get<any>(`${ADMIN_BASE}/dashboard`, { headers: this.headers() }); }
  getAllUsers():                                    Observable<ApiResponse<UserDto[]>>           { return this.http.get<any>(`${ADMIN_BASE}/users`, { headers: this.headers() }); }
  changeRole(userId: string, role: string):        Observable<ApiResponse<UserDto>>             { return this.http.put<any>(`${ADMIN_BASE}/users/${userId}/role`, { role }, { headers: this.headers() }); }
  suspendUser(userId: string):                     Observable<ApiResponse<UserDto>>             { return this.http.put<any>(`${ADMIN_BASE}/users/${userId}/suspend`, {}, { headers: this.headers() }); }
  reactivateUser(userId: string):                  Observable<ApiResponse<UserDto>>             { return this.http.put<any>(`${ADMIN_BASE}/users/${userId}/reactivate`, {}, { headers: this.headers() }); }
  deleteUser(userId: string):                      Observable<void>                             { return this.http.delete<void>(`${ADMIN_BASE}/users/${userId}`, { headers: this.headers() }); }

  // ── Workspaces ─────────────────────────────────────────────────────────────
  getWorkspaces():                                 Observable<any>                              { return this.http.get<any>(`${ADMIN_BASE}/workspaces`, { headers: this.headers() }); }
  deleteWorkspace(id: number):                     Observable<any>                              { return this.http.delete<any>(`${ADMIN_BASE}/workspaces/${id}`, { headers: this.headers() }); }

  // ── Boards ──────────────────────────────────────────────────────────────────
  getBoards():                                     Observable<any>                              { return this.http.get<any>(`${ADMIN_BASE}/boards`, { headers: this.headers() }); }
  deleteBoard(id: number):                         Observable<any>                              { return this.http.delete<any>(`${ADMIN_BASE}/boards/${id}`, { headers: this.headers() }); }

  // ── Cards ──────────────────────────────────────────────────────────────────
  getAllCards():                                    Observable<any>                              { return this.http.get<any>(`${ADMIN_BASE}/cards`, { headers: this.headers() }); }
  getOverdueCards():                               Observable<any>                              { return this.http.get<any>(`${ADMIN_BASE}/cards/overdue`, { headers: this.headers() }); }

  // ── Analytics & Reports ───────────────────────────────────────────────────
  getAnalytics():                                  Observable<ApiResponse<PlatformAnalyticsDto>>{ return this.http.get<any>(`${ADMIN_BASE}/analytics`, { headers: this.headers() }); }
  getAuditLogs():                                  Observable<ApiResponse<AuditLogDto[]>>       { return this.http.get<any>(`${ADMIN_BASE}/audit-logs`, { headers: this.headers() }); }
  generateReport():                                Observable<ApiResponse<ActivityReportDto>>   { return this.http.get<any>(`${ADMIN_BASE}/report`, { headers: this.headers() }); }

  // ── Platform Notification ─────────────────────────────────────────────────
  sendPlatformNotification(title: string, message: string): Observable<ApiResponse<string>>    { return this.http.post<any>(`${ADMIN_BASE}/notify`, { title, message }, { headers: this.headers() }); }
}
