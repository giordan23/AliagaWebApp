import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PrestamosService {
  private apiUrl = `${environment.apiUrl}/prestamos`;

  constructor(private http: HttpClient) {}

  registrarPrestamo(prestamo: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/prestamo`, prestamo);
  }

  registrarAbono(abono: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/abono`, abono);
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  obtenerTodos(
    page: number = 1,
    pageSize: number = 50,
    filtros?: {
      clienteId?: number;
      fechaInicio?: string;
      fechaFin?: string;
    }
  ): Observable<any> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filtros) {
      if (filtros.clienteId) params = params.set('clienteId', filtros.clienteId.toString());
      if (filtros.fechaInicio) params = params.set('fechaInicio', filtros.fechaInicio);
      if (filtros.fechaFin) params = params.set('fechaFin', filtros.fechaFin);
    }

    return this.http.get(this.apiUrl, { params });
  }

  obtenerPorCliente(clienteId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/cliente/${clienteId}`);
  }

  obtenerPorCaja(cajaId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/caja/${cajaId}`);
  }
}
