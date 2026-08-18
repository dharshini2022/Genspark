import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const currentUser = authService.currentUserValue;

  if (currentUser) {
    return true;
  }

  // Redirect to login, passing the original state URL as returnUrl query param
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
