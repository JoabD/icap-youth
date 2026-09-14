import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthenticatedUser, LoginRequest, LoginResult } from '../models/auth.models';

const TOKEN_STORAGE_KEY = 'icap_access_token';
const USER_STORAGE_KEY = 'icap_current_user';

/**
 * Maneja el estado de sesión con Signals (signal/computed). RxJS se usa
 * únicamente para la llamada HTTP de login, tal como indica el documento
 * de diseño ("RxJS se limitará exclusivamente a las llamadas HTTP").
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _currentUser = signal<AuthenticatedUser | null>(this.restoreUser());
  private readonly _accessToken = signal<string | null>(localStorage.getItem(TOKEN_STORAGE_KEY));

  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly isAdmin = computed(() => this._currentUser()?.role === 'Admin');

  get accessToken(): string | null {
    return this._accessToken();
  }

  login(request: LoginRequest): Observable<LoginResult> {
    return this.http.post<LoginResult>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((result) => this.setSession(result)),
    );
  }

  logout(): void {
    this._currentUser.set(null);
    this._accessToken.set(null);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(USER_STORAGE_KEY);
  }

  private setSession(result: LoginResult): void {
    const user: AuthenticatedUser = {
      userId: result.userId,
      fullName: result.fullName,
      email: result.email,
      role: result.role,
    };

    localStorage.setItem(TOKEN_STORAGE_KEY, result.accessToken);
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(user));

    this._accessToken.set(result.accessToken);
    this._currentUser.set(user);
  }

  private restoreUser(): AuthenticatedUser | null {
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as AuthenticatedUser;
    } catch {
      return null;
    }
  }
}
