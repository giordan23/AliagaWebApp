import { TipoMovimiento } from './enums';

export interface PrestamoResponse {
  id: number;
  clienteProveedorId: number;
  clienteNombre: string;
  clienteDNI: string;
  cajaId: number;
  tipoMovimiento: TipoMovimiento;
  monto: number;
  saldoDespues: number;
  descripcion: string;
  fechaMovimiento: Date;
  esAjustePosterior: boolean;
}

export interface RegistrarPrestamoRequest {
  clienteProveedorId: number;
  monto: number;
  concepto: string;
}

export interface RegistrarAbonoRequest {
  clienteProveedorId: number;
  monto: number;
  concepto: string;
}

// Interfaz para agrupar préstamos por cliente
export interface PrestamoAgrupado {
  clienteId: number;
  clienteNombre: string;
  clienteDNI: string;
  saldoActual: number;
  totalPrestado: number;
  totalAbonado: number;
  fechaUltimoMovimiento: Date;
  movimientos: PrestamoResponse[];
}
