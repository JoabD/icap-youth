import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

/**
 * Estructura de rutas:
 * - "/"        → Landing Page pública.
 * - "/login"   → Formulario de acceso para Admin/Delegado.
 * - "/admin/*" → Dashboard protegido por authGuard, con DashboardLayoutComponent
 *                (sidebar + topbar) envolviendo cada sección; "/admin/users"
 *                además exige adminGuard (rol Admin).
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
    loadComponent: () =>
      import('./layout/dashboard-layout.component').then((m) => m.DashboardLayoutComponent),
    children: [
      {
        path: 'receipts',
        loadComponent: () =>
          import('./features/receipt/receipt-list.component').then(
            (m) => m.ReceiptListComponent,
          ),
        title: 'Administrar recibos — ICAP Juvenil',
      },
      {
        path: 'receipts/new',
        loadComponent: () =>
          import('./features/receipt/receipt-form.component').then(
            (m) => m.ReceiptFormComponent,
          ),
        title: 'Generar recibo — ICAP Juvenil',
      },
      {
        path: 'receipts/:id',
        loadComponent: () =>
          import('./features/receipt/receipt-view.component').then(
            (m) => m.ReceiptViewComponent,
          ),
        title: 'Recibo — ICAP Juvenil',
      },
      {
        path: 'finance',
        loadComponent: () =>
          import('./features/finance/finance.component').then((m) => m.FinanceComponent),
        title: 'Finanzas — ICAP Juvenil',
      },
      {
        path: 'users',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/users/users.component').then((m) => m.UsersComponent),
        title: 'Usuarios — ICAP Juvenil',
      },
      { path: '', redirectTo: 'receipts', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
