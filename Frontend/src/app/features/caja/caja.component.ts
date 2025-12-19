import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CajaService } from '../../core/services/caja.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';

@Component({
  selector: 'app-caja',
  standalone: true,
  imports: [CommonModule, FormatoMonedaPipe, FormatoFechaPipe],
  template: `
    <div class="container">
      <h1>Gestión de Caja</h1>

      <div class="card" *ngIf="cajaActual">
        <h2>Caja Actual</h2>
        <div *ngIf="cajaActual.id">
          <p><strong>Fecha:</strong> {{ cajaActual.fecha | formatoFecha }}</p>
          <p><strong>Estado:</strong> {{ cajaActual.estado }}</p>
          <p><strong>Monto Inicial:</strong> {{ cajaActual.montoInicial | formatoMoneda }}</p>
          <p><strong>Monto Esperado:</strong> {{ cajaActual.montoEsperado | formatoMoneda }}</p>
        </div>
        <p *ngIf="!cajaActual.id">No hay caja abierta</p>
      </div>

      <div class="actions">
        <button class="btn btn-primary" (click)="abrirCaja()" *ngIf="!cajaActual?.id">
          Abrir Caja
        </button>
        <button class="btn btn-danger" (click)="cerrarCaja()" *ngIf="cajaActual?.id">
          Cerrar Caja
        </button>
      </div>

      <p class="info-text">💡 Módulo funcional básico. Expandir según necesidad.</p>
    </div>
  `,
  styles: [`
    .container { padding: 20px; }
    .card { background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    h1 { color: #2c3e50; margin-bottom: 20px; }
    h2 { color: #34495e; font-size: 18px; margin-bottom: 15px; border-bottom: 2px solid #3498db; padding-bottom: 8px; }
    .actions { margin: 20px 0; }
    .btn { padding: 12px 24px; border: none; border-radius: 4px; cursor: pointer; font-weight: 600; margin-right: 10px; }
    .btn-primary { background: #3498db; color: white; }
    .btn-danger { background: #e74c3c; color: white; }
    .btn:hover { opacity: 0.9; }
    .info-text { color: #7f8c8d; font-style: italic; margin-top: 20px; }
  `]
})
export class CajaComponent implements OnInit {
  cajaActual: any = null;

  constructor(private cajaService: CajaService) {}

  ngOnInit(): void {
    this.cargarCajaActual();
  }

  cargarCajaActual(): void {
    this.cajaService.obtenerCajaActual().subscribe({
      next: (data) => {
        this.cajaActual = data;
      },
      error: (error) => {
        console.error('Error al cargar caja:', error);
        this.cajaActual = {};
      }
    });
  }

  abrirCaja(): void {
    const montoInicial = prompt('Ingrese el monto inicial:');
    if (montoInicial) {
      this.cajaService.abrirCaja(parseFloat(montoInicial)).subscribe({
        next: () => {
          alert('Caja abierta exitosamente');
          this.cargarCajaActual();
        },
        error: (error) => {
          alert('Error: ' + error.message);
        }
      });
    }
  }

  cerrarCaja(): void {
    const arqueoReal = prompt('Ingrese el arqueo real:');
    if (arqueoReal) {
      this.cajaService.cerrarCaja(parseFloat(arqueoReal)).subscribe({
        next: () => {
          alert('Caja cerrada exitosamente');
          this.cargarCajaActual();
        },
        error: (error) => {
          alert('Error: ' + error.message);
        }
      });
    }
  }
}
