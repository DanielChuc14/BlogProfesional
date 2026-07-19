export interface RegisterRequest {
  username: string;
  displayName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  userId: string;
  username: string;
  displayName: string;
  email: string;
  roles: string[];
  avatarUrl: string | null;
  preferredLanguage: string;
}

export interface RefreshRequest {
  refreshToken?: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}
