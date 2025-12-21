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
import { VentasService } from '../../core/services/ventas.service';
import { CajaService } from '../../core/services/caja.service';
import { ProductosService } from '../../core/services/productos.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';
import { ClienteAutocompleteComponent } from '../../shared/components/cliente-autocomplete/cliente-autocomplete.component';
import { CustomValidators } from '../../core/validators/custom-validators';

@Component({
  selector: 'app-ventas',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, MatIconModule,
    MatTableModule, MatChipsModule, MatDialogModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatTooltipModule, FormatoMonedaPipe, FormatoFechaPipe
  ],
  template: `
    <div class="ventas-container">
      <div class="page-header">
        <h1>Gestión de Ventas</h1>
        <p class="subtitle">Registra ventas de productos a compradores</p>
      </div>

      <div class="actions-bar">
        <button mat-raised-button color="primary" (click)="abrirDialogoRegistrar()" [disabled]="!cajaAbierta">
          <mat-icon>add</mat-icon> Nueva Venta
        </button>
        <mat-chip-set *ngIf="!cajaAbierta">
          <mat-chip class="warning-chip">
            <mat-icon>warning</mat-icon>
            Caja cerrada
          </mat-chip>
        </mat-chip-set>
      </div>

      <mat-card class="filters-card">
        <mat-card-content>
          <form [formGroup]="filtrosForm" class="filters-form">
            <mat-form-field appearance="outline">
              <mat-label>Producto</mat-label>
              <mat-select formControlName="productoId">
                <mat-option [value]="null">Todos</mat-option>
                <mat-option *ngFor="let producto of productos" [value]="producto.id">
                  {{ producto.nombre }}
                </mat-option>
              </mat-select>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Fecha Inicio</mat-label>
              <input matInput type="date" formControlName="fechaInicio">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Fecha Fin</mat-label>
              <input matInput type="date" formControlName="fechaFin">
            </mat-form-field>
            <button mat-raised-button color="accent" (click)="aplicarFiltros()">
              <mat-icon>filter_list</mat-icon> Filtrar
            </button>
            <button mat-button (click)="limpiarFiltros()">
              <mat-icon>clear</mat-icon> Limpiar
            </button>
          </form>
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-header>
          <mat-card-title>Ventas Registradas</mat-card-title>
          <button mat-icon-button (click)="cargarVentas()" matTooltip="Actualizar">
            <mat-icon>refresh</mat-icon>
          </button>
        </mat-card-header>
        <mat-card-content>
          <div class="table-container">
            <table mat-table [dataSource]="ventas" class="ventas-table">
              <ng-container matColumnDef="fecha">
                <th mat-header-cell *matHeaderCellDef>Fecha</th>
                <td mat-cell *matCellDef="let venta">{{ venta.fechaVenta | formatoFecha: true }}</td>
              </ng-container>
              <ng-container matColumnDef="cliente">
                <th mat-header-cell *matHeaderCellDef>Comprador</th>
                <td mat-cell *matCellDef="let venta">{{ venta.clienteNombre }}</td>
              </ng-container>
              <ng-container matColumnDef="producto">
                <th mat-header-cell *matHeaderCellDef>Producto</th>
                <td mat-cell *matCellDef="let venta">
                  <mat-chip>{{ venta.productoNombre }}</mat-chip>
                </td>
              </ng-container>
              <ng-container matColumnDef="pesoNeto">
                <th mat-header-cell *matHeaderCellDef class="text-right">Peso Neto</th>
                <td mat-cell *matCellDef="let venta" class="text-right">{{ venta.pesoNeto }} kg</td>
              </ng-container>
              <ng-container matColumnDef="precio">
                <th mat-header-cell *matHeaderCellDef class="text-right">Precio/Kg</th>
                <td mat-cell *matCellDef="let venta" class="text-right">{{ venta.precioPorKg | formatoMoneda }}</td>
              </ng-container>
              <ng-container matColumnDef="monto">
                <th mat-header-cell *matHeaderCellDef class="text-right">Monto Total</th>
                <td mat-cell *matCellDef="let venta" class="text-right">
                  <span class="monto-total">{{ venta.montoTotal | formatoMoneda }}</span>
                </td>
              </ng-container>
              <ng-container matColumnDef="estado">
                <th mat-header-cell *matHeaderCellDef>Estado</th>
                <td mat-cell *matCellDef="let venta">
                  <mat-icon *ngIf="venta.editada" class="edit-icon" matTooltip="Editada">edit</mat-icon>
                </td>
              </ng-container>
              <ng-container matColumnDef="acciones">
                <th mat-header-cell *matHeaderCellDef>Acciones</th>
                <td mat-cell *matCellDef="let venta">
                  <button mat-icon-button (click)="editarVenta(venta)" *ngIf="puedeEditar(venta)" matTooltip="Editar">
                    <mat-icon>edit</mat-icon>
                  </button>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="columnasVentas"></tr>
              <tr mat-row *matRowDef="let row; columns: columnasVentas;"></tr>
              <tr class="mat-row" *matNoDataRow>
                <td class="mat-cell" [attr.colspan]="columnasVentas.length">
                  <div class="empty-state"><mat-icon>shopping_bag</mat-icon><p>No hay ventas registradas</p></div>
                </td>
              </tr>
            </table>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .ventas-container { padding: 24px; max-width: 1600px; margin: 0 auto; }
    .page-header h1 { font-size: 32px; font-weight: 600; color: #1a202c; margin: 0 0 8px 0; }
    .subtitle { color: #718096; font-size: 14px; margin: 0; }
    .actions-bar { display: flex; align-items: center; gap: 16px; margin: 24px 0; }
    .warning-chip { background-color: #fef5e7 !important; color: #975a16 !important; }
    .filters-card { margin-bottom: 24px; border-radius: 12px; }
    .filters-form { display: flex; gap: 16px; align-items: center; flex-wrap: wrap; }
    .filters-form mat-form-field { min-width: 200px; }
    mat-card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    mat-card-title { font-size: 20px; font-weight: 600; color: #2d3748; margin: 0; }
    .table-container { overflow-x: auto; }
    .ventas-table { width: 100%; }
    .ventas-table th { background-color: #f7fafc; font-weight: 600; color: #2d3748; padding: 16px; }
    .ventas-table td { padding: 14px; color: #4a5568; border-bottom: 1px solid #edf2f7; }
    .text-right { text-align: right; }
    .monto-total { font-weight: 600; color: #2d3748; font-size: 15px; }
    .edit-icon { color: #ed8936; font-size: 20px; width: 20px; height: 20px; }
    .empty-state { text-align: center; padding: 48px; }
    .empty-state mat-icon { font-size: 64px; width: 64px; height: 64px; color: #cbd5e0; }
    .empty-state p { color: #a0aec0; font-size: 16px; }
  `]
})
export class VentasComponent implements OnInit {
  ventas: any[] = [];
  productos: any[] = [];
  cajaAbierta: boolean = false;
  filtrosForm: FormGroup;
  columnasVentas: string[] = ['fecha', 'cliente', 'producto', 'pesoNeto', 'precio', 'monto', 'estado', 'acciones'];

  constructor(
    private ventasService: VentasService,
    private cajaService: CajaService,
    private productosService: ProductosService,
    private dialog: MatDialog,
    private fb: FormBuilder
  ) {
    this.filtrosForm = this.fb.group({ productoId: [null], fechaInicio: [''], fechaFin: [''] });
  }

  ngOnInit(): void {
    this.verificarCaja();
    this.cargarProductos();
    this.cargarVentas();
  }

  verificarCaja(): void {
    this.cajaService.obtenerCajaActual().subscribe({
      next: (data) => this.cajaAbierta = data && data.id && data.estado === 0,
      error: () => this.cajaAbierta = false
    });
  }

  cargarProductos(): void {
    this.productosService.obtenerTodos().subscribe({
      next: (data) => this.productos = data,
      error: (e) => console.error('Error:', e)
    });
  }

  cargarVentas(): void {
    const filtros = this.construirFiltros();
    this.ventasService.obtenerTodas(1, 50, filtros).subscribe({
      next: (response) => this.ventas = response.data || response.items || response || [],
      error: (e) => console.error('Error:', e)
    });
  }

  construirFiltros(): any {
    const v = this.filtrosForm.value;
    const f: any = {};
    if (v.productoId) f.productoId = v.productoId;
    if (v.fechaInicio) f.fechaInicio = v.fechaInicio;
    if (v.fechaFin) f.fechaFin = v.fechaFin;
    return f;
  }

  aplicarFiltros(): void { this.cargarVentas(); }
  limpiarFiltros(): void { this.filtrosForm.reset({ productoId: null, fechaInicio: '', fechaFin: '' }); this.aplicarFiltros(); }

  abrirDialogoRegistrar(): void {
    const dialogRef = this.dialog.open(RegistrarVentaDialogComponent, {
      width: '700px',
      disableClose: true,
      data: { productos: this.productos }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.ventasService.registrarVenta(result).subscribe({
          next: () => this.cargarVentas(),
          error: (e) => console.error('Error:', e)
        });
      }
    });
  }

  editarVenta(venta: any): void {
    const dialogRef = this.dialog.open(RegistrarVentaDialogComponent, {
      width: '700px',
      disableClose: true,
      data: { productos: this.productos, venta: venta }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.ventasService.editarVenta(venta.id, result).subscribe({
          next: () => this.cargarVentas(),
          error: (e) => console.error('Error:', e)
        });
      }
    });
  }

  puedeEditar(venta: any): boolean {
    if (!venta || !venta.fechaVenta) return false;
    const fechaVenta = new Date(venta.fechaVenta);
    const hoy = new Date();
    return fechaVenta.toDateString() === hoy.toDateString();
  }
}

@Component({
  selector: 'app-registrar-venta-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule,
    ClienteAutocompleteComponent, FormatoMonedaPipe
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>{{ esEdicion ? 'edit' : 'point_of_sale' }}</mat-icon>
      {{ esEdicion ? 'Editar Venta' : 'Registrar Nueva Venta' }}
    </h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="venta-form">
        <div class="form-section">
          <h3>Comprador</h3>
          <app-cliente-autocomplete
            formControlName="clienteCompradorId"
            [tipoCliente]="'comprador'"
            placeholder="Buscar comprador">
          </app-cliente-autocomplete>
        </div>

        <div class="form-section">
          <h3>Producto</h3>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Producto</mat-label>
            <mat-select formControlName="productoId" (selectionChange)="onProductoChange()">
              <mat-option *ngFor="let producto of productos" [value]="producto.id">
                {{ producto.nombre }}
              </mat-option>
            </mat-select>
            <mat-error>Seleccione un producto</mat-error>
          </mat-form-field>
        </div>

        <div class="form-section">
          <h3>Pesado</h3>
          <div class="peso-row">
            <mat-form-field appearance="outline">
              <mat-label>Peso Bruto</mat-label>
              <input matInput type="number" step="0.1" formControlName="pesoBruto" placeholder="0.0" (input)="calcular()">
              <span matSuffix>kg</span>
              <mat-error>Requerido</mat-error>
            </mat-form-field>
            <div class="peso-neto-display">
              <span class="label">Peso Neto:</span>
              <span class="value">{{ form.get('pesoBruto')?.value || 0 | number:'1.1-1' }} kg</span>
            </div>
          </div>
        </div>

        <div class="form-section">
          <h3>Precio</h3>
          <div class="precio-row">
            <mat-form-field appearance="outline">
              <mat-label>Precio por Kg</mat-label>
              <input matInput type="number" step="0.01" formControlName="precioPorKg" placeholder="0.00" (input)="calcular()">
              <span matPrefix>S/&nbsp;</span>
              <mat-error>Requerido</mat-error>
            </mat-form-field>
            <div class="monto-total-display">
              <span class="label">Monto Total:</span>
              <span class="value">{{ montoTotal | formatoMoneda }}</span>
            </div>
          </div>
        </div>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancelar</button>
      <button mat-raised-button color="primary" (click)="onConfirm()" [disabled]="!form.valid">
        {{ esEdicion ? 'Guardar' : 'Registrar' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 12px; color: #2d3748; }
    mat-dialog-content { padding: 20px 24px; }
    .venta-form { display: flex; flex-direction: column; gap: 20px; }
    .form-section { border: 1px solid #e2e8f0; padding: 16px; border-radius: 8px; background: #f7fafc; }
    .form-section h3 { margin: 0 0 16px 0; font-size: 16px; font-weight: 600; color: #2d3748; border-bottom: 2px solid #cbd5e0; padding-bottom: 8px; }
    .full-width { width: 100%; }
    .peso-row, .precio-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .peso-neto-display, .monto-total-display { background: #edf2f7; padding: 16px; border-radius: 8px; display: flex; flex-direction: column; gap: 4px; }
    .peso-neto-display .label, .monto-total-display .label { font-size: 11px; font-weight: 600; color: #718096; text-transform: uppercase; }
    .peso-neto-display .value, .monto-total-display .value { font-size: 20px; font-weight: 700; color: #2b6cb0; }
  `]
})
export class RegistrarVentaDialogComponent implements OnInit {
  form: FormGroup;
  productos: any[] = [];
  esEdicion: boolean = false;
  montoTotal: number = 0;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<RegistrarVentaDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.productos = data.productos || [];
    this.esEdicion = !!data.venta;
    this.form = this.fb.group({
      clienteCompradorId: ['', Validators.required],
      productoId: ['', Validators.required],
      pesoBruto: ['', [Validators.required, CustomValidators.peso()]],
      precioPorKg: ['', [Validators.required, CustomValidators.monto()]]
    });

    if (this.esEdicion) {
      this.form.patchValue(data.venta);
      this.calcular();
    }
  }

  ngOnInit(): void {}

  onProductoChange(): void {
    const productoId = this.form.get('productoId')?.value;
    const producto = this.productos.find(p => p.id === productoId);
    if (producto && producto.precioVenta > 0 && !this.esEdicion) {
      this.form.patchValue({ precioPorKg: producto.precioVenta });
      this.calcular();
    }
  }

  calcular(): void {
    const peso = parseFloat(this.form.get('pesoBruto')?.value) || 0;
    const precio = parseFloat(this.form.get('precioPorKg')?.value) || 0;
    this.montoTotal = peso * precio;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.form.valid && this.montoTotal > 0) {
      const data = {
        ...this.form.value,
        pesoNeto: parseFloat(this.form.get('pesoBruto')?.value),
        montoTotal: this.montoTotal
      };
      this.dialogRef.close(data);
    }
  }
}
