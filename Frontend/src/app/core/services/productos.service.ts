import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductosService {
  private apiUrl = `${environment.apiUrl}/productos`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<any> {
    return this.http.get(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  actualizarPrecio(id: number, precioSugeridoPorKg: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/precio`, { precioSugeridoPorKg });
  }
}
