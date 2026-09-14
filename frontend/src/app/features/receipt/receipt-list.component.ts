import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';

import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ReceiptService } from '../../core/services/receipt.service';
import { AuthService } from '../../core/services/auth.service';
import { Receipt } from '../../core/models/receipt.models';

/**
 * Tabla de registro de recibos (Angular Material) mencionada en el
 * Documento de Diseño. Un Admin ve todos los recibos; un Delegate solo ve
 * los suyos — esa restricción ya la aplica ReceiptsController en el backend
 * (GET /api/v1/receipts), este componente solo pinta lo que recibe.
 */
@Component({
  selector: 'icap-receipt-list',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    CurrencyPipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './receipt-list.component.html',
  styleUrl: './receipt-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReceiptListComponent {
  private readonly receiptService = inject(ReceiptService);
  private readonly authService = inject(AuthService);

  readonly isAdmin = this.authService.isAdmin;

  readonly displayedColumns = [
    'folioNumber',
    'delegateName',
    'areaOrRegion',
    'wristbandsQuantity',
    'totalCost',
    'status',
    'createdAt',
    'actions',
  ];

  readonly receipts = signal<Receipt[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly hasReceipts = computed(() => this.receipts().length > 0);

  constructor() {
    this.receiptService.getAll().subscribe({
      next: (receipts) => {
        this.receipts.set(receipts);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('No se pudo cargar el listado de recibos.');
      },
    });
  }
}
