import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ClientesService {
  private apiUrl = `${environment.apiUrl}/clientes`;

  constructor(private http: HttpClient) {}

  // Proveedores
  obtenerProveedores(
    page: number = 1,
    pageSize: number = 50,
    filtros?: {
      dni?: string;
      nombre?: string;
      zonaId?: number;
    }
  ): Observable<any> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filtros) {
      if (filtros.dni) params = params.set('dni', filtros.dni);
      if (filtros.nombre) params = params.set('nombre', filtros.nombre);
      if (filtros.zonaId) params = params.set('zonaId', filtros.zonaId.toString());
    }

    return this.http.get(`${this.apiUrl}/proveedores`, { params });
  }

  obtenerProveedorPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/proveedores/${id}`);
  }

  crearProveedor(proveedor: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/proveedores`, proveedor);
  }

  actualizarProveedor(id: number, proveedor: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/proveedores/${id}`, proveedor);
  }

  consultarReniec(dni: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/reniec/${dni}`);
  }

  obtenerProveedoresPorZona(zonaId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/proveedores/zona/${zonaId}`);
  }

  // Compradores
  obtenerCompradores(page: number = 1, pageSize: number = 50): Observable<any> {
    return this.http.get(`${this.apiUrl}/compradores`, {
      params: { page: page.toString(), pageSize: pageSize.toString() }
    });
  }

  obtenerCompradorPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/compradores/${id}`);
  }

  crearComprador(comprador: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/compradores`, comprador);
  }

  actualizarComprador(id: number, comprador: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/compradores/${id}`, comprador);
  }
}
