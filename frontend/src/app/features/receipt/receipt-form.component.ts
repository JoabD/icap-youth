import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReceiptService } from '../../core/services/receipt.service';

/**
 * Formulario de emisión de recibo (CreateReceiptCommand). Placeholder
 * funcional: fuera del alcance detallado de esta entrega (Login + Vista de
 * Recibo), pero se incluye para que la navegación post-login sea completa
 * y para poder probar de punta a punta la vista de recibo.
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
              <mat-label>Cantidad de pulseras</mat-label>
              <input matInput type="number" formControlName="wristbandsQuantity" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Precio unitario</mat-label>
              <input matInput type="number" formControlName="unitPrice" />
            </mat-form-field>

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
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReceiptFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly receiptService = inject(ReceiptService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);

  readonly form = this.fb.nonNullable.group({
    delegateName: ['', Validators.required],
    areaOrRegion: ['', Validators.required],
    wristbandsQuantity: [1, [Validators.required, Validators.min(1)]],
    unitPrice: [0, [Validators.required, Validators.min(0.01)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    this.receiptService.create(this.form.getRawValue()).subscribe({
      next: (receipt) => {
        this.isLoading.set(false);
        this.router.navigate(['/admin/receipts', receipt.id]);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
