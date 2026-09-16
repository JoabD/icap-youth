import { ChangeDetectionStrategy, Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../core/services/auth.service';

/**
 * Formulario de acceso puro (sin card ni layout de página): solo el
 * <form> reactivo y la llamada a AuthService.login(). Se extrajo de
 * LoginComponent para poder reutilizarlo tanto en la página completa
 * "/login" (bookmarks, redirect de authGuard) como en LoginDialogComponent
 * (el modal que abre el botón "Acceder" de la landing) sin duplicar la
 * lógica de validación/envío en dos lugares.
 *
 * Este componente NO decide qué pasa después de un login exitoso (no
 * navega ni cierra nada) — solo emite `success`; cada contenedor (página o
 * diálogo) decide su propio siguiente paso.
 */
@Component({
  selector: 'icap-login-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()" class="login-form" novalidate>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Correo electrónico</mat-label>
        <input matInput type="email" formControlName="email" autocomplete="username" />
        <mat-icon matSuffix>mail</mat-icon>
        @if (form.controls.email.hasError('required') && form.controls.email.touched) {
          <mat-error>El correo es requerido.</mat-error>
        }
        @if (form.controls.email.hasError('email') && form.controls.email.touched) {
          <mat-error>Ingresa un correo válido.</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Contraseña</mat-label>
        <input
          matInput
          [type]="hidePassword() ? 'password' : 'text'"
          formControlName="password"
          autocomplete="current-password"
        />
        <button
          mat-icon-button
          matSuffix
          type="button"
          (click)="togglePasswordVisibility()"
          [attr.aria-label]="'Mostrar contraseña'"
          [attr.aria-pressed]="!hidePassword()"
        >
          <mat-icon>{{ hidePassword() ? 'visibility_off' : 'visibility' }}</mat-icon>
        </button>
        @if (form.controls.password.hasError('required') && form.controls.password.touched) {
          <mat-error>La contraseña es requerida.</mat-error>
        }
      </mat-form-field>

      @if (errorMessage()) {
        <p class="error-banner" role="alert">{{ errorMessage() }}</p>
      }

      <button
        mat-flat-button
        color="primary"
        type="submit"
        class="full-width submit-button"
        [disabled]="isLoading()"
      >
        @if (isLoading()) {
          <mat-spinner diameter="20" />
        } @else {
          <span>Iniciar sesión</span>
        }
      </button>
    </form>
  `,
  styles: `
    .login-form {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .full-width {
      width: 100%;
    }

    .submit-button {
      height: 44px;
      margin-top: 8px;
    }

    .error-banner {
      color: #b71c1c;
      font-size: 0.875rem;
      margin: 0 0 8px;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  /** Emitido tras un login exitoso; el contenedor decide qué hacer después. */
  @Output() readonly success = new EventEmitter<void>();

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly hidePassword = signal(true);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  togglePasswordVisibility(): void {
    this.hidePassword.update((hidden) => !hidden);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.form.getRawValue();

    this.authService.login({ email, password }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.success.emit();
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.errorMessage.set(
          error.status === 401
            ? 'Correo o contraseña incorrectos.'
            : 'No se pudo iniciar sesión. Intenta de nuevo más tarde.',
        );
      },
    });
  }
}
