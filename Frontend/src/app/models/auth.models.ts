// ── Request DTOs ──────────────────────────────────────────────────────────────
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

// ── Response DTOs ─────────────────────────────────────────────────────────────
export interface UserDto {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  avatarUrl: string | null;
  provider: string;
  role: string;          // 'Member' | 'BoardAdmin' | 'PlatformAdmin'
  isActive: boolean;
  lastLoginAt: string | null;
  createdAt: string;
}

export interface AuthResponseDto {
  token: string;
  expiresAt: string;
  user: UserDto;
}

// ── Generic API Envelope ──────────────────────────────────────────────────────
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}
