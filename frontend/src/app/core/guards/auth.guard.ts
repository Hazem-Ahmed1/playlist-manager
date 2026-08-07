import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStateService } from '../services/auth-state.service';
import { ModalService } from '../services/modal.service';

/** Requires a logged-in user. Anonymous visitors get sent home with the login modal opened, rather than a dead-end route. */
export const authGuard: CanActivateFn = () => {
  const authState = inject(AuthStateService);
  const modal = inject(ModalService);
  const router = inject(Router);

  if (authState.isLoggedIn()) {
    return true;
  }

  modal.openLogin();
  return router.createUrlTree(['/']);
};
