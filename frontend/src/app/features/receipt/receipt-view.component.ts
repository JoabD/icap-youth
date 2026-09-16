import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { QRCodeComponent } from 'angularx-qrcode';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ReceiptService } from '../../core/services/receipt.service';
import { AuthService } from '../../core/services/auth.service';
import { Receipt } from '../../core/models/receipt.models';

/**
 * Vista/impresión del recibo, con formato de factura: logo + certificador
 * (ICAP Juvenil - Sociedad Juvenil Amigos de Cristo), datos del delegado,
 * tabla de concepto/cantidad/precio/importe, QR de validación y firmas.
 * Es el mismo diseño que genera el backend en PDF (QuestPdfReceiptGenerator)
 * — a propósito, para que lo que el delegado ve en pantalla, imprime, y
 * recibe por correo sea siempre el mismo documento.
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
  private readonly snackBar = inject(MatSnackBar);
  protected readonly authService = inject(AuthService);

  readonly concept = 'Pre venta pulseras — Noviembre-Diciembre 2026';
  readonly certifierName = 'ICAP Juvenil - Sociedad Juvenil Amigos de Cristo';

  readonly receipt = signal<Receipt | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly isDownloading = signal(false);
  readonly isSendingEmail = signal(false);

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

  downloadPdf(): void {
    const receipt = this.receipt();
    if (!receipt) return;

    this.isDownloading.set(true);
    this.receiptService.downloadPdf(receipt.id).subscribe({
      next: (blob) => {
        this.isDownloading.set(false);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Recibo-${receipt.folioNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.isDownloading.set(false);
        this.snackBar.open('No se pudo descargar el PDF.', 'Cerrar', { duration: 4000 });
      },
    });
  }

  resendEmail(): void {
    const receipt = this.receipt();
    if (!receipt) return;

    this.isSendingEmail.set(true);
    this.receiptService.sendEmail(receipt.id).subscribe({
      next: () => {
        this.isSendingEmail.set(false);
        this.snackBar.open(`Recibo reenviado a ${receipt.delegateEmail}.`, 'Cerrar', { duration: 4000 });
      },
      error: () => {
        this.isSendingEmail.set(false);
        this.snackBar.open('No se pudo enviar el correo.', 'Cerrar', { duration: 4000 });
      },
    });
  }
}
