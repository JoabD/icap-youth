import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { QRCodeComponent } from 'angularx-qrcode';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReceiptService } from '../../core/services/receipt.service';
import { Receipt } from '../../core/models/receipt.models';

type CardCopy = 'Original' | 'Copia';

/**
 * Vista/impresión del recibo. Renderiza DOS tarjetas idénticas (Original y
 * Copia) en pantalla, con un QR por tarjeta generado a partir del QRHash
 * del backend. La hoja de estilos @media print oculta todo lo que no sea
 * las tarjetas (nav, botones) al invocar window.print(), dejando listo el
 * espacio de firmas para "ICAP Juvenil".
 */
@Component({
  selector: 'icap-receipt-view',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    CurrencyPipe,
    QRCodeComponent,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './receipt-view.component.html',
  styleUrl: './receipt-view.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReceiptViewComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly receiptService = inject(ReceiptService);

  readonly copies: CardCopy[] = ['Original', 'Copia'];

  readonly receipt = signal<Receipt | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly isCancelled = computed(() => this.receipt()?.status === 'Cancelled');

  constructor() {
    const receiptId = this.route.snapshot.paramMap.get('id');

    if (!receiptId) {
      this.isLoading.set(false);
      this.errorMessage.set('No se especificó un recibo válido.');
      return;
    }

    this.receiptService.getById(receiptId).subscribe({
      next: (receipt) => {
        this.receipt.set(receipt);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('No se pudo cargar el recibo solicitado.');
      },
    });
  }

  print(): void {
    window.print();
  }
}
