import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { LoginFormComponent } from './login-form.component';

/**
 * Versión del login en modal (Material Dialog), la que abre el botón
 * "Acceder" de la landing (ver LandingComponent.openLogin()). Reutiliza el
 * mismo <icap-login-form> que la página "/login" — la única diferencia es
 * el contenedor (diálogo en vez de página completa) y qué pasa después de
 * un login exitoso: aquí se cierra el modal y se navega a "/admin".
 */
@Component({
  selector: 'icap-login-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, LoginFormComponent],
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>Acceder</h2>
      <button mat-icon-button mat-dialog-close aria-label="Cerrar">
        <mat-icon>close</mat-icon>
      </button>
    </div>

    <mat-dialog-content>
      <p class="subtitle">Acceso para delegados y administradores</p>
      <icap-login-form (success)="onSuccess()" />
    </mat-dialog-content>
  `,
  styles: `
    .dialog-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 8px 8px 0 24px;

      h2 {
        margin: 0;
      }
    }

    .subtitle {
      margin: 0 0 16px;
      color: #6b7280;
      font-size: 0.9rem;
    }

    mat-dialog-content {
      padding-bottom: 8px;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<LoginDialogComponent>);
  private readonly router = inject(Router);

  onSuccess(): void {
    this.dialogRef.close();
    this.router.navigate(['/admin']);
  }
}
