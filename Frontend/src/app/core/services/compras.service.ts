import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EditarCompraRequest {
  clienteProveedorId: number;
  detalles: DetalleCompraEditarRequest[];
}

export interface DetalleCompraEditarRequest {
  id: number;
  productoId: number;
  nivelSecado: string;
  calidad: string;
  tipoPesado: number;
  pesoBruto: number;
  descuentoKg: number;
  precioPorKg: number;
}

@Injectable({
  providedIn: 'root'
})
export class ComprasService {
  private apiUrl = `${environment.apiUrl}/compras`;

  constructor(private http: HttpClient) {}

  registrarCompra(compra: any): Observable<any> {
    return this.http.post(this.apiUrl, compra);
  }

  editarCompra(id: number, compra: EditarCompraRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, compra);
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  obtenerPorVoucher(numeroVoucher: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/voucher/${numeroVoucher}`);
  }

  obtenerTodas(
    page: number = 1,
    pageSize: number = 50,
    filtros?: {
      clienteId?: number;
      productoId?: number;
      fechaInicio?: string;
      fechaFin?: string;
    }
  ): Observable<any> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filtros) {
      if (filtros.clienteId) params = params.set('clienteId', filtros.clienteId.toString());
      if (filtros.productoId) params = params.set('productoId', filtros.productoId.toString());
      if (filtros.fechaInicio) params = params.set('fechaInicio', filtros.fechaInicio);
      if (filtros.fechaFin) params = params.set('fechaFin', filtros.fechaFin);
    }

    return this.http.get(this.apiUrl, { params });
  }

  obtenerPorCaja(cajaId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/caja/${cajaId}`);
  }

  reimprimirVoucher(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/reimprimir`, {});
  }
}
