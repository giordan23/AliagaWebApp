import { Component } from '@angular/core';

@Component({
  selector: 'app-clientes',
  standalone: true,
  template: `<div class="container"><h1>Gestión de Clientes</h1><div class="card"><p>Módulo en desarrollo</p></div></div>`,
  styles: [`.container { padding: 20px; } .card { background: white; padding: 20px; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); } h1 { color: #2c3e50; }`]
})
export class ClientesComponent {}
