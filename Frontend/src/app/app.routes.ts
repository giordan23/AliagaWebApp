import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  {
    path: 'caja',
    loadComponent: () => import('./features/caja/caja.component').then(m => m.CajaComponent)
  },
  {
    path: 'compras',
    loadComponent: () => import('./features/compras/compras.component').then(m => m.ComprasComponent)
  },
  {
    path: 'ventas',
    loadComponent: () => import('./features/ventas/ventas.component').then(m => m.VentasComponent)
  },
  {
    path: 'clientes',
    loadComponent: () => import('./features/clientes/clientes.component').then(m => m.ClientesComponent)
  },
  {
    path: 'prestamos',
    loadComponent: () => import('./features/prestamos/prestamos.component').then(m => m.PrestamosComponent)
  },
  {
    path: 'productos',
    loadComponent: () => import('./features/productos/productos.component').then(m => m.ProductosComponent)
  },
  {
    path: 'zonas',
    loadComponent: () => import('./features/zonas/zonas.component').then(m => m.ZonasComponent)
  },
  {
    path: 'reportes',
    loadComponent: () => import('./features/reportes/reportes.component').then(m => m.ReportesComponent)
  },
  {
    path: 'configuracion',
    loadComponent: () => import('./features/configuracion/configuracion.component').then(m => m.ConfiguracionComponent)
  }
];