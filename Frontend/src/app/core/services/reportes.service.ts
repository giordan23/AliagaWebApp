import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportesService {
  private apiUrl = `${environment.apiUrl}/reportes`;

  constructor(private http: HttpClient) {}

  generarReporteComprasCliente(filtros: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/compras-cliente`, filtros);
  }

  exportarReporteComprasCliente(filtros: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/compras-cliente/exportar`, filtros, {
      responseType: 'blob'
    });
  }

  generarReporteComprasProducto(filtros: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/compras-producto`, filtros);
  }

  exportarReporteComprasProducto(filtros: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/compras-producto/exportar`, filtros, {
      responseType: 'blob'
    });
  }

  generarReporteZonas(filtros: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/zonas`, filtros);
  }

  exportarReporteZonas(filtros: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/zonas/exportar`, filtros, {
      responseType: 'blob'
    });
  }

  generarReporteMovimientosCaja(filtros: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/movimientos-caja`, filtros);
  }

  exportarReporteMovimientosCaja(filtros: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/movimientos-caja/exportar`, filtros, {
      responseType: 'blob'
    });
  }

  generarReporteVentas(filtros: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/ventas`, filtros);
  }

  exportarReporteVentas(filtros: any): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/ventas/exportar`, filtros, {
      responseType: 'blob'
    });
  }

  descargarExcel(blob: Blob, nombreArchivo: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = nombreArchivo;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
