import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ZonasService } from '../../core/services/zonas.service';
import { AlertBannerComponent, AlertType } from '../../shared/components/alert-banner/alert-banner.component';

@Component({
  selector: 'app-zonas',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatCardModule,
    MatProgressSpinnerModule,
    AlertBannerComponent
  ],
  templateUrl: './zonas.component.html',
  styleUrls: ['./zonas.component.css']
})
export class ZonasComponent implements OnInit {
  zonas: any[] = [];
  loading = false;
  displayedColumns: string[] = ['id', 'nombre', 'descripcion', 'acciones'];

  // Para el alert banner
  showAlert = false;
  alertType: AlertType = 'info';
  alertMessage = '';

  constructor(
    private zonasService: ZonasService,
    private dialog: MatDialog,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.cargarZonas();
  }

  cargarZonas(): void {
    this.loading = true;
    this.zonasService.obtenerTodas().subscribe({
      next: (response) => {
        this.zonas = response.items || [];
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar zonas:', error);
        this.mostrarAlerta('error', 'Error al cargar las zonas');
        this.loading = false;
      }
    });
  }

  eliminarZona(zona: any): void {
    if (!confirm(`¿Está seguro de eliminar la zona "${zona.nombre}"?`)) {
      return;
    }

    this.zonasService.eliminar(zona.id).subscribe({
      next: () => {
        this.cargarZonas();
        this.mostrarAlerta('success', 'Zona eliminada exitosamente');
      },
      error: (error) => {
        console.error('Error al eliminar zona:', error);
        this.mostrarAlerta('error', error.error?.message || 'Error al eliminar la zona');
      }
    });
  }

  abrirFormularioNueva(): void {
    const dialogRef = this.dialog.open(ZonaFormDialogComponent, {
      width: '600px',
      disableClose: false,
      data: null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.cargarZonas();
        this.mostrarAlerta('success', 'Zona creada exitosamente');
      }
    });
  }

  abrirFormularioEditar(zona: any): void {
    const dialogRef = this.dialog.open(ZonaFormDialogComponent, {
      width: '600px',
      disableClose: false,
      data: zona
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.cargarZonas();
        this.mostrarAlerta('success', 'Zona actualizada exitosamente');
      }
    });
  }

  verClientes(zona: any): void {
    this.zonasService.obtenerClientesPorZona(zona.id).subscribe({
      next: (clientes) => {
        const dialogRef = this.dialog.open(ClientesZonaDialogComponent, {
          width: '700px',
          data: { zona, clientes }
        });
      },
      error: (error) => {
        this.mostrarAlerta('error', 'Error al cargar clientes de la zona');
      }
    });
  }

  mostrarAlerta(type: AlertType, message: string): void {
    this.showAlert = false;
    setTimeout(() => {
      this.alertType = type;
      this.alertMessage = message;
      this.showAlert = true;
    }, 100);
  }
}

// ==================== COMPONENTE DE FORMULARIO ====================

import { Component as NgComponent, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@NgComponent({
  selector: 'app-zona-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data ? 'Editar' : 'Nueva' }} Zona</h2>

    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Nombre de la Zona</mat-label>
          <input matInput formControlName="nombre" maxlength="100" required>
          <mat-error *ngIf="submitted && form.get('nombre')?.hasError('required')">
            El nombre es requerido
          </mat-error>
          <mat-error *ngIf="form.get('nombre')?.hasError('maxlength')">
            Máximo 100 caracteres
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Descripción (Opcional)</mat-label>
          <textarea
            matInput
            formControlName="descripcion"
            rows="3"
            maxlength="500"></textarea>
          <mat-hint align="end">
            {{ form.get('descripcion')?.value?.length || 0 }}/500
          </mat-hint>
        </mat-form-field>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()" [disabled]="loading">
        Cancelar
      </button>
      <button
        mat-raised-button
        color="primary"
        (click)="onSubmit()"
        [disabled]="loading">
        <mat-spinner *ngIf="loading" diameter="20" style="display: inline-block; margin-right: 8px;"></mat-spinner>
        {{ loading ? 'Guardando...' : 'Guardar' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    mat-dialog-content {
      padding: 20px 24px;
      min-height: 200px;
    }

    form {
      display: flex;
      flex-direction: column;
    }
  `]
})
export class ZonaFormDialogComponent implements OnInit {
  form: FormGroup;
  loading = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private zonasService: ZonasService,
    private dialogRef: MatDialogRef<ZonaFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.form = this.fb.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      descripcion: ['', Validators.maxLength(500)]
    });
  }

  ngOnInit(): void {
    if (this.data) {
      this.form.patchValue({
        nombre: this.data.nombre,
        descripcion: this.data.descripcion
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    this.loading = true;

    const request = this.data
      ? this.zonasService.actualizar(this.data.id, this.form.value)
      : this.zonasService.crear(this.form.value);

    request.subscribe({
      next: (response) => {
        this.dialogRef.close(response);
      },
      error: (error) => {
        console.error('Error al guardar zona:', error);
        alert('Error al guardar la zona: ' + (error.error?.message || 'Error desconocido'));
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}

// ==================== COMPONENTE DE CLIENTES POR ZONA ====================

@NgComponent({
  selector: 'app-clientes-zona-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>people</mat-icon>
      Clientes de: {{ data.zona.nombre }}
    </h2>

    <mat-dialog-content>
      <div *ngIf="data.clientes.length === 0" class="no-data">
        <p>No hay clientes en esta zona</p>
      </div>

      <table *ngIf="data.clientes.length > 0" class="clientes-table">
        <thead>
          <tr>
            <th>DNI</th>
            <th>Nombre Completo</th>
            <th>Teléfono</th>
            <th>Saldo Préstamo</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let cliente of data.clientes">
            <td>{{ cliente.dni }}</td>
            <td>{{ cliente.nombreCompleto }}</td>
            <td>{{ cliente.telefono || '-' }}</td>
            <td [class.saldo-deuda]="cliente.saldoPrestamo > 0">
              S/ {{ cliente.saldoPrestamo?.toFixed(2) || '0.00' }}
            </td>
          </tr>
        </tbody>
      </table>

      <div class="total-clientes">
        <strong>Total de clientes: {{ data.clientes.length }}</strong>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-raised-button color="primary" mat-dialog-close>
        Cerrar
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 10px;
      color: #2c3e50;
    }

    mat-dialog-content {
      padding: 20px 24px;
      min-height: 200px;
      max-height: 500px;
      overflow-y: auto;
    }

    .no-data {
      text-align: center;
      padding: 40px;
      color: #999;
    }

    .clientes-table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 20px;
    }

    .clientes-table th {
      background: #34495e;
      color: white;
      padding: 12px;
      text-align: left;
      font-weight: 600;
    }

    .clientes-table td {
      padding: 12px;
      border-bottom: 1px solid #ecf0f1;
    }

    .clientes-table tr:hover {
      background: #f8f9fa;
    }

    .saldo-deuda {
      color: #e74c3c;
      font-weight: 600;
    }

    .total-clientes {
      padding: 12px;
      background: #ecf0f1;
      border-radius: 4px;
      text-align: right;
    }
  `]
})
export class ClientesZonaDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}
}
