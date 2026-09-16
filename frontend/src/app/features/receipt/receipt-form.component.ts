import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReceiptService } from '../../core/services/receipt.service';

/**
 * Formulario de emisión de recibo (CreateReceiptCommand). El correo del
 * delegado es obligatorio (no solo un "nice to have"): es el destino real
 * del PDF que se envía automáticamente al emitir (ver
 * CreateReceiptCommandHandler en el backend).
 */
@Component({
  selector: 'icap-receipt-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="form-page">
      <mat-card class="form-card">
        <mat-card-header>
          <mat-card-title>Nuevo recibo</mat-card-title>
          <mat-card-subtitle>Pre venta pulseras — Noviembre-Diciembre 2026</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()" class="receipt-form">
            <mat-form-field appearance="outline">
              <mat-label>Nombre del delegado</mat-label>
              <input matInput formControlName="delegateName" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Área o región</mat-label>
              <input matInput formControlName="areaOrRegion" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Correo del delegado</mat-label>
              <input matInput type="email" formControlName="delegateEmail" />
              <mat-hint>El recibo en PDF se enviará automáticamente a este correo.</mat-hint>
              @if (form.controls.delegateEmail.hasError('email') && form.controls.delegateEmail.touched) {
                <mat-error>Ingresa un correo válido.</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Cantidad de pulseras</mat-label>
              <input matInput type="number" formControlName="wristbandsQuantity" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Precio unitario</mat-label>
              <input matInput type="number" formControlName="unitPrice" />
            </mat-form-field>

            @if (errorMessage()) {
              <p class="error-banner" role="alert">{{ errorMessage() }}</p>
            }

            <button mat-flat-button color="primary" type="submit" [disabled]="isLoading()">
              @if (isLoading()) {
                <mat-spinner diameter="20" />
              } @else {
                <span>Emitir recibo</span>
              }
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: `
    .form-page { display: flex; justify-content: center; padding: 32px 16px; }
    .form-card { width: 100%; max-width: 480px; }
    .receipt-form { display: flex; flex-direction: column; gap: 4px; }
    .error-banner { color: #b91c1c; font-size: 0.875rem; margin: 4px 0; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReceiptFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly receiptService = inject(ReceiptService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    delegateName: ['', Validators.required],
    areaOrRegion: ['', Validators.required],
    delegateEmail: ['', [Validators.required, Validators.email]],
    wristbandsQuantity: [1, [Validators.required, Validators.min(1)]],
    unitPrice: [0, [Validators.required, Validators.min(0.01)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.receiptService.create(this.form.getRawValue()).subscribe({
      next: (receipt) => {
        this.isLoading.set(false);
        this.router.navigate(['/admin/receipts', receipt.id]);
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.title ?? 'No se pudo emitir el recibo.');
      },
    });
  }
}
