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
    skip: number = 0,
    take: number = 1000,
    search?: string,
    zonaId?: number
  ): Observable<any> {
    let params = new HttpParams()
      .set('skip', skip.toString())
      .set('take', take.toString());

    if (search) {
      params = params.set('search', search);
    }
    if (zonaId) {
      params = params.set('zonaId', zonaId.toString());
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
  obtenerCompradores(): Observable<any> {
    return this.http.get(`${this.apiUrl}/compradores`);
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
