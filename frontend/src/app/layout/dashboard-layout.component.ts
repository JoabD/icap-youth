import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AuthService } from '../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  path: string;
  adminOnly: boolean;
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Generar recibo', icon: 'add_circle', path: '/admin/receipts/new', adminOnly: false },
  { label: 'Administrar recibos', icon: 'receipt_long', path: '/admin/receipts', adminOnly: false },
  { label: 'Finanzas', icon: 'payments', path: '/admin/finance', adminOnly: false },
  { label: 'Usuarios', icon: 'group', path: '/admin/users', adminOnly: true },
];

/**
 * Shell del panel interno: sidebar de navegación + barra superior con la
 * sesión activa. Todas las pantallas bajo "/admin" (excepto la vista de
 * impresión de un recibo, que necesita ir a pantalla completa para
 * imprimir) se renderizan dentro de <router-outlet>.
 */
@Component({
  selector: 'icap-dashboard-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatIconModule, MatButtonModule, MatToolbarModule, MatTooltipModule],
  template: `
    <div class="shell">
      <aside class="sidebar">
        <div class="brand">
          <img src="assets/logo-sociedad-juvenil.png" alt="Sociedad Juvenil Amigos de Cristo" />
          <span>ICAP Juvenil</span>
        </div>

        <nav class="nav">
          @for (item of visibleNavItems(); track item.path) {
            <a
              class="nav-item"
              [routerLink]="item.path"
              routerLinkActive="nav-item--active"
              [routerLinkActiveOptions]="{ exact: false }"
            >
              <mat-icon>{{ item.icon }}</mat-icon>
              <span>{{ item.label }}</span>
            </a>
          }
        </nav>
      </aside>

      <div class="main">
        <mat-toolbar class="topbar">
          <span class="spacer"></span>
          <span class="user-name">{{ authService.currentUser()?.fullName }}</span>
          <span class="user-role">{{ roleLabel() }}</span>
          <button mat-icon-button matTooltip="Cerrar sesión" (click)="logout()">
            <mat-icon>logout</mat-icon>
          </button>
        </mat-toolbar>

        <main class="content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: `
    .shell {
      display: flex;
      min-height: 100vh;
      background: #f3f4f6;
    }

    .sidebar {
      width: 240px;
      flex-shrink: 0;
      background: #111827;
      color: #e5e7eb;
      display: flex;
      flex-direction: column;
      padding: 20px 12px;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 0 10px 20px;
      margin-bottom: 12px;
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);

      img {
        width: 34px;
        height: 34px;
        object-fit: contain;
        border-radius: 50%;
        background: #fff;
      }

      span {
        font-weight: 700;
        font-size: 1rem;
      }
    }

    .nav {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px 12px;
      border-radius: 8px;
      color: #cbd5e1;
      text-decoration: none;
      font-size: 0.9rem;
      transition: background-color 0.15s ease, color 0.15s ease;

      mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
      }

      &:hover {
        background: rgba(255, 255, 255, 0.06);
        color: #fff;
      }
    }

    .nav-item--active {
      background: #1f2937;
      color: #fff;
      font-weight: 600;
      box-shadow: inset 3px 0 0 #f59e0b;
    }

    .main {
      flex: 1;
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .topbar {
      background: #fff !important;
      color: #111827 !important;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
    }

    .spacer {
      flex: 1;
    }

    .user-name {
      font-weight: 600;
      margin-right: 8px;
    }

    .user-role {
      font-size: 0.75rem;
      color: #6b7280;
      background: #f3f4f6;
      padding: 2px 10px;
      border-radius: 999px;
      margin-right: 8px;
    }

    .content {
      flex: 1;
      overflow-y: auto;
    }

    @media (max-width: 720px) {
      .shell {
        flex-direction: column;
      }

      .sidebar {
        width: 100%;
        flex-direction: row;
        overflow-x: auto;
        padding: 10px;
      }

      .brand {
        display: none;
      }

      .nav {
        flex-direction: row;
      }

      .nav-item span {
        display: none;
      }
    }

    @media (max-width: 480px) {
      .user-name {
        display: none;
      }

      .content {
        padding: 0;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardLayoutComponent {
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  visibleNavItems(): NavItem[] {
    const isAdmin = this.authService.isAdmin();
    return NAV_ITEMS.filter((item) => !item.adminOnly || isAdmin);
  }

  roleLabel(): string {
    return this.authService.currentUser()?.role === 'Admin' ? 'Administrador' : 'Delegado';
  }

  logout(): void {
    this.authService.logout();
    // Al cerrar sesión se regresa a la raíz (landing pública), no a /login:
    // así el usuario cae en la portada y decide si vuelve a "Acceder" (que
    // ahora abre el login en un modal) en vez de aterrizar directo en un
    // formulario de login a pantalla completa.
    this.router.navigate(['/']);
  }
}
