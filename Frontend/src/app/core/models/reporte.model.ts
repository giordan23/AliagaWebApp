import { TipoMovimiento, TipoOperacion } from './enums';

// Filtros comunes para todos los reportes
export interface FiltrosReporte {
  fechaInicio?: Date | string;
  fechaFin?: Date | string;
  clienteId?: number;
  productoId?: number;
  zonaId?: number;
  cajaId?: number;
}

// Reporte de Compras por Cliente
export interface ReporteComprasCliente {
  clienteDNI: string;
  clienteNombre: string;
  zona: string;
  totalCompras: number;
  pesoTotalKg: number;
  montoTotal: number;
  saldoPrestamo: number;
  ultimaCompra?: Date | string;
}

// Reporte de Compras por Producto
export interface ReporteComprasProducto {
  productoNombre: string;
  totalCompras: number;
  pesoTotalKg: number;
  montoTotal: number;
  precioPromedioPorKg: number;
  pesoPromedioCompra: number;
}

// Reporte de Resumen por Zonas
export interface ReporteZonas {
  zonaNombre: string;
  totalProveedores: number;
  totalCompras: number;
  pesoTotalKg: number;
  montoTotal: number;
  promedioComprasPorProveedor: number;
}

// Reporte de Movimientos de Caja
export interface ReporteMovimientosCaja {
  fecha: Date | string;
  cajaId: number;
  tipoMovimiento: TipoMovimiento;
  concepto: string;
  monto: number;
  tipoOperacion: TipoOperacion;
  esAjustePosterior: boolean;
}

// Reporte de Ventas
export interface ReporteVentas {
  fechaVenta: Date | string;
  clienteNombre: string;
  productoNombre: string;
  pesoNeto: number;
  precioPorKg: number;
  montoTotal: number;
  editada: boolean;
}

// Totales para reportes agrupados
export interface TotalesReporte {
  totalRegistros?: number;
  totalKg?: number;
  totalMonto?: number;
  totalIngresos?: number;
  totalEgresos?: number;
  diferenciaNeta?: number;
}
