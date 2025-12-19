import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ComprasService } from '../../core/services/compras.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';
import { NumeroVoucherPipe } from '../../shared/pipes/numero-voucher.pipe';

@Component({
  selector: 'app-compras',
  standalone: true,
  imports: [CommonModule, FormatoMonedaPipe, FormatoFechaPipe, NumeroVoucherPipe],
  template: `
    <div class="container">
      <h1>Gestión de Compras</h1>

      <div class="card">
        <h2>Últimas Compras</h2>
        <table *ngIf="compras.length > 0">
          <thead>
            <tr>
              <th>Voucher</th>
              <th>Cliente</th>
              <th>Producto</th>
              <th>Peso Neto</th>
              <th>Monto Total</th>
              <th>Fecha</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let compra of compras">
              <td>{{ compra.numeroVoucher | numeroVoucher }}</td>
              <td>{{ compra.clienteNombre }}</td>
              <td>{{ compra.productoNombre }}</td>
              <td>{{ compra.pesoNeto }} kg</td>
              <td>{{ compra.montoTotal | formatoMoneda }}</td>
              <td>{{ compra.fechaCompra | formatoFecha:true }}</td>
            </tr>
          </tbody>
        </table>
        <p *ngIf="compras.length === 0">No hay compras registradas</p>
      </div>

      <p class="info-text">💡 Módulo básico funcional. Para registrar compras, expandir formulario.</p>
    </div>
  `,
  styles: [`
    .container { padding: 20px; max-width: 1200px; }
    .card { background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    h1 { color: #2c3e50; }
    h2 { color: #34495e; font-size: 18px; margin-bottom: 15px; border-bottom: 2px solid #3498db; padding-bottom: 8px; }
    table { width: 100%; border-collapse: collapse; }
    th { background: #34495e; color: white; padding: 12px; text-align: left; }
    td { padding: 12px; border-bottom: 1px solid #ecf0f1; }
    tr:hover { background: #f8f9fa; }
    .info-text { color: #7f8c8d; font-style: italic; margin-top: 20px; }
  `]
})
export class ComprasComponent implements OnInit {
  compras: any[] = [];

  constructor(private comprasService: ComprasService) {}

  ngOnInit(): void {
    this.cargarCompras();
  }

  cargarCompras(): void {
    this.comprasService.obtenerTodas(1, 10).subscribe({
      next: (response) => {
        this.compras = response.data || [];
      },
      error: (error) => {
        console.error('Error al cargar compras:', error);
      }
    });
  }
}
