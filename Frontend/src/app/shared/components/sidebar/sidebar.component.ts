import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2>Comercial Aliaga</h2>
      </div>

      <nav class="sidebar-nav">
        <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">
          <span>📊</span> Dashboard
        </a>

        <a routerLink="/caja" routerLinkActive="active" class="nav-item">
          <span>💰</span> Caja
        </a>

        <a routerLink="/compras" routerLinkActive="active" class="nav-item">
          <span>📦</span> Compras
        </a>

        <a routerLink="/ventas" routerLinkActive="active" class="nav-item">
          <span>🛒</span> Ventas
        </a>

        <a routerLink="/clientes" routerLinkActive="active" class="nav-item">
          <span>👥</span> Clientes
        </a>

        <a routerLink="/prestamos" routerLinkActive="active" class="nav-item">
          <span>💵</span> Préstamos
        </a>

        <a routerLink="/productos" routerLinkActive="active" class="nav-item">
          <span>🌾</span> Productos
        </a>

        <a routerLink="/zonas" routerLinkActive="active" class="nav-item">
          <span>📍</span> Zonas
        </a>

        <a routerLink="/reportes" routerLinkActive="active" class="nav-item">
          <span>📈</span> Reportes
        </a>

        <a routerLink="/configuracion" routerLinkActive="active" class="nav-item">
          <span>⚙️</span> Configuración
        </a>
      </nav>
    </aside>
  `,
  styles: [`
    .sidebar {
      width: 250px;
      height: 100vh;
      background: #2c3e50;
      color: white;
      position: fixed;
      left: 0;
      top: 0;
      overflow-y: auto;
    }

    .sidebar-header {
      padding: 20px;
      background: #34495e;
      border-bottom: 1px solid #4a5f7f;
    }

    .sidebar-header h2 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
    }

    .sidebar-nav {
      padding: 10px 0;
    }

    .nav-item {
      display: flex;
      align-items: center;
      padding: 12px 20px;
      color: #ecf0f1;
      text-decoration: none;
      transition: background 0.3s;
      cursor: pointer;
    }

    .nav-item:hover {
      background: #34495e;
    }

    .nav-item.active {
      background: #3498db;
      border-left: 4px solid #2980b9;
    }

    .nav-item span {
      margin-right: 10px;
      font-size: 18px;
    }
  `]
})
export class SidebarComponent {}
