export interface Venta {
  id: number;
  clienteCompradorId: number;
  productoId: number;
  cajaId: number;
  pesoBruto: number;
  pesoNeto: number;
  precioPorKg: number;
  montoTotal: number;
  fechaVenta: Date;
  editada: boolean;
  fechaEdicion?: Date;
  esAjustePosterior: boolean;
}
