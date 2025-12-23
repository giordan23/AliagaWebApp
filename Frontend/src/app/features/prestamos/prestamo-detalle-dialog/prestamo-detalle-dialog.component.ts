import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { PrestamoAgrupado } from '../../../core/models/prestamo.model';
import { TipoMovimiento } from '../../../core/models/enums';
import { FormatoMonedaPipe } from '../../../shared/pipes/formato-moneda.pipe';

@Component({
  selector: 'app-prestamo-detalle-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    FormatoMonedaPipe
  ],
  templateUrl: './prestamo-detalle-dialog.component.html',
  styleUrls: ['./prestamo-detalle-dialog.component.css']
})
export class PrestamoDetalleDialogComponent {
  displayedColumns: string[] = ['fecha', 'tipo', 'monto', 'saldoDespues', 'descripcion'];
  TipoMovimiento = TipoMovimiento;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: PrestamoAgrupado,
    public dialogRef: MatDialogRef<PrestamoDetalleDialogComponent>
  ) {}

  getTipoMovimientoLabel(tipo: TipoMovimiento): string {
    return tipo === TipoMovimiento.Prestamo ? 'Préstamo' : 'Abono';
  }

  getTipoMovimientoClass(tipo: TipoMovimiento): string {
    return tipo === TipoMovimiento.Prestamo ? 'tipo-prestamo' : 'tipo-abono';
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}
