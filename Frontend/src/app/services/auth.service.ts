import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponseDto,
  UserDto,
  ApiResponse,
} from '../models/auth.models';

const API_BASE = '/api/auth';
const TOKEN_KEY = 'fb_token';
const USER_KEY  = 'fb_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // ── Reactive state ──────────────────────────────────────────────────────
  private _currentUser = signal<UserDto | null>(this._loadUser());
  private _token       = signal<string | null>(this._loadToken());

  /** Read-only signal for the logged-in user */
  readonly currentUser  = this._currentUser.asReadonly();
  /** Derived boolean signal */
  readonly isLoggedIn   = computed(() => this._token() !== null);

  constructor(private http: HttpClient, private router: Router) {}

  // ── Public API ──────────────────────────────────────────────────────────

  login(payload: LoginRequest): Observable<ApiResponse<AuthResponseDto>> {
    return this.http
      .post<ApiResponse<AuthResponseDto>>(`${API_BASE}/login`, payload)
      .pipe(
        tap(res => { if (res.success && res.data) this._persist(res.data); }),
        catchError(this._handleError)
      );
  }

  register(payload: RegisterRequest): Observable<ApiResponse<AuthResponseDto>> {
    return this.http
      .post<ApiResponse<AuthResponseDto>>(`${API_BASE}/register`, payload)
      .pipe(
        tap(res => { if (res.success && res.data) this._persist(res.data); }),
        catchError(this._handleError)
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._token.set(null);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this._token();
  }

  // ── Private helpers ─────────────────────────────────────────────────────

  private _persist(data: AuthResponseDto): void {
    localStorage.setItem(TOKEN_KEY, data.token);
    localStorage.setItem(USER_KEY, JSON.stringify(data.user));
    this._token.set(data.token);
    this._currentUser.set(data.user);
  }

  private _loadToken(): string | null {
    try { return localStorage.getItem(TOKEN_KEY); } catch { return null; }
  }

  private _loadUser(): UserDto | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch { return null; }
  }

  private _handleError(err: HttpErrorResponse): Observable<never> {
    let message = 'Something went wrong. Please try again.';
    if (err.error && err.error.message) {
      message = err.error.message;
    } else if (err.status === 0) {
      message = 'Cannot connect to server. Please check your connection.';
    }
    return throwError(() => new Error(message));
  }
}
