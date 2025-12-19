import { TipoPesado } from './enums';

export interface Compra {
  id: number;
  numeroVoucher: string;
  clienteProveedorId: number;
  productoId: number;
  cajaId: number;
  nivelSecado: string;
  calidad: string;
  tipoPesado: TipoPesado;
  pesoBruto: number;
  descuentoKg: number;
  pesoNeto: number;
  precioPorKg: number;
  montoTotal: number;
  fechaCompra: Date;
  editada: boolean;
  fechaEdicion?: Date;
  esAjustePosterior: boolean;
}
