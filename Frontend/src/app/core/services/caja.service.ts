import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CajaService {
  private apiUrl = `${environment.apiUrl}/caja`;

  constructor(private http: HttpClient) {}

  abrirCaja(montoInicial: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/abrir`, { montoInicial });
  }

  cerrarCaja(arqueoReal: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/cerrar`, { arqueoReal });
  }

  reabrirCaja(): Observable<any> {
    return this.http.post(`${this.apiUrl}/reabrir`, {});
  }

  obtenerCajaActual(): Observable<any> {
    return this.http.get(`${this.apiUrl}/actual`);
  }

  obtenerUltimaCajaCerrada(): Observable<any> {
    return this.http.get(`${this.apiUrl}/ultima-cerrada`);
  }

  obtenerHistorial(page: number = 1, pageSize: number = 50): Observable<any> {
    return this.http.get(`${this.apiUrl}/historial`, {
      params: { page: page.toString(), pageSize: pageSize.toString() }
    });
  }

  obtenerDetalleCaja(cajaId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${cajaId}`);
  }

  registrarMovimiento(tipoMovimiento: number, monto: number, concepto: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/movimiento`, {
      tipoMovimiento,
      monto,
      descripcion: concepto
    });
  }
}
