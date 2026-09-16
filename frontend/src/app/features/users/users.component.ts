import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { AppUser } from '../../core/models/user.models';

type FormMode = 'hidden' | 'create' | 'edit';

/**
 * Administración de usuarios (Admin/Delegate). Pantalla accesible solo para
 * Admin (ver adminGuard en app.routes.ts); el backend además la protege por
 * su cuenta con [Authorize(Roles = "Admin")] en UsersController, así que
 * esta UI es "defensa en profundidad", no la única barrera.
 */
@Component({
  selector: 'icap-users',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  template: `
    <div class="users-page">
      <div class="page-header">
        <h1>Usuarios</h1>
        @if (formMode() === 'hidden') {
          <button mat-flat-button color="primary" (click)="startCreate()">
            <mat-icon>person_add</mat-icon>
            Nuevo usuario
          </button>
        }
      </div>

      @if (formMode() !== 'hidden') {
        <mat-card class="form-card">
          <h2>{{ formMode() === 'create' ? 'Nuevo usuario' : 'Editar usuario' }}</h2>

          <form [formGroup]="form" (ngSubmit)="submit()" class="user-form">
            <mat-form-field appearance="outline">
              <mat-label>Nombre completo</mat-label>
              <input matInput formControlName="fullName" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Correo electrónico</mat-label>
              <input matInput type="email" formControlName="email" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Rol</mat-label>
              <mat-select formControlName="role">
                <mat-option value="Delegate">Delegado</mat-option>
                <mat-option value="Admin">Administrador</mat-option>
              </mat-select>
            </mat-form-field>

            @if (formMode() === 'create') {
              <mat-form-field appearance="outline">
                <mat-label>Contraseña</mat-label>
                <input matInput type="password" formControlName="password" />
                <mat-hint>Mínimo 8 caracteres.</mat-hint>
              </mat-form-field>
            }

            @if (errorMessage()) {
              <p class="error-banner" role="alert">{{ errorMessage() }}</p>
            }

            <div class="form-actions">
              <button mat-button type="button" (click)="cancelForm()">Cancelar</button>
              <button mat-flat-button color="primary" type="submit" [disabled]="isSaving()">
                @if (isSaving()) {
                  <mat-spinner diameter="18" />
                } @else {
                  <span>Guardar</span>
                }
              </button>
            </div>
          </form>
        </mat-card>
      }

      @if (resettingPasswordFor(); as resetTarget) {
        <mat-card class="form-card">
          <h2>Restablecer contraseña — {{ resetTarget.fullName }}</h2>
          <form [formGroup]="passwordForm" (ngSubmit)="submitPasswordReset(resetTarget.id)" class="user-form">
            <mat-form-field appearance="outline">
              <mat-label>Nueva contraseña</mat-label>
              <input matInput type="password" formControlName="newPassword" />
              <mat-hint>Mínimo 8 caracteres.</mat-hint>
            </mat-form-field>

            <div class="form-actions">
              <button mat-button type="button" (click)="cancelPasswordReset()">Cancelar</button>
              <button mat-flat-button color="primary" type="submit" [disabled]="passwordForm.invalid || isSaving()">
                Restablecer
              </button>
            </div>
          </form>
        </mat-card>
      }

      @if (isLoading()) {
        <div class="state-container">
          <mat-spinner diameter="36" />
        </div>
      } @else {
        <mat-card class="table-card">
          <div class="table-scroll">
          <table mat-table [dataSource]="users()" class="users-table">
            <ng-container matColumnDef="fullName">
              <th mat-header-cell *matHeaderCellDef>Nombre</th>
              <td mat-cell *matCellDef="let user">{{ user.fullName }}</td>
            </ng-container>

            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef>Correo</th>
              <td mat-cell *matCellDef="let user">{{ user.email }}</td>
            </ng-container>

            <ng-container matColumnDef="role">
              <th mat-header-cell *matHeaderCellDef>Rol</th>
              <td mat-cell *matCellDef="let user">{{ user.role === 'Admin' ? 'Administrador' : 'Delegado' }}</td>
            </ng-container>

            <ng-container matColumnDef="isActive">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let user">
                <mat-chip [class.chip--active]="user.isActive" [class.chip--inactive]="!user.isActive">
                  {{ user.isActive ? 'Activo' : 'Inactivo' }}
                </mat-chip>
              </td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let user">
                <button mat-icon-button matTooltip="Editar" (click)="startEdit(user)">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button matTooltip="Restablecer contraseña" (click)="startPasswordReset(user)">
                  <mat-icon>vpn_key</mat-icon>
                </button>
                @if (user.isActive) {
                  <button
                    mat-icon-button
                    matTooltip="Desactivar"
                    [disabled]="user.id === currentUserId()"
                    (click)="deactivate(user)"
                  >
                    <mat-icon>person_off</mat-icon>
                  </button>
                } @else {
                  <button mat-icon-button matTooltip="Reactivar" (click)="activate(user)">
                    <mat-icon>person</mat-icon>
                  </button>
                }
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns"></tr>
          </table>
          </div>
        </mat-card>
      }
    </div>
  `,
  styles: `
    .users-page {
      padding: 24px 32px 48px;
      max-width: 1100px;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    h1 {
      margin: 0;
      color: #111827;
    }

    .form-card {
      padding: 20px !important;
      margin-bottom: 20px;
    }

    .form-card h2 {
      margin: 0 0 12px;
      font-size: 1.05rem;
    }

    .user-form {
      display: flex;
      flex-direction: column;
      gap: 4px;
      max-width: 420px;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      margin-top: 8px;
    }

    .error-banner {
      color: #b91c1c;
      font-size: 0.875rem;
      margin: 0 0 4px;
    }

    .state-container {
      display: flex;
      justify-content: center;
      padding: 64px 0;
    }

    .table-card {
      padding: 8px !important;
    }

    .table-scroll {
      width: 100%;
      overflow-x: auto;
      -webkit-overflow-scrolling: touch;
    }

    .users-table {
      width: 100%;
      min-width: 640px;
    }

    .chip--active {
      background: #dcfce7 !important;
      color: #166534 !important;
    }

    .chip--inactive {
      background: #f3f4f6 !important;
      color: #6b7280 !important;
    }

    @media (max-width: 600px) {
      .users-page {
        padding: 16px 12px 32px;
      }

      .page-header {
        flex-direction: column;
        align-items: stretch;
        gap: 12px;
      }

      .user-form {
        max-width: none;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  readonly columns = ['fullName', 'email', 'role', 'isActive', 'actions'];

  readonly users = signal<AppUser[]>([]);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly formMode = signal<FormMode>('hidden');
  readonly resettingPasswordFor = signal<AppUser | null>(null);

  private editingUserId: string | null = null;

  readonly currentUserId = () => this.authService.currentUser()?.userId ?? null;

  readonly form = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    role: ['Delegate' as 'Admin' | 'Delegate', Validators.required],
    password: ['', [Validators.minLength(8)]],
  });

  readonly passwordForm = this.fb.nonNullable.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor() {
    this.loadUsers();
  }

  private loadUsers(): void {
    this.isLoading.set(true);
    this.userService.getAll().subscribe({
      next: (users) => {
        this.users.set(users);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  startCreate(): void {
    this.editingUserId = null;
    this.errorMessage.set(null);
    this.form.reset({ fullName: '', email: '', role: 'Delegate', password: '' });
    this.form.controls.password.addValidators(Validators.required);
    this.formMode.set('create');
  }

  startEdit(user: AppUser): void {
    this.editingUserId = user.id;
    this.errorMessage.set(null);
    this.form.controls.password.clearValidators();
    this.form.reset({ fullName: user.fullName, email: user.email, role: user.role, password: '' });
    this.formMode.set('edit');
  }

  cancelForm(): void {
    this.formMode.set('hidden');
  }

  startPasswordReset(user: AppUser): void {
    this.passwordForm.reset({ newPassword: '' });
    this.resettingPasswordFor.set(user);
  }

  cancelPasswordReset(): void {
    this.resettingPasswordFor.set(null);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    const { fullName, email, role, password } = this.form.getRawValue();

    const request$ =
      this.formMode() === 'create'
        ? this.userService.create({ fullName, email, role, password })
        : this.userService.update(this.editingUserId!, { fullName, email, role });

    request$.subscribe({
      next: () => {
        this.isSaving.set(false);
        this.formMode.set('hidden');
        this.snackBar.open('Usuario guardado.', 'Cerrar', { duration: 3000 });
        this.loadUsers();
      },
      error: (error: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(error.error?.title ?? 'No se pudo guardar el usuario.');
      },
    });
  }

  submitPasswordReset(userId: string): void {
    if (this.passwordForm.invalid) return;

    this.isSaving.set(true);
    this.userService.changePassword(userId, this.passwordForm.getRawValue().newPassword).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.resettingPasswordFor.set(null);
        this.snackBar.open('Contraseña restablecida.', 'Cerrar', { duration: 3000 });
      },
      error: () => {
        this.isSaving.set(false);
        this.snackBar.open('No se pudo restablecer la contraseña.', 'Cerrar', { duration: 3000 });
      },
    });
  }

  activate(user: AppUser): void {
    this.userService.activate(user.id).subscribe(() => this.loadUsers());
  }

  deactivate(user: AppUser): void {
    this.userService.deactivate(user.id).subscribe({
      next: () => this.loadUsers(),
      error: (error: HttpErrorResponse) => {
        this.snackBar.open(error.error?.title ?? 'No se pudo desactivar el usuario.', 'Cerrar', { duration: 4000 });
      },
    });
  }
}
