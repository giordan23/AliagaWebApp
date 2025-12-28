import { TipoMovimiento, TipoOperacion } from './enums';

export interface MovimientoCaja {
  id: number;
  cajaId: number;
  fechaCaja: Date;
  tipoMovimiento: TipoMovimiento;
  referenciaId?: number;
  concepto: string;
  monto: number;
  tipoOperacion: TipoOperacion;
  fechaMovimiento: Date;
  esAjustePosterior: boolean;
}
