import { TipoPesado } from './enums';

// Detalle de un producto dentro de una compra
export interface DetalleCompra {
  id: number;
  productoId: number;
  productoNombre: string;
  nivelSecado: string;
  calidad: string;
  tipoPesado: TipoPesado;
  pesoBruto: number;
  descuentoKg: number;
  pesoNeto: number;
  precioPorKg: number;
  subtotal: number;
}

// Detalle de producto para registrar una nueva compra
export interface DetalleCompraRequest {
  productoId: number;
  nivelSecado: string;
  calidad: string;
  tipoPesado: TipoPesado;
  pesoBruto: number;
  descuentoKg: number;
  precioPorKg: number;
}

// Compra completa con múltiples productos
export interface Compra {
  id: number;
  numeroVoucher: string;
  clienteProveedorId: number;
  clienteNombre: string;
  clienteDNI: string;
  cajaId: number;
  detalles: DetalleCompra[];
  pesoTotal: number;
  montoTotal: number;
  fechaCompra: Date;
  editada: boolean;
  fechaEdicion?: Date;
  esAjustePosterior: boolean;
}

// Request para registrar una nueva compra
export interface RegistrarCompraRequest {
  clienteProveedorId: number;
  detalles: DetalleCompraRequest[];
}
