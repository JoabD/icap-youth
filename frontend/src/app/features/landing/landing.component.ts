import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';

import { LoginDialogComponent } from '../login/login-dialog.component';

/**
 * Landing Page pública en "/": navbar con el logo de la Sociedad Juvenil
 * Amigos de Cristo y el botón "Acceder", que abre el login en un modal
 * (LoginDialogComponent) en vez de navegar a una página aparte — así el
 * usuario nunca "pierde" la landing detrás del formulario. Alcance
 * intencional de esta iteración: solo la portada + acceso, tal como pidió
 * el usuario ("de momento tenga solo el logo... un Navbar con el botón de
 * Acceder").
 */
@Component({
  selector: 'icap-landing',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  template: `
    <div class="landing">
      <nav class="navbar">
        <div class="navbar-brand">
          <img src="assets/logo-sociedad-juvenil.png" alt="Sociedad Juvenil Amigos de Cristo" />
          <span>ICAP Juvenil</span>
        </div>
        <button mat-flat-button color="primary" (click)="openLogin()">
          Acceder
          <mat-icon>arrow_forward</mat-icon>
        </button>
      </nav>

      <section class="hero">
        <img src="assets/logo-sociedad-juvenil.png" alt="Sociedad Juvenil Amigos de Cristo" class="hero-logo" />
        <h1>Sociedad Juvenil Amigos de Cristo</h1>
        <p>Plataforma de emisión y control de recibos — ICAP Juvenil</p>
      </section>
    </div>
  `,
  styles: `
    .landing {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background: #f9fafb;
    }

    .navbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px 32px;
      background: #111827;
      gap: 12px;
    }

    .navbar-brand {
      display: flex;
      align-items: center;
      gap: 10px;
      color: #fff;
      font-weight: 700;
      min-width: 0;

      span {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
    }

    .navbar-brand img {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: #fff;
      object-fit: contain;
      flex-shrink: 0;
    }

    .hero {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 16px;
      text-align: center;
      padding: 24px;
    }

    .hero-logo {
      width: 160px;
      height: 160px;
      object-fit: contain;
    }

    .hero h1 {
      margin: 0;
      color: #111827;
      font-size: 1.6rem;
      max-width: 500px;
    }

    .hero p {
      margin: 0;
      color: #6b7280;
    }

    @media (max-width: 480px) {
      .navbar {
        padding: 12px 16px;
      }

      .navbar-brand span {
        display: none;
      }

      .hero {
        padding: 16px;
      }

      .hero-logo {
        width: 110px;
        height: 110px;
      }

      .hero h1 {
        font-size: 1.25rem;
      }

      .hero p {
        font-size: 0.9rem;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingComponent {
  private readonly dialog = inject(MatDialog);

  openLogin(): void {
    this.dialog.open(LoginDialogComponent, {
      width: '440px',
      maxWidth: '92vw',
      autoFocus: 'first-tabbable',
    });
  }
}
