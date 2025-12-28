import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportesService } from '../../core/services/reportes.service';
import { ClientesService } from '../../core/services/clientes.service';
import { ProductosService } from '../../core/services/productos.service';
import { ZonasService } from '../../core/services/zonas.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';
import {
  FiltrosReporte,
  ReporteComprasCliente,
  ReporteComprasProducto,
  ReporteZonas,
  ReporteMovimientosCaja,
  ReporteVentas,
  TotalesReporte
} from '../../core/models/reporte.model';
import { ClienteProveedor } from '../../core/models/cliente-proveedor.model';
import { ClienteComprador } from '../../core/models/cliente-comprador.model';
import { Producto } from '../../core/models/producto.model';
import { Zona } from '../../core/models/zona.model';
import { TipoMovimiento, TipoOperacion } from '../../core/models/enums';

@Component({
  selector: 'app-reportes',
  standalone: true,
  imports: [CommonModule, FormsModule, FormatoMonedaPipe, FormatoFechaPipe],
  templateUrl: './reportes.component.html',
  styleUrls: ['./reportes.component.css']
})
export class ReportesComponent implements OnInit {
  // Tab activa
  tabActiva: string = 'compras-cliente';

  // Datos para filtros
  productos: Producto[] = [];
  zonas: Zona[] = [];
  clientesProveedores: ClienteProveedor[] = [];
  clientesCompradores: ClienteComprador[] = [];

  // Filtros
  filtros: FiltrosReporte = {
    fechaInicio: this.obtenerPrimerDiaDelMes(),
    fechaFin: new Date().toISOString().split('T')[0]
  };

  // Opción de agrupar para movimientos de caja
  agruparPorTipo: boolean = false;

  // Datos de reportes
  reporteComprasCliente: ReporteComprasCliente[] = [];
  reporteComprasProducto: ReporteComprasProducto[] = [];
  reporteZonas: ReporteZonas[] = [];
  reporteMovimientosCaja: ReporteMovimientosCaja[] = [];
  reporteMovimientosCajaAgrupado: any[] = [];
  reporteVentas: ReporteVentas[] = [];

  // Totales
  totales: TotalesReporte = {};

  // Estado de carga
  cargando: boolean = false;
  mostrandoResultados: boolean = false;

  // Enums para el template
  TipoMovimiento = TipoMovimiento;
  TipoOperacion = TipoOperacion;

  constructor(
    private reportesService: ReportesService,
    private clientesService: ClientesService,
    private productosService: ProductosService,
    private zonasService: ZonasService
  ) {}

  ngOnInit(): void {
    this.cargarDatosParaFiltros();
  }

  cargarDatosParaFiltros(): void {
    // Cargar productos
    this.productosService.obtenerTodos().subscribe({
      next: (productos) => this.productos = productos,
      error: (error) => console.error('Error al cargar productos:', error)
    });

    // Cargar zonas
    this.zonasService.obtenerTodas(0, 1000).subscribe({
      next: (response) => this.zonas = response.items || [],
      error: (error) => console.error('Error al cargar zonas:', error)
    });

    // Cargar clientes proveedores (para filtro opcional)
    this.clientesService.obtenerProveedores(0, 1000).subscribe({
      next: (response) => this.clientesProveedores = response.data || [],
      error: (error) => console.error('Error al cargar clientes proveedores:', error)
    });

    // Cargar clientes compradores
    this.clientesService.obtenerCompradores().subscribe({
      next: (compradores) => this.clientesCompradores = compradores || [],
      error: (error) => console.error('Error al cargar clientes compradores:', error)
    });
  }

  cambiarTab(tab: string): void {
    this.tabActiva = tab;
    this.limpiarFiltros();
    this.mostrandoResultados = false;
  }

  limpiarFiltros(): void {
    this.filtros = {
      fechaInicio: this.obtenerPrimerDiaDelMes(),
      fechaFin: new Date().toISOString().split('T')[0]
    };
    this.agruparPorTipo = false;
  }

  obtenerPrimerDiaDelMes(): string {
    const fecha = new Date();
    return new Date(fecha.getFullYear(), fecha.getMonth(), 1).toISOString().split('T')[0];
  }

  validarFiltros(): boolean {
    if (!this.filtros.fechaInicio || !this.filtros.fechaFin) {
      alert('Las fechas de inicio y fin son obligatorias');
      return false;
    }

    const inicio = new Date(this.filtros.fechaInicio);
    const fin = new Date(this.filtros.fechaFin);

    if (inicio > fin) {
      alert('La fecha de inicio no puede ser mayor a la fecha fin');
      return false;
    }

    return true;
  }

  generarReporte(): void {
    if (!this.validarFiltros()) return;

    this.cargando = true;
    this.totales = {};

    switch (this.tabActiva) {
      case 'compras-cliente':
        this.generarReporteComprasCliente();
        break;
      case 'compras-producto':
        this.generarReporteComprasProducto();
        break;
      case 'zonas':
        this.generarReporteZonas();
        break;
      case 'movimientos-caja':
        this.generarReporteMovimientosCaja();
        break;
      case 'ventas':
        this.generarReporteVentas();
        break;
    }
  }

  generarReporteComprasCliente(): void {
    this.reportesService.generarReporteComprasCliente(this.filtros).subscribe({
      next: (data: ReporteComprasCliente[]) => {
        this.reporteComprasCliente = data;
        this.calcularTotalesComprasCliente();
        this.mostrandoResultados = true;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al generar reporte:', error);
        alert('Error al generar el reporte');
        this.cargando = false;
      }
    });
  }

  generarReporteComprasProducto(): void {
    this.reportesService.generarReporteComprasProducto(this.filtros).subscribe({
      next: (data: ReporteComprasProducto[]) => {
        this.reporteComprasProducto = data;
        this.calcularTotalesComprasProducto();
        this.mostrandoResultados = true;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al generar reporte:', error);
        alert('Error al generar el reporte');
        this.cargando = false;
      }
    });
  }

  generarReporteZonas(): void {
    this.reportesService.generarReporteZonas(this.filtros).subscribe({
      next: (data: ReporteZonas[]) => {
        this.reporteZonas = data;
        this.calcularTotalesZonas();
        this.mostrandoResultados = true;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al generar reporte:', error);
        alert('Error al generar el reporte');
        this.cargando = false;
      }
    });
  }

  generarReporteMovimientosCaja(): void {
    this.reportesService.generarReporteMovimientosCaja(this.filtros).subscribe({
      next: (data: ReporteMovimientosCaja[]) => {
        this.reporteMovimientosCaja = data;

        if (this.agruparPorTipo) {
          this.agruparMovimientosPorTipo();
        }

        this.calcularTotalesMovimientosCaja();
        this.mostrandoResultados = true;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al generar reporte:', error);
        alert('Error al generar el reporte');
        this.cargando = false;
      }
    });
  }

  generarReporteVentas(): void {
    this.reportesService.generarReporteVentas(this.filtros).subscribe({
      next: (data: ReporteVentas[]) => {
        this.reporteVentas = data;
        this.calcularTotalesVentas();
        this.mostrandoResultados = true;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al generar reporte:', error);
        alert('Error al generar el reporte');
        this.cargando = false;
      }
    });
  }

  agruparMovimientosPorTipo(): void {
    const grupos: { [key: string]: ReporteMovimientosCaja[] } = {};

    this.reporteMovimientosCaja.forEach(mov => {
      const tipo = this.obtenerNombreTipoMovimiento(mov.tipoMovimiento);
      if (!grupos[tipo]) {
        grupos[tipo] = [];
      }
      grupos[tipo].push(mov);
    });

    this.reporteMovimientosCajaAgrupado = Object.keys(grupos).map(tipo => ({
      tipo,
      movimientos: grupos[tipo],
      totalIngresos: grupos[tipo]
        .filter(m => m.tipoOperacion === TipoOperacion.Ingreso)
        .reduce((sum, m) => sum + m.monto, 0),
      totalEgresos: grupos[tipo]
        .filter(m => m.tipoOperacion === TipoOperacion.Egreso)
        .reduce((sum, m) => sum + m.monto, 0)
    }));
  }

  calcularTotalesComprasCliente(): void {
    this.totales = {
      totalRegistros: this.reporteComprasCliente.length,
      totalKg: this.reporteComprasCliente.reduce((sum, item) => sum + item.pesoTotalKg, 0),
      totalMonto: this.reporteComprasCliente.reduce((sum, item) => sum + item.montoTotal, 0)
    };
  }

  calcularTotalesComprasProducto(): void {
    this.totales = {
      totalRegistros: this.reporteComprasProducto.length,
      totalKg: this.reporteComprasProducto.reduce((sum, item) => sum + item.pesoTotalKg, 0),
      totalMonto: this.reporteComprasProducto.reduce((sum, item) => sum + item.montoTotal, 0)
    };
  }

  calcularTotalesZonas(): void {
    this.totales = {
      totalRegistros: this.reporteZonas.length,
      totalKg: this.reporteZonas.reduce((sum, item) => sum + item.pesoTotalKg, 0),
      totalMonto: this.reporteZonas.reduce((sum, item) => sum + item.montoTotal, 0)
    };
  }

  calcularTotalesMovimientosCaja(): void {
    this.totales = {
      totalRegistros: this.reporteMovimientosCaja.length,
      totalIngresos: this.reporteMovimientosCaja
        .filter(m => m.tipoOperacion === TipoOperacion.Ingreso)
        .reduce((sum, m) => sum + m.monto, 0),
      totalEgresos: this.reporteMovimientosCaja
        .filter(m => m.tipoOperacion === TipoOperacion.Egreso)
        .reduce((sum, m) => sum + m.monto, 0)
    };
    this.totales.diferenciaNeta = this.totales.totalIngresos! - this.totales.totalEgresos!;
  }

  calcularTotalesVentas(): void {
    this.totales = {
      totalRegistros: this.reporteVentas.length,
      totalKg: this.reporteVentas.reduce((sum, item) => sum + item.pesoNeto, 0),
      totalMonto: this.reporteVentas.reduce((sum, item) => sum + item.montoTotal, 0)
    };
  }

  exportarExcel(): void {
    if (!this.mostrandoResultados) {
      alert('Debe generar un reporte primero');
      return;
    }

    this.cargando = true;

    let exportObservable;
    let nombreArchivo = '';

    switch (this.tabActiva) {
      case 'compras-cliente':
        exportObservable = this.reportesService.exportarReporteComprasCliente(this.filtros);
        nombreArchivo = `reporte_compras_cliente_${this.obtenerTimestamp()}.xlsx`;
        break;
      case 'compras-producto':
        exportObservable = this.reportesService.exportarReporteComprasProducto(this.filtros);
        nombreArchivo = `reporte_compras_producto_${this.obtenerTimestamp()}.xlsx`;
        break;
      case 'zonas':
        exportObservable = this.reportesService.exportarReporteZonas(this.filtros);
        nombreArchivo = `reporte_zonas_${this.obtenerTimestamp()}.xlsx`;
        break;
      case 'movimientos-caja':
        exportObservable = this.reportesService.exportarReporteMovimientosCaja(this.filtros);
        nombreArchivo = `reporte_movimientos_caja_${this.obtenerTimestamp()}.xlsx`;
        break;
      case 'ventas':
        exportObservable = this.reportesService.exportarReporteVentas(this.filtros);
        nombreArchivo = `reporte_ventas_${this.obtenerTimestamp()}.xlsx`;
        break;
      default:
        this.cargando = false;
        return;
    }

    exportObservable.subscribe({
      next: (blob: Blob) => {
        this.reportesService.descargarExcel(blob, nombreArchivo);
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al exportar reporte:', error);
        alert('Error al exportar el reporte');
        this.cargando = false;
      }
    });
  }

  obtenerTimestamp(): string {
    const now = new Date();
    return `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}_${String(now.getHours()).padStart(2, '0')}${String(now.getMinutes()).padStart(2, '0')}${String(now.getSeconds()).padStart(2, '0')}`;
  }

  obtenerNombreTipoMovimiento(tipo: TipoMovimiento): string {
    const nombres: { [key in TipoMovimiento]: string } = {
      [TipoMovimiento.Compra]: 'Compra',
      [TipoMovimiento.Venta]: 'Venta',
      [TipoMovimiento.Prestamo]: 'Préstamo',
      [TipoMovimiento.Abono]: 'Abono',
      [TipoMovimiento.Inyeccion]: 'Inyección',
      [TipoMovimiento.Retiro]: 'Retiro',
      [TipoMovimiento.GastoOperativo]: 'Gasto Operativo'
    };
    return nombres[tipo] || tipo.toString();
  }

  obtenerNombreTipoOperacion(tipo: TipoOperacion): string {
    return tipo === TipoOperacion.Ingreso ? 'Ingreso' : 'Egreso';
  }
}
