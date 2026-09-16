import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReceiptService } from '../../core/services/receipt.service';
import { Receipt } from '../../core/models/receipt.models';

interface AreaSummary {
  areaOrRegion: string;
  receiptsCount: number;
  wristbandsSold: number;
  totalCollected: number;
}

/**
 * Resumen financiero calculado en el cliente a partir de GetReceipts — no
 * se agregó un endpoint nuevo en el backend a propósito: los mismos datos
 * que ya expone /api/v1/receipts alcanzan para este primer tablero. Si más
 * adelante se necesitan reportes históricos pesados (por rango de fechas,
 * exportables, etc.) ahí sí conviene mover el cálculo a Application con
 * una Query dedicada — hoy sería sobre-ingeniería.
 */
@Component({
  selector: 'icap-finance',
  standalone: true,
  imports: [CurrencyPipe, MatCardModule, MatTableModule, MatProgressSpinnerModule],
  template: `
    <div class="finance-page">
      <h1>Finanzas</h1>

      @if (isLoading()) {
        <div class="state-container">
          <mat-spinner diameter="36" />
        </div>
      } @else {
        <div class="stat-grid">
          <mat-card class="stat-card">
            <span class="stat-label">Total recaudado</span>
            <span class="stat-value">{{ totalCollected() | currency: 'MXN' }}</span>
          </mat-card>
          <mat-card class="stat-card">
            <span class="stat-label">Pulseras vendidas</span>
            <span class="stat-value">{{ totalWristbands() }}</span>
          </mat-card>
          <mat-card class="stat-card">
            <span class="stat-label">Recibos vigentes</span>
            <span class="stat-value">{{ issuedCount() }}</span>
          </mat-card>
          <mat-card class="stat-card stat-card--warn">
            <span class="stat-label">Recibos cancelados</span>
            <span class="stat-value">{{ cancelledCount() }}</span>
          </mat-card>
        </div>

        <mat-card class="table-card">
          <h2>Desglose por área / región</h2>
          <div class="table-scroll">
          <table mat-table [dataSource]="areaSummaries()" class="area-table">
            <ng-container matColumnDef="areaOrRegion">
              <th mat-header-cell *matHeaderCellDef>Área / Región</th>
              <td mat-cell *matCellDef="let row">{{ row.areaOrRegion }}</td>
            </ng-container>
            <ng-container matColumnDef="receiptsCount">
              <th mat-header-cell *matHeaderCellDef>Recibos</th>
              <td mat-cell *matCellDef="let row">{{ row.receiptsCount }}</td>
            </ng-container>
            <ng-container matColumnDef="wristbandsSold">
              <th mat-header-cell *matHeaderCellDef>Pulseras</th>
              <td mat-cell *matCellDef="let row">{{ row.wristbandsSold }}</td>
            </ng-container>
            <ng-container matColumnDef="totalCollected">
              <th mat-header-cell *matHeaderCellDef>Total</th>
              <td mat-cell *matCellDef="let row">{{ row.totalCollected | currency: 'MXN' }}</td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns"></tr>
          </table>
          </div>

          @if (areaSummaries().length === 0) {
            <p class="empty-state">Aún no hay recibos emitidos.</p>
          }
        </mat-card>
      }
    </div>
  `,
  styles: `
    .finance-page {
      padding: 24px 32px 48px;
      max-width: 1100px;
    }

    h1 {
      margin: 0 0 20px;
      color: #111827;
    }

    .state-container {
      display: flex;
      justify-content: center;
      padding: 64px 0;
    }

    .stat-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      display: flex;
      flex-direction: column;
      gap: 6px;
      padding: 18px 20px !important;
    }

    .stat-card--warn .stat-value {
      color: #b91c1c;
    }

    .stat-label {
      font-size: 0.8rem;
      color: #6b7280;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }

    .stat-value {
      font-size: 1.6rem;
      font-weight: 700;
      color: #111827;
    }

    .table-card {
      padding: 20px !important;
    }

    .table-card h2 {
      margin: 0 0 12px;
      font-size: 1.05rem;
      color: #111827;
    }

    .table-scroll {
      width: 100%;
      overflow-x: auto;
      -webkit-overflow-scrolling: touch;
    }

    .area-table {
      width: 100%;
      min-width: 480px;
    }

    .empty-state {
      color: #6b7280;
      padding: 16px 0 0;
    }

    @media (max-width: 600px) {
      .finance-page {
        padding: 16px 12px 32px;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FinanceComponent {
  private readonly receiptService = inject(ReceiptService);

  readonly columns = ['areaOrRegion', 'receiptsCount', 'wristbandsSold', 'totalCollected'];

  readonly isLoading = signal(true);
  private readonly receipts = signal<Receipt[]>([]);

  readonly issuedReceipts = computed(() => this.receipts().filter((r) => r.status === 'Issued'));
  readonly issuedCount = computed(() => this.issuedReceipts().length);
  readonly cancelledCount = computed(() => this.receipts().filter((r) => r.status === 'Cancelled').length);
  readonly totalCollected = computed(() => this.issuedReceipts().reduce((sum, r) => sum + r.totalCost, 0));
  readonly totalWristbands = computed(() => this.issuedReceipts().reduce((sum, r) => sum + r.wristbandsQuantity, 0));

  readonly areaSummaries = computed<AreaSummary[]>(() => {
    const byArea = new Map<string, AreaSummary>();

    for (const receipt of this.issuedReceipts()) {
      const existing = byArea.get(receipt.areaOrRegion);

      if (existing) {
        existing.receiptsCount += 1;
        existing.wristbandsSold += receipt.wristbandsQuantity;
        existing.totalCollected += receipt.totalCost;
      } else {
        byArea.set(receipt.areaOrRegion, {
          areaOrRegion: receipt.areaOrRegion,
          receiptsCount: 1,
          wristbandsSold: receipt.wristbandsQuantity,
          totalCollected: receipt.totalCost,
        });
      }
    }

    return [...byArea.values()].sort((a, b) => b.totalCollected - a.totalCollected);
  });

  constructor() {
    this.receiptService.getAll().subscribe({
      next: (receipts) => {
        this.receipts.set(receipts);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
