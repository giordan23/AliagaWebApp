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
}
