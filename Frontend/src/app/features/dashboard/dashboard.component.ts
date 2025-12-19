import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../core/services/dashboard.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormatoMonedaPipe, FormatoFechaPipe],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  resumenDia: any = null;
  estadoCaja: any = null;
  alertas: any[] = [];
  estadisticas: any = null;
  topDeudores: any[] = [];
  loading = true;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.loading = true;

    // Cargar resumen del día
    this.dashboardService.obtenerResumenDelDia().subscribe({
      next: (data) => {
        this.resumenDia = data;
      },
      error: (error) => {
        console.error('Error al cargar resumen del día:', error);
      }
    });

    // Cargar estado de caja
    this.dashboardService.obtenerEstadoCaja().subscribe({
      next: (data) => {
        this.estadoCaja = data;
      },
      error: (error) => {
        console.error('Error al cargar estado de caja:', error);
      }
    });

    // Cargar alertas
    this.dashboardService.obtenerAlertas().subscribe({
      next: (data) => {
        this.alertas = data;
      },
      error: (error) => {
        console.error('Error al cargar alertas:', error);
      }
    });

    // Cargar estadísticas
    this.dashboardService.obtenerEstadisticas().subscribe({
      next: (data) => {
        this.estadisticas = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar estadísticas:', error);
        this.loading = false;
      }
    });

    // Cargar top deudores
    this.dashboardService.obtenerTopDeudores(5).subscribe({
      next: (data) => {
        this.topDeudores = data;
      },
      error: (error) => {
        console.error('Error al cargar top deudores:', error);
      }
    });
  }

  getPrioridadClass(prioridad: string): string {
    switch (prioridad) {
      case 'alta': return 'alert-danger';
      case 'media': return 'alert-warning';
      default: return 'alert-info';
    }
  }
}
