import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpInterceptorFn } from '@angular/common/http';
import { routes } from './app.routes';

// ── JWT Interceptor ─────────────────────────────────────────────────────────
// Attaches the stored token to every outgoing request automatically.
const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('fb_token');
  if (token) {
    const cloned = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
    return next(cloned);
  }
  return next(req);
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
  ]
};
