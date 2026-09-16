import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateReceiptRequest, Receipt } from '../models/receipt.models';

@Injectable({ providedIn: 'root' })
export class ReceiptService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/receipts`;

  getById(id: string): Observable<Receipt> {
    return this.http.get<Receipt>(`${this.baseUrl}/${id}`);
  }

  getAll(): Observable<Receipt[]> {
    return this.http.get<Receipt[]>(this.baseUrl);
  }

  create(request: CreateReceiptRequest): Observable<Receipt> {
    return this.http.post<Receipt>(this.baseUrl, request);
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cancel`, {});
  }

  /** Descarga el PDF del recibo como Blob (el componente se encarga de disparar la descarga en el navegador). */
  downloadPdf(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/pdf`, { responseType: 'blob' });
  }

  /** Reenvía el PDF del recibo al correo del delegado (con copia administrativa, ver backend EmailSettings). */
  sendEmail(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/send-email`, {});
  }
}
