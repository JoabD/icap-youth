/** Espejo de ReceiptDto (Icap.Application.Receipts.DTOs). */
export interface Receipt {
  id: string;
  folioNumber: string;
  delegateName: string;
  areaOrRegion: string;
  wristbandsQuantity: number;
  unitPrice: number;
  totalCost: number;
  currency: string;
  qrHash: string;
  status: 'Issued' | 'Cancelled';
  createdAt: string;
  createdBy: string;
}

/** Espejo de CreateReceiptCommand (sin CreatedByUserId: lo agrega el interceptor/backend desde el JWT). */
export interface CreateReceiptRequest {
  delegateName: string;
  areaOrRegion: string;
  wristbandsQuantity: number;
  unitPrice: number;
}
