import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth   = inject(AuthService);
  const router = inject(Router);
  const token  = auth.token();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      const isRefreshCall = req.url.includes('/api/auth/refresh');

      if (err.status === 401 && token && !isRefreshCall) {
        return auth.refresh().pipe(
          switchMap(res => {
            const retried = req.clone({ setHeaders: { Authorization: `Bearer ${res.accessToken}` } });
            return next(retried);
          }),
          catchError(refreshErr => {
            auth.clearSession();
            router.navigate(['/login']);
            return throwError(() => refreshErr);
          }),
        );
      }
      return throwError(() => err);
    }),
  );
};
