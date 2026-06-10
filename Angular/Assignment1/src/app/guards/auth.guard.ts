import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { isLoggedIn } from '../rxjs/auth.operator';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  if (isLoggedIn()) {
    return true;
  } else {
    router.navigate(['/']);
    return false;
  }
};
