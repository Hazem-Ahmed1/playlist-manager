import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AuthUser, LoginPayload, RegisterPayload } from '../models/auth.model';
import { AuthStateService } from './auth-state.service';

/** Calls POST /api/auth/register and /api/auth/login, and updates AuthStateService on success. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly authState = inject(AuthStateService);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  register(payload: RegisterPayload): Observable<AuthUser> {
    return this.http.post<ApiResponse<AuthUser>>(`${this.baseUrl}/register`, payload).pipe(
      map((res) => res.data),
      tap((user) => this.authState.setUser(user)),
    );
  }

  login(payload: LoginPayload): Observable<AuthUser> {
    return this.http.post<ApiResponse<AuthUser>>(`${this.baseUrl}/login`, payload).pipe(
      map((res) => res.data),
      tap((user) => this.authState.setUser(user)),
    );
  }

  logout(): void {
    this.authState.logout();
  }
}
