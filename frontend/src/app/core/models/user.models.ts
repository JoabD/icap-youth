/** Espejo de UserDto (Icap.Application.Users.DTOs). */
export interface AppUser {
  id: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Delegate';
  isActive: boolean;
}

/** Espejo de CreateUserRequest (Icap.WebApi.Controllers.UsersController). */
export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: 'Admin' | 'Delegate';
}

/** Espejo de UpdateUserRequest. */
export interface UpdateUserRequest {
  fullName: string;
  email: string;
  role: 'Admin' | 'Delegate';
}
