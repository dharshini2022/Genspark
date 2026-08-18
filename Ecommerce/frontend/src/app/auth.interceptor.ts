import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { AuthService } from "./services/auth.service";
import { BehaviorSubject, throwError, Observable } from "rxjs";
import { catchError, filter, switchMap, take } from "rxjs/operators";

let isRefreshing = false;
const refreshTokenSubject: BehaviorSubject<boolean | null> = new BehaviorSubject<boolean | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const role = sessionStorage.getItem('role');
  const headers: { [key: string]: string } = {};
  
  if (role) {
    const formattedRole = role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();
    headers['X-Role'] = formattedRole;
  }

  const token = authService.getAccessToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const cloned = req.clone({
    withCredentials: true,
    setHeaders: headers
  });

  return next(cloned).pipe(
    catchError((error) => {
      if (
        error instanceof HttpErrorResponse && 
        error.status === 401 && 
        !req.url.includes('/Auth/login') && 
        !req.url.includes('/Auth/refresh') &&
        !req.url.includes('/Auth/logout')
      ) {
        return handle401Error(cloned, next, authService);
      }
      return throwError(() => error);
    })
  );
};

function handle401Error(request: HttpRequest<any>, next: HttpHandlerFn, authService: AuthService): Observable<any> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null); 

    return authService.refreshToken().pipe(
      switchMap((res: any) => {
        isRefreshing = false;
        refreshTokenSubject.next(true); 
        
        const newToken = authService.getAccessToken();
        const retriedRequest = request.clone({
          setHeaders: {
            Authorization: newToken ? `Bearer ${newToken}` : ''
          }
        });
        return next(retriedRequest); 
      }),
      catchError((err) => {
        isRefreshing = false;
        refreshTokenSubject.next(false); 
        authService.logout().subscribe(); 
        return throwError(() => err);
      })
    );
  } else {
    return refreshTokenSubject.pipe(
      filter(result => result !== null),
      take(1),
      switchMap((success) => {
        if (success) {
          const newToken = authService.getAccessToken();
          const retriedRequest = request.clone({
            setHeaders: {
              Authorization: newToken ? `Bearer ${newToken}` : ''
            }
          });
          return next(retriedRequest);
        }
        return throwError(() => new Error('Authentication token refresh failed.'));
      })
    );
  }
}
