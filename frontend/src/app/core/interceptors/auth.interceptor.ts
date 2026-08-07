import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStateService } from '../services/auth-state.service';
import { ToastService } from '../services/toast.service';

/**
 * Attaches the current JWT (if any) to every request bound for our own
 * API, and logs the user out if the server ever says the token is no
 * longer valid — keeps that logic in one place instead of every service
 * checking for 401 itself.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authState = inject(AuthStateService);
  const toast = inject(ToastService);

  const isApiRequest = req.url.startsWith(environment.apiUrl);
  const token = authState.getToken();

  const authorizedReq =
    isApiRequest && token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authorizedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (isApiRequest && error.status === 401 && authState.isLoggedIn()) {
        authState.logout();
        toast.warning('Your session has expired. Please log in again.');
      }
      return throwError(() => error);
    }),
  );
};
