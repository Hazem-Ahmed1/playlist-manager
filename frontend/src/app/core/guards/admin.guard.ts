import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from '../services/auth-state.service';

/** Requires an Admin-role user (e.g. the catalog dashboard). Everyone else, including logged-out visitors, is redirected to the 403 page. */
export const adminGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const router = inject(Router);

  if (authState.isLoggedIn() && authState.isAdmin()) {
    return true;
  }

  return router.createUrlTree(['/403']);
};
