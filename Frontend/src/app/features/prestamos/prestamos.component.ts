import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PrestamosService } from '../../core/services/prestamos.service';
import { PrestamoResponse, PrestamoAgrupado, RegistrarPrestamoRequest, RegistrarAbonoRequest } from '../../core/models/prestamo.model';
import { TipoMovimiento } from '../../core/models/enums';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { ClienteAutocompleteComponent } from '../../shared/components/cliente-autocomplete/cliente-autocomplete.component';

@Component({
  selector: 'app-prestamos',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatSnackBarModule,
    FormatoMonedaPipe,
    ClienteAutocompleteComponent
  ],
  templateUrl: './prestamos.component.html',
  styleUrls: ['./prestamos.component.css']
})
export class PrestamosComponent implements OnInit {
  activeTab: 'prestamo' | 'abono' = 'prestamo';
  prestamoForm: FormGroup;
  abonoForm: FormGroup;
  prestamosAgrupados: PrestamoAgrupado[] = [];
  ordenamiento: 'monto-desc' | 'monto-asc' | 'fecha-desc' | 'fecha-asc' = 'monto-desc';
  procesando = false;
  cargando = false;

  constructor(
    private fb: FormBuilder,
    private prestamosService: PrestamosService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.prestamoForm = this.fb.group({
      clienteProveedorId: [null, Validators.required],
      monto: [null, [Validators.required, Validators.min(0.01)]],
      concepto: ['Préstamo sin interés']
    });

    this.abonoForm = this.fb.group({
      clienteProveedorId: [null, Validators.required],
      monto: [null, [Validators.required, Validators.min(0.01)]],
      concepto: ['Abono a préstamo']
    });
  }

  ngOnInit(): void {
    this.cargarPrestamos();
  }

  cargarPrestamos(): void {
    this.cargando = true;
    this.prestamosService.obtenerTodos(1, 1000).subscribe({
      next: (response) => {
        const prestamos: PrestamoResponse[] = response.data || response || [];
        this.agruparPrestamos(prestamos);
        this.ordenarPrestamos();
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar préstamos:', error);
        this.snackBar.open('Error al cargar los préstamos', 'Cerrar', { duration: 3000 });
        this.cargando = false;
      }
    });
  }

  agruparPrestamos(prestamos: PrestamoResponse[]): void {
    // Agrupar por cliente
    const agrupados = new Map<number, PrestamoAgrupado>();

    prestamos.forEach(prestamo => {
      if (!agrupados.has(prestamo.clienteProveedorId)) {
        agrupados.set(prestamo.clienteProveedorId, {
          clienteId: prestamo.clienteProveedorId,
          clienteNombre: prestamo.clienteNombre,
          clienteDNI: prestamo.clienteDNI,
          saldoActual: 0,
          totalPrestado: 0,
          totalAbonado: 0,
          fechaUltimoMovimiento: new Date(prestamo.fechaMovimiento),
          movimientos: []
        });
      }

      const grupo = agrupados.get(prestamo.clienteProveedorId)!;
      grupo.movimientos.push(prestamo);

      // Actualizar totales
      if (prestamo.tipoMovimiento === TipoMovimiento.Prestamo) {
        grupo.totalPrestado += prestamo.monto;
      } else if (prestamo.tipoMovimiento === TipoMovimiento.Abono) {
        grupo.totalAbonado += prestamo.monto;
      }

      // Actualizar fecha de último movimiento
      const fechaMovimiento = new Date(prestamo.fechaMovimiento);
      if (fechaMovimiento > grupo.fechaUltimoMovimiento) {
        grupo.fechaUltimoMovimiento = fechaMovimiento;
      }
    });

    // Calcular saldo actual y convertir a array
    this.prestamosAgrupados = Array.from(agrupados.values()).map(grupo => {
      grupo.saldoActual = grupo.totalPrestado - grupo.totalAbonado;
      // Ordenar movimientos por fecha descendente
      grupo.movimientos.sort((a, b) =>
        new Date(b.fechaMovimiento).getTime() - new Date(a.fechaMovimiento).getTime()
      );
      return grupo;
    });
  }

  ordenarPrestamos(): void {
    switch (this.ordenamiento) {
      case 'monto-desc':
        this.prestamosAgrupados.sort((a, b) => b.saldoActual - a.saldoActual);
        break;
      case 'monto-asc':
        this.prestamosAgrupados.sort((a, b) => a.saldoActual - b.saldoActual);
        break;
      case 'fecha-desc':
        this.prestamosAgrupados.sort((a, b) =>
          b.fechaUltimoMovimiento.getTime() - a.fechaUltimoMovimiento.getTime()
        );
        break;
      case 'fecha-asc':
        this.prestamosAgrupados.sort((a, b) =>
          a.fechaUltimoMovimiento.getTime() - b.fechaUltimoMovimiento.getTime()
        );
        break;
    }
  }

  registrarPrestamo(): void {
    if (this.prestamoForm.invalid) {
      this.prestamoForm.markAllAsTouched();
      return;
    }

    this.procesando = true;
    const request: RegistrarPrestamoRequest = this.prestamoForm.value;

    this.prestamosService.registrarPrestamo(request).subscribe({
      next: () => {
        this.snackBar.open('Préstamo registrado exitosamente', 'Cerrar', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.resetPrestamoForm();
        this.cargarPrestamos();
        this.procesando = false;
      },
      error: (error) => {
        console.error('Error al registrar préstamo:', error);
        const mensaje = error.error?.message || 'Error al registrar el préstamo';
        this.snackBar.open(mensaje, 'Cerrar', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
        this.procesando = false;
      }
    });
  }

  registrarAbono(): void {
    if (this.abonoForm.invalid) {
      this.abonoForm.markAllAsTouched();
      return;
    }

    this.procesando = true;
    const request: RegistrarAbonoRequest = this.abonoForm.value;

    this.prestamosService.registrarAbono(request).subscribe({
      next: () => {
        this.snackBar.open('Abono registrado exitosamente', 'Cerrar', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.resetAbonoForm();
        this.cargarPrestamos();
        this.procesando = false;
      },
      error: (error) => {
        console.error('Error al registrar abono:', error);
        const mensaje = error.error?.message || 'Error al registrar el abono';
        this.snackBar.open(mensaje, 'Cerrar', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
        this.procesando = false;
      }
    });
  }

  resetPrestamoForm(): void {
    this.prestamoForm.reset({
      clienteProveedorId: null,
      monto: null,
      concepto: 'Préstamo sin interés'
    });
  }

  resetAbonoForm(): void {
    this.abonoForm.reset({
      clienteProveedorId: null,
      monto: null,
      concepto: 'Abono a préstamo'
    });
  }

  async verDetallesPrestamo(prestamo: PrestamoAgrupado): Promise<void> {
    const { PrestamoDetalleDialogComponent } = await import('./prestamo-detalle-dialog/prestamo-detalle-dialog.component');
    this.dialog.open(PrestamoDetalleDialogComponent, {
      width: '700px',
      maxWidth: '90vw',
      data: prestamo
    });
  }
}
