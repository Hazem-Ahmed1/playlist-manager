import { Injectable, computed, signal } from '@angular/core';
import { AuthUser } from '../models/auth.model';
import { ADMIN_ROLE } from '../constants/roles';
import { getRolesFromToken } from '../utils/jwt.util';

const STORAGE_KEY = 'pm_auth_user';

/**
 * Single source of truth for "who is logged in", persisted to
 * localStorage so a page refresh doesn't lose the session. The backend
 * never returns a role list in AuthResponseDto — role lives only in the
 * JWT's claims, so isAdmin decodes the token rather than trusting a
 * separate field.
 */
@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly currentUserSignal = signal<AuthUser | null>(readStoredUser());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isLoggedIn = computed(() => this.currentUserSignal() !== null);
  readonly isAdmin = computed(() => {
    const user = this.currentUserSignal();
    return user ? getRolesFromToken(user.token).includes(ADMIN_ROLE) : false;
  });

  setUser(user: AuthUser): void {
    this.currentUserSignal.set(user);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
  }

  logout(): void {
    this.currentUserSignal.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  getToken(): string | null {
    return this.currentUserSignal()?.token ?? null;
  }
}

function readStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthUser;
  } catch {
    return null;
  }
}
