import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

/**
 * Estructura de rutas según el Documento de Diseño:
 * - "/"        → Landing Page pública (componente en la raíz).
 * - "/login"   → Formulario de acceso para Admin/Delegado.
 * - "/admin/*" → Sistema protegido por authGuard (recibos, etc.).
 *
 * Todos los componentes son Standalone: se cargan de forma perezosa (lazy)
 * con loadComponent para mantener el bundle inicial pequeño.
 */
export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/landing/landing.component').then((m) => m.LandingComponent),
    title: 'ICAP Juvenil',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then((m) => m.LoginComponent),
    title: 'Iniciar sesión — ICAP Juvenil',
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      {
        path: 'receipts',
        loadComponent: () =>
          import('./features/receipt/receipt-list.component').then(
            (m) => m.ReceiptListComponent,
          ),
        title: 'Recibos — ICAP Juvenil',
      },
      {
        path: 'receipts/new',
        loadComponent: () =>
          import('./features/receipt/receipt-form.component').then(
            (m) => m.ReceiptFormComponent,
          ),
        title: 'Nuevo recibo — ICAP Juvenil',
      },
      {
        path: 'receipts/:id',
        loadComponent: () =>
          import('./features/receipt/receipt-view.component').then(
            (m) => m.ReceiptViewComponent,
          ),
        title: 'Recibo — ICAP Juvenil',
      },
      { path: '', redirectTo: 'receipts', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
