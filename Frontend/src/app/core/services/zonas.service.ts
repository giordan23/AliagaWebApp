import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ZonasService {
  private apiUrl = `${environment.apiUrl}/zonas`;

  constructor(private http: HttpClient) {}

  obtenerTodas(page: number = 1, pageSize: number = 50): Observable<any> {
    return this.http.get(this.apiUrl, {
      params: { page: page.toString(), pageSize: pageSize.toString() }
    });
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  crear(zona: any): Observable<any> {
    return this.http.post(this.apiUrl, zona);
  }

  actualizar(id: number, zona: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, zona);
  }

  obtenerClientesPorZona(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/clientes`);
  }
}
