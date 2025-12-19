import { TipoMovimiento } from './enums';

export interface Prestamo {
  id: number;
  clienteProveedorId: number;
  cajaId: number;
  tipoMovimiento: TipoMovimiento;
  monto: number;
  descripcion?: string;
  fechaMovimiento: Date;
  saldoDespues: number;
  esAjustePosterior: boolean;
}
