import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard funcional adicional a authGuard: además de estar autenticado, exige
 * rol Admin. Protege "/admin/users" (un Delegate no debe ni ver la pantalla,
 * aunque el backend igual la rechazaría con 403 — este guard evita el
 * viaje redondo innecesario y la confusión de ver una pantalla que truena).
 */
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdmin()) {
    return true;
  }

  return router.createUrlTree(['/admin/receipts']);
};
