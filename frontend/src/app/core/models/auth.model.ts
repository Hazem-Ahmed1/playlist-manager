/** Mirrors PlaylistManagement.Api.DTOs.Auth.RegisterDto. */
export interface RegisterPayload {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

/** Mirrors PlaylistManagement.Api.DTOs.Auth.LoginDto. */
export interface LoginPayload {
  email: string;
  password: string;
}

/** Mirrors PlaylistManagement.Api.DTOs.Auth.AuthResponseDto. */
export interface AuthUser {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  token: string;
  expiresAt: string;
}
