export enum EstadoCaja {
  Abierta = 0,
  CerradaManual = 1,
  CerradaAutomatica = 2
}

export enum TipoMovimiento {
  Compra = 0,
  Venta = 1,
  Prestamo = 2,
  Abono = 3,
  Inyeccion = 4,
  Retiro = 5,
  GastoOperativo = 6
}

export enum TipoPesado {
  Kg = 0,
  Valdeo = 1
}

export enum TipoOperacion {
  Ingreso = 0,
  Egreso = 1
}
