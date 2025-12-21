export interface Producto {
  id: number;
  nombre: string;
  precioSugeridoPorKg: number;
  nivelesSecado: string[]; // Array de niveles de secado
  calidades: string[]; // Array de calidades
  permiteValdeo: boolean;
  fechaModificacion: Date;
}

export interface ProductoCaracteristicas {
  nivelesSecado: string[];
  calidades: string[];
  permiteValdeo: boolean;
}
