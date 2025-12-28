import { EstadoCaja } from './enums';

export interface Caja {
  id: number;
  fecha: Date;
  montoInicial: number;
  montoEsperado: number;
  arqueoReal?: number;
  diferencia: number;
  estado: EstadoCaja;
  fechaApertura: Date;
  fechaCierre?: Date;
  usuarioApertura: string;
  usuarioCierre?: string;

  // Totales calculados
  totalIngresos: number;
  totalEgresos: number;
  saldoActual: number;

  // Desglose de movimientos
  totalInyecciones: number;
  totalIngresosSinInyecciones: number;
  totalRetiros: number;
  totalGastos: number;
  totalCompras: number;
  totalVentas: number;
  totalPrestamos: number;
  totalAbonos: number;
}
