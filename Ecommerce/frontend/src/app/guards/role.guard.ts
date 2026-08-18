import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

function normalizeRole(role: string): string {
  if (!role) return '';
  const r = role.trim().toUpperCase();
  if (r === 'ADMIN') return 'ADMIN';
  if (r === 'VENDOR') return 'VENDOR';
  if (r === 'CUSTOMER') return 'CUSTOMER';
  return r;
}

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const currentUser = authService.currentUserValue;

  if (!currentUser) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const userRole = currentUser.role;
  if (!userRole) {
    return router.createUrlTree(['/login']);
  }

  const allowedRoles = route.data?.['roles'] as string[];
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  const normalizedUserRole = normalizeRole(userRole);
  const isAuthorized = allowedRoles.some(role => normalizeRole(role) === normalizedUserRole);

  if (isAuthorized) {
    return true;
  }

  return router.createUrlTree(['/access-denied']);
};
