import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CajaService } from '../../core/services/caja.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { Caja } from '../../core/models/caja.model';
import { EstadoCaja, TipoMovimiento, TipoOperacion } from '../../core/models/enums';
import { CustomValidators } from '../../core/validators/custom-validators';

@Component({
  selector: 'app-caja',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTooltipModule,
    FormatoMonedaPipe,
    FormatoFechaPipe
  ],
  templateUrl: './caja.component.html',
  styleUrls: ['./caja.component.css']
})
export class CajaComponent implements OnInit {
  cajaActual: any = null;
  movimientos: any[] = [];
  historial: any[] = [];

  columnasMovimientos: string[] = ['fecha', 'tipo', 'concepto', 'operacion', 'monto', 'ajuste'];
  columnasHistorial: string[] = ['fecha', 'estado', 'montoInicial', 'montoEsperado', 'arqueoReal', 'diferencia', 'usuarioApertura', 'acciones'];

  alertMessage: string = '';
  alertType: 'success' | 'error' | 'warning' | 'info' = 'info';
  showAlert: boolean = false;

  constructor(
    private cajaService: CajaService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.cargarCajaActual();
    this.cargarHistorial();
  }

  cargarCajaActual(): void {
    this.cajaService.obtenerCajaActual().subscribe({
      next: (data) => {
        this.cajaActual = data;
        if (this.cajaActual && this.cajaActual.id) {
          this.cargarMovimientos();
        }
      },
      error: (error) => {
        console.error('Error al cargar caja:', error);
        this.cajaActual = {};
        this.mostrarAlerta('Error al cargar la caja actual', 'error');
      }
    });
  }

  cargarMovimientos(): void {
    if (!this.cajaActual || !this.cajaActual.id) return;

    this.cajaService.obtenerDetalleCaja(this.cajaActual.id).subscribe({
      next: (data) => {
        this.movimientos = data.movimientos || [];
      },
      error: (error) => {
        console.error('Error al cargar movimientos:', error);
        this.mostrarAlerta('Error al cargar los movimientos', 'error');
      }
    });
  }

  cargarHistorial(): void {
    this.cajaService.obtenerHistorial().subscribe({
      next: (data) => {
        this.historial = data.items || data || [];
      },
      error: (error) => {
        console.error('Error al cargar historial:', error);
        this.mostrarAlerta('Error al cargar el historial', 'error');
      }
    });
  }

  abrirDialogoAbrirCaja(): void {
    // Obtener la última caja cerrada para prellenar el monto inicial
    this.cajaService.obtenerUltimaCajaCerrada().subscribe({
      next: (ultimaCaja) => {
        const montoInicialSugerido = ultimaCaja?.arqueoReal || 0;

        const dialogRef = this.dialog.open(AbrirCajaDialogComponent, {
          width: '500px',
          disableClose: false,
          data: { montoInicialSugerido }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.cajaService.abrirCaja(result.montoInicial).subscribe({
              next: () => {
                this.mostrarAlerta('Caja abierta exitosamente', 'success');
                this.cargarCajaActual();
                this.cargarHistorial();
              },
              error: (error) => {
                this.mostrarAlerta('Error al abrir la caja: ' + (error.error?.message || error.message), 'error');
              }
            });
          }
        });
      },
      error: () => {
        // Si no hay caja anterior, abrir sin monto sugerido
        const dialogRef = this.dialog.open(AbrirCajaDialogComponent, {
          width: '500px',
          disableClose: false,
          data: { montoInicialSugerido: 0 }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.cajaService.abrirCaja(result.montoInicial).subscribe({
              next: () => {
                this.mostrarAlerta('Caja abierta exitosamente', 'success');
                this.cargarCajaActual();
                this.cargarHistorial();
              },
              error: (error) => {
                this.mostrarAlerta('Error al abrir la caja: ' + (error.error?.message || error.message), 'error');
              }
            });
          }
        });
      }
    });
  }

  abrirDialogoCerrarCaja(): void {
    const dialogRef = this.dialog.open(CerrarCajaDialogComponent, {
      width: '500px',
      disableClose: false,
      data: { cajaActual: this.cajaActual }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.cajaService.cerrarCaja(result.arqueoReal).subscribe({
          next: () => {
            this.mostrarAlerta('Caja cerrada exitosamente', 'success');
            this.cargarCajaActual();
            this.cargarHistorial();
          },
          error: (error) => {
            this.mostrarAlerta('Error al cerrar la caja: ' + (error.error?.message || error.message), 'error');
          }
        });
      }
    });
  }

  reabrirCaja(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirmar Reapertura',
        message: '¿Está seguro que desea reabrir la caja? Esto permitirá realizar operaciones nuevamente.',
        confirmText: 'Reabrir',
        cancelText: 'Cancelar',
        type: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.cajaService.reabrirCaja().subscribe({
          next: () => {
            this.mostrarAlerta('Caja reabierta exitosamente', 'success');
            this.cargarCajaActual();
            this.cargarHistorial();
          },
          error: (error) => {
            this.mostrarAlerta('Error al reabrir la caja: ' + (error.error?.message || error.message), 'error');
          }
        });
      }
    });
  }

  abrirDialogoMovimiento(): void {
    const dialogRef = this.dialog.open(RegistrarMovimientoDialogComponent, {
      width: '600px',
      disableClose: false,
      data: { cajaId: this.cajaActual.id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.cajaService.registrarMovimiento(
          result.tipoMovimiento,
          result.monto,
          result.concepto
        ).subscribe({
          next: () => {
            this.mostrarAlerta('Movimiento registrado exitosamente', 'success');
            this.cargarCajaActual();
            this.cargarMovimientos();
          },
          error: (error) => {
            this.mostrarAlerta('Error al registrar el movimiento: ' + (error.error?.message || error.message), 'error');
          }
        });
      }
    });
  }

  verDetalleCaja(cajaId: number): void {
    this.cajaService.obtenerDetalleCaja(cajaId).subscribe({
      next: (data) => {
        this.dialog.open(DetalleCajaDialogComponent, {
          width: '900px',
          data: data
        });
      },
      error: (error) => {
        this.mostrarAlerta('Error al cargar el detalle de la caja', 'error');
      }
    });
  }

  getEstadoClass(estado: EstadoCaja): string {
    switch (estado) {
      case EstadoCaja.Abierta: return 'estado-abierta';
      case EstadoCaja.CerradaManual: return 'estado-cerrada-manual';
      case EstadoCaja.CerradaAutomatica: return 'estado-cerrada-automatica';
      default: return '';
    }
  }

  getEstadoTexto(estado: EstadoCaja): string {
    switch (estado) {
      case EstadoCaja.Abierta: return 'Abierta';
      case EstadoCaja.CerradaManual: return 'Cerrada Manual';
      case EstadoCaja.CerradaAutomatica: return 'Cerrada Automática';
      default: return 'Desconocido';
    }
  }

  getTipoMovimientoClass(tipo: TipoMovimiento): string {
    switch (tipo) {
      case TipoMovimiento.Compra: return 'tipo-compra';
      case TipoMovimiento.Venta: return 'tipo-venta';
      case TipoMovimiento.Prestamo: return 'tipo-prestamo';
      case TipoMovimiento.Abono: return 'tipo-abono';
      case TipoMovimiento.Inyeccion: return 'tipo-inyeccion';
      case TipoMovimiento.Retiro: return 'tipo-retiro';
      case TipoMovimiento.GastoOperativo: return 'tipo-gasto';
      default: return '';
    }
  }

  getTipoMovimientoTexto(tipo: TipoMovimiento): string {
    switch (tipo) {
      case TipoMovimiento.Compra: return 'Compra';
      case TipoMovimiento.Venta: return 'Venta';
      case TipoMovimiento.Prestamo: return 'Préstamo';
      case TipoMovimiento.Abono: return 'Abono';
      case TipoMovimiento.Inyeccion: return 'Inyección';
      case TipoMovimiento.Retiro: return 'Retiro';
      case TipoMovimiento.GastoOperativo: return 'Gasto Operativo';
      default: return 'Desconocido';
    }
  }

  mostrarAlerta(mensaje: string, tipo: 'success' | 'error' | 'warning' | 'info'): void {
    this.alertMessage = mensaje;
    this.alertType = tipo;
    this.showAlert = true;
  }
}

// ==================== DIALOGS ====================

// Dialog: Abrir Caja
@Component({
  selector: 'app-abrir-caja-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>lock_open</mat-icon>
      Abrir Caja
    </h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Monto Inicial</mat-label>
          <input matInput type="number" step="0.01" formControlName="montoInicial" placeholder="0.00">
          <span matPrefix class="currency-prefix">S/&nbsp;</span>
          <mat-error *ngIf="form.get('montoInicial')?.hasError('required')">
            El monto inicial es requerido
          </mat-error>
          <mat-error *ngIf="form.get('montoInicial')?.hasError('monto')">
            {{ getErrorMessage('montoInicial') }}
          </mat-error>
        </mat-form-field>
      </form>
      <p class="info-text" *ngIf="montoSugerido > 0">
        <mat-icon>lightbulb</mat-icon>
        El monto inicial se ha prellenado con el arqueo de la última caja cerrada (S/ {{ montoSugerido | number:'1.2-2' }}). Puede modificarlo si es necesario.
      </p>
      <p class="info-text" *ngIf="montoSugerido === 0">
        <mat-icon>info</mat-icon>
        Ingrese el monto en efectivo con el que iniciará la caja del día.
      </p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancelar</button>
      <button mat-raised-button color="primary" (click)="onConfirm()" [disabled]="!form.valid">
        Abrir Caja
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 12px; color: #2d3748; }
    mat-dialog-content { min-width: 400px; padding: 20px 24px; }
    .full-width { width: 100%; }
    .info-text {
      display: flex;
      align-items: center;
      gap: 8px;
      background: #edf2f7;
      padding: 12px;
      border-radius: 8px;
      font-size: 13px;
      color: #4a5568;
      margin-top: 16px;
    }
    .info-text mat-icon { font-size: 20px; width: 20px; height: 20px; color: #4299e1; }
    mat-dialog-actions { padding: 16px 24px; }
  `]
})
export class AbrirCajaDialogComponent {
  form: FormGroup;
  montoSugerido: number = 0;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AbrirCajaDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.montoSugerido = data?.montoInicialSugerido || 0;
    this.form = this.fb.group({
      montoInicial: [this.montoSugerido, [Validators.required, CustomValidators.monto()]]
    });
  }

  getErrorMessage(controlName: string): string {
    return CustomValidators.getErrorMessage(this.form.get(controlName));
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }
}

// Dialog: Cerrar Caja
@Component({
  selector: 'app-cerrar-caja-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    FormatoMonedaPipe
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>lock</mat-icon>
      Cerrar Caja
    </h2>
    <mat-dialog-content>
      <div class="caja-info">
        <div class="info-row">
          <span class="label">Saldo Actual:</span>
          <span class="value">{{ cajaActual.saldoActual | formatoMoneda }}</span>
        </div>
      </div>

      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Arqueo Real</mat-label>
          <input matInput type="number" step="0.01" formControlName="arqueoReal" placeholder="0.00" (input)="calcularDiferencia()">
          <span matPrefix class="currency-prefix">S/&nbsp;</span>
          <mat-error *ngIf="form.get('arqueoReal')?.hasError('required')">
            El arqueo real es requerido
          </mat-error>
          <mat-error *ngIf="form.get('arqueoReal')?.hasError('monto')">
            {{ getErrorMessage('arqueoReal') }}
          </mat-error>
        </mat-form-field>
      </form>

      <div class="diferencia-box" *ngIf="diferencia !== null" [ngClass]="{'positiva': diferencia >= 0, 'negativa': diferencia < 0}">
        <mat-icon>{{ diferencia >= 0 ? 'trending_up' : 'trending_down' }}</mat-icon>
        <div>
          <span class="label">Diferencia:</span>
          <span class="value">{{ diferencia | formatoMoneda }}</span>
        </div>
      </div>

      <p class="warning-text" *ngIf="diferencia !== null && diferencia !== 0">
        <mat-icon>warning</mat-icon>
        {{ diferencia > 0 ? 'Hay un sobrante' : 'Hay un faltante' }} en caja. Verifique el arqueo antes de confirmar.
      </p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancelar</button>
      <button mat-raised-button color="warn" (click)="onConfirm()" [disabled]="!form.valid">
        Cerrar Caja
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 12px; color: #2d3748; }
    mat-dialog-content { min-width: 400px; padding: 20px 24px; }
    .full-width { width: 100%; }
    .caja-info {
      background: #f7fafc;
      padding: 16px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    .info-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .info-row .label {
      font-size: 14px;
      color: #718096;
      font-weight: 600;
    }
    .info-row .value {
      font-size: 18px;
      color: #2b6cb0;
      font-weight: 600;
    }
    .diferencia-box {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      border-radius: 8px;
      margin-top: 16px;
    }
    .diferencia-box.positiva {
      background: #c6f6d5;
      color: #22543d;
    }
    .diferencia-box.negativa {
      background: #fed7d7;
      color: #742a2a;
    }
    .diferencia-box mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
    }
    .diferencia-box .label {
      font-size: 12px;
      font-weight: 600;
      display: block;
    }
    .diferencia-box .value {
      font-size: 20px;
      font-weight: 700;
    }
    .warning-text {
      display: flex;
      align-items: center;
      gap: 8px;
      background: #fef5e7;
      padding: 12px;
      border-radius: 8px;
      font-size: 13px;
      color: #975a16;
      margin-top: 16px;
    }
    .warning-text mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
      color: #ed8936;
    }
    mat-dialog-actions { padding: 16px 24px; }
  `]
})
export class CerrarCajaDialogComponent {
  form: FormGroup;
  cajaActual: any;
  diferencia: number | null = null;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CerrarCajaDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.cajaActual = data.cajaActual;
    this.form = this.fb.group({
      arqueoReal: ['', [Validators.required, CustomValidators.monto()]]
    });
  }

  calcularDiferencia(): void {
    const arqueoReal = this.form.get('arqueoReal')?.value;
    if (arqueoReal !== null && arqueoReal !== '') {
      this.diferencia = parseFloat(arqueoReal) - this.cajaActual.saldoActual;
    } else {
      this.diferencia = null;
    }
  }

  getErrorMessage(controlName: string): string {
    return CustomValidators.getErrorMessage(this.form.get(controlName));
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }
}

// Dialog: Registrar Movimiento
@Component({
  selector: 'app-registrar-movimiento-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>add_circle</mat-icon>
      Registrar Movimiento Manual
    </h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Tipo de Movimiento</mat-label>
          <mat-select formControlName="tipoMovimiento">
            <mat-option [value]="4">Inyección (Ingreso)</mat-option>
            <mat-option [value]="5">Retiro (Egreso)</mat-option>
            <mat-option [value]="6">Gasto Operativo (Egreso)</mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('tipoMovimiento')?.hasError('required')">
            Seleccione un tipo de movimiento
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Monto</mat-label>
          <input matInput type="number" step="0.01" formControlName="monto" placeholder="0.00">
          <span matPrefix class="currency-prefix">S/&nbsp;</span>
          <mat-error *ngIf="form.get('monto')?.hasError('required')">
            El monto es requerido
          </mat-error>
          <mat-error *ngIf="form.get('monto')?.hasError('monto')">
            {{ getErrorMessage('monto') }}
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Concepto</mat-label>
          <textarea matInput formControlName="concepto" rows="3" placeholder="Descripción del movimiento"></textarea>
          <mat-error *ngIf="form.get('concepto')?.hasError('required')">
            El concepto es requerido
          </mat-error>
          <mat-error *ngIf="form.get('concepto')?.hasError('maxlength')">
            Máximo 500 caracteres
          </mat-error>
        </mat-form-field>
      </form>

      <p class="info-text">
        <mat-icon>info</mat-icon>
        Los movimientos manuales permiten registrar inyecciones de efectivo, retiros o gastos operativos.
      </p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancelar</button>
      <button mat-raised-button color="accent" (click)="onConfirm()" [disabled]="!form.valid">
        Registrar
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 12px; color: #2d3748; }
    mat-dialog-content { min-width: 500px; padding: 20px 24px; }
    .full-width { width: 100%; margin-bottom: 16px; }
    .info-text {
      display: flex;
      align-items: flex-start;
      gap: 8px;
      background: #edf2f7;
      padding: 12px;
      border-radius: 8px;
      font-size: 13px;
      color: #4a5568;
      margin-top: 8px;
    }
    .info-text mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
      color: #4299e1;
      margin-top: 2px;
    }
    mat-dialog-actions { padding: 16px 24px; }
  `]
})
export class RegistrarMovimientoDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<RegistrarMovimientoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.form = this.fb.group({
      tipoMovimiento: ['', Validators.required],
      monto: ['', [Validators.required, CustomValidators.monto()]],
      concepto: ['', [Validators.required, Validators.maxLength(500)]]
    });
  }

  getErrorMessage(controlName: string): string {
    return CustomValidators.getErrorMessage(this.form.get(controlName));
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }
}

// Dialog: Detalle Caja
@Component({
  selector: 'app-detalle-caja-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    FormatoMonedaPipe,
    FormatoFechaPipe
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>receipt</mat-icon>
      Detalle de Caja - {{ caja.fecha | formatoFecha }}
    </h2>
    <mat-dialog-content>
      <div class="caja-resumen">
        <div class="info-grid">
          <div class="info-item">
            <span class="label">Estado</span>
            <span class="value">{{ getEstadoTexto(caja.estado) }}</span>
          </div>
          <div class="info-item">
            <span class="label">Monto Inicial</span>
            <span class="value">{{ caja.montoInicial | formatoMoneda }}</span>
          </div>
          <div class="info-item">
            <span class="label">Saldo Esperado</span>
            <span class="value">{{ caja.montoEsperado | formatoMoneda }}</span>
          </div>
          <div class="info-item">
            <span class="label">Arqueo Real</span>
            <span class="value">{{ caja.arqueoReal | formatoMoneda }}</span>
          </div>
          <div class="info-item">
            <span class="label">Diferencia</span>
            <span class="value" [ngClass]="{'positive': caja.diferencia >= 0, 'negative': caja.diferencia < 0}">
              {{ caja.diferencia | formatoMoneda }}
            </span>
          </div>
          <div class="info-item">
            <span class="label">Monto Compras</span>
            <span class="value egreso">{{ caja.totalCompras | formatoMoneda }}</span>
          </div>
          <div class="info-item">
            <span class="label">Monto Gastos</span>
            <span class="value egreso">{{ caja.totalGastos | formatoMoneda }}</span>
          </div>
          <div class="info-item">
            <span class="label">Monto Retiros</span>
            <span class="value egreso">{{ caja.totalRetiros | formatoMoneda }}</span>
          </div>
          <div class="info-item">
            <span class="label">Monto Inyecciones</span>
            <span class="value ingreso">{{ caja.totalInyecciones | formatoMoneda }}</span>
          </div>
        </div>
      </div>

      <h3>Movimientos</h3>
      <div class="table-container">
        <table mat-table [dataSource]="movimientos" class="movimientos-table">
          <ng-container matColumnDef="fecha">
            <th mat-header-cell *matHeaderCellDef>Fecha</th>
            <td mat-cell *matCellDef="let mov">{{ mov.fechaMovimiento | formatoFecha: true }}</td>
          </ng-container>

          <ng-container matColumnDef="tipo">
            <th mat-header-cell *matHeaderCellDef>Tipo</th>
            <td mat-cell *matCellDef="let mov">{{ getTipoMovimientoTexto(mov.tipoMovimiento) }}</td>
          </ng-container>

          <ng-container matColumnDef="concepto">
            <th mat-header-cell *matHeaderCellDef>Concepto</th>
            <td mat-cell *matCellDef="let mov">{{ mov.concepto }}</td>
          </ng-container>

          <ng-container matColumnDef="monto">
            <th mat-header-cell *matHeaderCellDef class="text-right">Monto</th>
            <td mat-cell *matCellDef="let mov" class="text-right">
              <span [ngClass]="{'ingreso': mov.tipoOperacion === 0, 'egreso': mov.tipoOperacion === 1}">
                {{ mov.monto | formatoMoneda }}
              </span>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="columnas"></tr>
          <tr mat-row *matRowDef="let row; columns: columnas;"></tr>
        </table>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-raised-button color="primary" (click)="onClose()">Cerrar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 12px; color: #2d3748; }
    mat-dialog-content { min-width: 800px; max-height: 600px; padding: 20px 24px; }
    .caja-resumen {
      background: #f7fafc;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 24px;
    }
    .info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 16px;
    }
    .info-item { display: flex; flex-direction: column; gap: 4px; }
    .info-item .label {
      font-size: 11px;
      font-weight: 600;
      color: #718096;
      text-transform: uppercase;
    }
    .info-item .value {
      font-size: 16px;
      color: #2d3748;
      font-weight: 600;
    }
    .positive { color: #48bb78; }
    .negative { color: #f56565; }
    h3 {
      color: #2d3748;
      font-size: 16px;
      font-weight: 600;
      margin: 0 0 16px 0;
      padding-bottom: 8px;
      border-bottom: 2px solid #e2e8f0;
    }
    .table-container { overflow-x: auto; }
    .movimientos-table { width: 100%; }
    .movimientos-table th {
      background: #f7fafc;
      font-weight: 600;
      padding: 12px;
    }
    .movimientos-table td { padding: 10px 12px; }
    .text-right { text-align: right; }
    .ingreso { color: #48bb78; font-weight: 600; }
    .egreso { color: #f56565; font-weight: 600; }
    mat-dialog-actions { padding: 16px 24px; }
  `]
})
export class DetalleCajaDialogComponent {
  caja: any;
  movimientos: any[] = [];
  columnas: string[] = ['fecha', 'tipo', 'concepto', 'monto'];

  constructor(
    private dialogRef: MatDialogRef<DetalleCajaDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.caja = data;
    this.movimientos = data.movimientos || [];
  }

  getEstadoTexto(estado: EstadoCaja): string {
    switch (estado) {
      case EstadoCaja.Abierta: return 'Abierta';
      case EstadoCaja.CerradaManual: return 'Cerrada Manual';
      case EstadoCaja.CerradaAutomatica: return 'Cerrada Automática';
      default: return 'Desconocido';
    }
  }

  getTipoMovimientoTexto(tipo: TipoMovimiento): string {
    switch (tipo) {
      case TipoMovimiento.Compra: return 'Compra';
      case TipoMovimiento.Venta: return 'Venta';
      case TipoMovimiento.Prestamo: return 'Préstamo';
      case TipoMovimiento.Abono: return 'Abono';
      case TipoMovimiento.Inyeccion: return 'Inyección';
      case TipoMovimiento.Retiro: return 'Retiro';
      case TipoMovimiento.GastoOperativo: return 'Gasto Operativo';
      default: return 'Desconocido';
    }
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
