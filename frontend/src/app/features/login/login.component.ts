import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';

import { LoginFormComponent } from './login-form.component';

/**
 * Página completa de acceso en "/login". Sigue existiendo (además del modal
 * que abre "Acceder" en la landing) porque authGuard redirige aquí a
 * cualquiera que intente entrar a "/admin/*" sin sesión — ese caso necesita
 * una URL real a la que redirigir, no un modal.
 */
@Component({
  selector: 'icap-login',
  standalone: true,
  imports: [MatCardModule, LoginFormComponent],
  template: `
    <div class="login-page">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>ICAP Juvenil</mat-card-title>
          <mat-card-subtitle>Acceso para delegados y administradores</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <icap-login-form (success)="onSuccess()" />
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: `
    .login-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #1a237e 0%, #3949ab 100%);
      padding: 16px;
    }

    .login-card {
      width: 100%;
      max-width: 400px;
      padding: 8px 8px 16px;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly router = inject(Router);

  onSuccess(): void {
    this.router.navigate(['/admin']);
  }
}
