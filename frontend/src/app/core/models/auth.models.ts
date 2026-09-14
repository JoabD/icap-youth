/** Espejo de LoginQuery (Icap.Application.Auth.Commands.Login). */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Espejo de LoginResultDto (Icap.Application.Auth.DTOs). */
export interface LoginResult {
  accessToken: string;
  userId: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Delegate';
}

/** Sesión persistida en el cliente (derivada de LoginResult, sin el token en claro en memoria del componente). */
export interface AuthenticatedUser {
  userId: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Delegate';
}
