import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  obtenerResumenDelDia(): Observable<any> {
    return this.http.get(`${this.apiUrl}/resumen-dia`);
  }

  obtenerEstadoCaja(): Observable<any> {
    return this.http.get(`${this.apiUrl}/estado-caja`);
  }

  obtenerAlertas(): Observable<any> {
    return this.http.get(`${this.apiUrl}/alertas`);
  }

  obtenerEstadisticas(): Observable<any> {
    return this.http.get(`${this.apiUrl}/estadisticas`);
  }

  obtenerTopDeudores(cantidad: number = 5): Observable<any> {
    return this.http.get(`${this.apiUrl}/top-deudores`, {
      params: { cantidad: cantidad.toString() }
    });
  }
}
