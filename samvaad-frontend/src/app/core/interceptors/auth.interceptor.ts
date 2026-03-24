import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const addToken = (req: Parameters<HttpInterceptorFn>[0], token: string | null) =>
  token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(addToken(req, auth.getAccessToken())).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only attempt refresh for 401s on non-auth endpoints
      if (error.status === 401 && !req.url.includes('/auth/')) {
        return auth.refresh().pipe(
          switchMap(() => next(addToken(req, auth.getAccessToken()))),
          catchError(refreshError => {
            auth.logout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
