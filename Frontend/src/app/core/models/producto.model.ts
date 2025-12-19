export interface Producto {
  id: number;
  nombre: string;
  precioSugeridoPorKg: number;
  nivelesSecado: string; // JSON string
  calidades: string; // JSON string
  permiteValdeo: boolean;
  fechaModificacion: Date;
}

export interface ProductoCaracteristicas {
  nivelesSecado: string[];
  calidades: string[];
  permiteValdeo: boolean;
}
