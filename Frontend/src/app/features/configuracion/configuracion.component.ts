import { Component } from '@angular/core';
import { ConfiguracionService } from '../../core/services/configuracion.service';

@Component({
  selector: 'app-configuracion',
  standalone: true,
  template: `
    <div class="container">
      <h1>Configuración</h1>
      <div class="card">
        <h2>Backup de Base de Datos</h2>
        <button class="btn" (click)="descargarBackup()">💾 Descargar Backup</button>
        <p class="info-text">Descarga una copia completa de la base de datos SQLite</p>
      </div>
    </div>
  `,
  styles: [`
    .container { padding: 20px; }
    .card { background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    h1, h2 { color: #2c3e50; }
    .btn { padding: 12px 24px; background: #3498db; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: 600; }
    .btn:hover { opacity: 0.9; }
    .info-text { color: #7f8c8d; margin-top: 10px; }
  `]
})
export class ConfiguracionComponent {
  constructor(private configuracionService: ConfiguracionService) {}

  descargarBackup(): void {
    this.configuracionService.ejecutarBackup();
  }
}
