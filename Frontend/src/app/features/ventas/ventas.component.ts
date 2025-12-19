import { Component } from '@angular/core';

@Component({
  selector: 'app-ventas',
  standalone: true,
  template: `
    <div class="container">
      <h1>Gestión de Ventas</h1>
      <div class="card">
        <p>Módulo en desarrollo</p>
        <p class="info-text">💡 Funcionalidad similar a Compras. Implementar según necesidad.</p>
      </div>
    </div>
  `,
  styles: [`
    .container { padding: 20px; }
    .card { background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    h1 { color: #2c3e50; }
    .info-text { color: #7f8c8d; font-style: italic; }
  `]
})
export class VentasComponent {}
