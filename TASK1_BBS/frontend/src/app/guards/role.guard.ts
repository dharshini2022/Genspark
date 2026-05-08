import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const requiredRole: string = route.data?.['role'];

  if (!auth.isLoggedIn) {
    router.navigate(['/auth/login']);
    return false;
  }
  if (requiredRole && auth.role !== requiredRole) {
    const redirects: Record<string, string> = {
      Customer: '/user/home', Operator: '/operator/dashboard', Admin: '/admin/dashboard'
    };
    router.navigate([redirects[auth.role] ?? '/']);
    return false;
  }
  return true;
};
