export interface ClienteProveedor {
  id: number;
  dni: string;
  nombreCompleto: string;
  telefono?: string;
  direccion?: string;
  fechaNacimiento?: Date;
  zonaId?: number;
  saldoPrestamo: number;
  esAnonimo: boolean;
  fechaCreacion: Date;
  fechaModificacion: Date;
  zona?: any; // Zona
}
