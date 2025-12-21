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
import { MatRadioModule } from '@angular/material/radio';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ComprasService } from '../../core/services/compras.service';
import { CajaService } from '../../core/services/caja.service';
import { ProductosService } from '../../core/services/productos.service';
import { ClientesService } from '../../core/services/clientes.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';
import { NumeroVoucherPipe } from '../../shared/pipes/numero-voucher.pipe';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { ClienteAutocompleteComponent } from '../../shared/components/cliente-autocomplete/cliente-autocomplete.component';
import { TipoPesado } from '../../core/models/enums';
import { CustomValidators } from '../../core/validators/custom-validators';

@Component({
  selector: 'app-compras',
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
    FormatoFechaPipe,
    NumeroVoucherPipe
  ],
  templateUrl: './compras.component.html',
  styleUrls: ['./compras.component.css']
})
export class ComprasComponent implements OnInit {
  compras: any[] = [];
  productos: any[] = [];
  cajaAbierta: boolean = false;
  filtrosForm: FormGroup;

  columnasCompras: string[] = ['voucher', 'fecha', 'cliente', 'productos', 'pesoTotal', 'monto', 'estado', 'acciones'];

  paginaActual: number = 1;
  pageSize: number = 20;
  totalItems: number = 0;
  totalPaginas: number = 0;

  alertMessage: string = '';
  alertType: 'success' | 'error' | 'warning' | 'info' = 'info';
  showAlert: boolean = false;

  constructor(
    private comprasService: ComprasService,
    private cajaService: CajaService,
    private productosService: ProductosService,
    private dialog: MatDialog,
    private fb: FormBuilder
  ) {
    this.filtrosForm = this.fb.group({
      productoId: [null],
      fechaInicio: [''],
      fechaFin: ['']
    });
  }

  ngOnInit(): void {
    this.verificarCaja();
    this.cargarProductos();
    this.cargarCompras();
  }

  verificarCaja(): void {
    this.cajaService.obtenerCajaActual().subscribe({
      next: (data) => {
        this.cajaAbierta = data && data.id && data.estado === 0;
      },
      error: () => {
        this.cajaAbierta = false;
      }
    });
  }

  cargarProductos(): void {
    this.productosService.obtenerTodos().subscribe({
      next: (data) => {
        this.productos = data;
      },
      error: (error) => {
        console.error('Error al cargar productos:', error);
      }
    });
  }

  cargarCompras(): void {
    const filtros = this.construirFiltros();

    this.comprasService.obtenerTodas(this.paginaActual, this.pageSize, filtros).subscribe({
      next: (response) => {
        this.compras = response.data || response.items || response || [];
        this.totalItems = response.total || this.compras.length;
        this.totalPaginas = Math.ceil(this.totalItems / this.pageSize);
      },
      error: (error) => {
        console.error('Error al cargar compras:', error);
        this.mostrarAlerta('Error al cargar las compras', 'error');
      }
    });
  }

  construirFiltros(): any {
    const valores = this.filtrosForm.value;
    const filtros: any = {};

    if (valores.productoId) filtros.productoId = valores.productoId;
    if (valores.fechaInicio) filtros.fechaInicio = valores.fechaInicio;
    if (valores.fechaFin) filtros.fechaFin = valores.fechaFin;

    return filtros;
  }

  aplicarFiltros(): void {
    this.paginaActual = 1;
    this.cargarCompras();
  }

  limpiarFiltros(): void {
    this.filtrosForm.reset({
      productoId: null,
      fechaInicio: '',
      fechaFin: ''
    });
    this.aplicarFiltros();
  }

  cambiarPagina(pagina: number): void {
    if (pagina >= 1 && pagina <= this.totalPaginas) {
      this.paginaActual = pagina;
      this.cargarCompras();
    }
  }

  abrirDialogoRegistrar(): void {
    const dialogRef = this.dialog.open(RegistrarCompraDialogComponent, {
      width: '800px',
      maxHeight: '90vh',
      disableClose: true,
      data: { productos: this.productos }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.comprasService.registrarCompra(result).subscribe({
          next: () => {
            this.mostrarAlerta('Compra registrada exitosamente', 'success');
            this.cargarCompras();
          },
          error: (error) => {
            this.mostrarAlerta('Error al registrar la compra: ' + (error.error?.message || error.message), 'error');
          }
        });
      }
    });
  }

  verDetalle(compraId: number): void {
    this.comprasService.obtenerPorId(compraId).subscribe({
      next: (data) => {
        this.dialog.open(DetalleCompraDialogComponent, {
          width: '700px',
          data: data
        });
      },
      error: (error) => {
        this.mostrarAlerta('Error al cargar el detalle de la compra', 'error');
      }
    });
  }

  reimprimirVoucher(compraId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Reimprimir Voucher',
        message: '¿Desea reimprimir el voucher de esta compra?',
        confirmText: 'Reimprimir',
        cancelText: 'Cancelar',
        type: 'info'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.comprasService.reimprimirVoucher(compraId).subscribe({
          next: () => {
            this.mostrarAlerta('Voucher enviado a impresión', 'success');
          },
          error: (error) => {
            this.mostrarAlerta('Error al reimprimir voucher: ' + (error.error?.message || error.message), 'error');
          }
        });
      }
    });
  }

  mostrarAlerta(mensaje: string, tipo: 'success' | 'error' | 'warning' | 'info'): void {
    this.alertMessage = mensaje;
    this.alertType = tipo;
    this.showAlert = true;
  }
}

// ==================== DIALOG: REGISTRAR/EDITAR COMPRA ====================

@Component({
  selector: 'app-registrar-compra-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatRadioModule,
    MatTableModule,
    MatProgressBarModule,
    ClienteAutocompleteComponent,
    FormatoMonedaPipe
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>add_shopping_cart</mat-icon>
      Registrar Nueva Compra
    </h2>
    <mat-dialog-content>
      <!-- SECCIÓN 1: Información del Proveedor -->
      <div class="form-section">
        <h3><mat-icon>person</mat-icon> Información del Proveedor</h3>
        <form [formGroup]="generalForm">
          <app-cliente-autocomplete
            formControlName="clienteProveedorId"
            [tipoCliente]="'proveedor'"
            placeholder="Buscar por DNI o nombre"
            (searchTextChange)="onSearchTextChange($event)">
          </app-cliente-autocomplete>

          <!-- Botón Consultar RENIEC -->
          <div class="reniec-section" *ngIf="mostrarBotonReniec">
            <button
              mat-raised-button
              type="button"
              class="btn-reniec"
              (click)="consultarReniec()"
              [disabled]="consultandoReniec">
              <mat-icon>cloud_download</mat-icon>
              CONSULTAR RENIEC
            </button>
          </div>

          <!-- Barra de progreso mientras consulta -->
          <mat-progress-bar *ngIf="consultandoReniec" mode="indeterminate"></mat-progress-bar>

          <!-- Campo de nombre (aparece después de consultar RENIEC) -->
          <div class="nombre-section" *ngIf="mostrarCampoNombre">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Nombre Completo del Nuevo Cliente</mat-label>
              <input
                matInput
                formControlName="nuevoClienteNombre"
                [readonly]="!nombreEditable"
                placeholder="Ingrese el nombre completo">
              <mat-icon matSuffix [class.success-icon]="reniecExitoso" [class.manual-icon]="!reniecExitoso">
                {{ reniecExitoso ? 'check_circle' : 'edit' }}
              </mat-icon>
              <mat-hint [class.manual-hint]="!reniecExitoso" [class.success-hint]="reniecExitoso">
                {{ reniecExitoso ? 'Datos obtenidos de RENIEC' : 'Registro manual de cliente' }}
              </mat-hint>
            </mat-form-field>
          </div>
        </form>
      </div>

      <!-- SECCIÓN 2: Agregar Productos -->
      <div class="form-section agregar-producto">
        <h3><mat-icon>add_circle</mat-icon> Agregar Productos a la Compra</h3>
        <form [formGroup]="productoForm" class="producto-form-grid">
          <!-- Selección de Producto -->
          <mat-form-field appearance="outline" class="producto-select">
            <mat-label>Producto</mat-label>
            <mat-select formControlName="productoId" (selectionChange)="onProductoChange()">
              <mat-option *ngFor="let producto of productos" [value]="producto.id">
                {{ producto.nombre }}
              </mat-option>
            </mat-select>
            <mat-error *ngIf="productoForm.get('productoId')?.hasError('required')">
              Seleccione un producto
            </mat-error>
          </mat-form-field>

          <!-- Nivel de Secado y Calidad -->
          <mat-form-field appearance="outline" *ngIf="productoSeleccionado">
            <mat-label>Nivel de Secado</mat-label>
            <mat-select formControlName="nivelSecado">
              <mat-option *ngFor="let nivel of nivelesSecado" [value]="nivel">
                {{ nivel }}
              </mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" *ngIf="productoSeleccionado">
            <mat-label>Calidad</mat-label>
            <mat-select formControlName="calidad">
              <mat-option *ngFor="let cal of calidades" [value]="cal">
                {{ cal }}
              </mat-option>
            </mat-select>
          </mat-form-field>

          <!-- Tipo de Pesado -->
          <div class="tipo-pesado" *ngIf="productoSeleccionado">
            <label>Tipo Pesado:</label>
            <mat-radio-group formControlName="tipoPesado">
              <mat-radio-button [value]="0">Kg</mat-radio-button>
              <mat-radio-button [value]="1" [disabled]="!permiteValdeo">Valdeo</mat-radio-button>
            </mat-radio-group>
          </div>

          <!-- Peso Bruto, Descuento, Precio -->
          <mat-form-field appearance="outline" *ngIf="productoSeleccionado">
            <mat-label>Peso Bruto</mat-label>
            <input matInput type="number" step="0.1" formControlName="pesoBruto" (input)="calcularPesoNetoProducto()">
            <span matSuffix>kg</span>
          </mat-form-field>

          <mat-form-field appearance="outline" *ngIf="productoSeleccionado">
            <mat-label>Descuento</mat-label>
            <input matInput type="number" step="0.1" formControlName="descuentoKg" (input)="calcularPesoNetoProducto()">
            <span matSuffix>kg</span>
          </mat-form-field>

          <mat-form-field appearance="outline" *ngIf="productoSeleccionado">
            <mat-label>Precio/Kg</mat-label>
            <input matInput type="number" step="0.01" formControlName="precioPorKg" (input)="calcularSubtotalProducto()">
            <span matPrefix>S/&nbsp;</span>
          </mat-form-field>

          <!-- Resumen del producto -->
          <div class="resumen-producto" *ngIf="productoSeleccionado">
            <div class="resumen-item">
              <span class="label">Peso Neto:</span>
              <span class="value">{{ pesoNetoProducto | number:'1.1-1' }} kg</span>
            </div>
            <div class="resumen-item">
              <span class="label">Subtotal:</span>
              <span class="value money">{{ subtotalProducto | formatoMoneda }}</span>
            </div>
          </div>

          <!-- Botón Agregar -->
          <div class="boton-agregar">
            <button mat-raised-button color="accent" type="button" (click)="agregarProducto()"
                    [disabled]="!productoForm.valid || pesoNetoProducto <= 0">
              <mat-icon>add</mat-icon> Agregar Producto
            </button>
          </div>
        </form>
      </div>

      <!-- SECCIÓN 3: Tabla de Productos Agregados -->
      <div class="form-section tabla-productos" *ngIf="productosAgregados.length > 0">
        <h3><mat-icon>list</mat-icon> Productos en la Compra ({{ productosAgregados.length }})</h3>
        <table mat-table [dataSource]="productosAgregados" class="productos-table">
          <ng-container matColumnDef="producto">
            <th mat-header-cell *matHeaderCellDef>Producto</th>
            <td mat-cell *matCellDef="let detalle">{{ detalle.productoNombre }}</td>
          </ng-container>

          <ng-container matColumnDef="caracteristicas">
            <th mat-header-cell *matHeaderCellDef>Características</th>
            <td mat-cell *matCellDef="let detalle">
              {{ detalle.nivelSecado }} / {{ detalle.calidad }}
            </td>
          </ng-container>

          <ng-container matColumnDef="pesoNeto">
            <th mat-header-cell *matHeaderCellDef>Peso Neto</th>
            <td mat-cell *matCellDef="let detalle">{{ detalle.pesoNeto | number:'1.1-1' }} kg</td>
          </ng-container>

          <ng-container matColumnDef="precio">
            <th mat-header-cell *matHeaderCellDef>Precio/Kg</th>
            <td mat-cell *matCellDef="let detalle">{{ detalle.precioPorKg | formatoMoneda }}</td>
          </ng-container>

          <ng-container matColumnDef="subtotal">
            <th mat-header-cell *matHeaderCellDef>Subtotal</th>
            <td mat-cell *matCellDef="let detalle">{{ detalle.subtotal | formatoMoneda }}</td>
          </ng-container>

          <ng-container matColumnDef="acciones">
            <th mat-header-cell *matHeaderCellDef>Acciones</th>
            <td mat-cell *matCellDef="let detalle; let i = index">
              <button mat-icon-button color="warn" (click)="eliminarProducto(i)" matTooltip="Eliminar">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="columnasProductos"></tr>
          <tr mat-row *matRowDef="let row; columns: columnasProductos;"></tr>
        </table>

        <!-- TOTALES -->
        <div class="totales-compra">
          <div class="total-item">
            <span class="label">PESO TOTAL:</span>
            <span class="value">{{ pesoTotalCompra | number:'1.1-1' }} kg</span>
          </div>
          <div class="total-item final">
            <span class="label">MONTO TOTAL:</span>
            <span class="value">{{ montoTotalCompra | formatoMoneda }}</span>
          </div>
        </div>
      </div>

      <!-- Mensaje si no hay productos -->
      <div class="empty-state" *ngIf="productosAgregados.length === 0">
        <mat-icon>inventory_2</mat-icon>
        <p>No hay productos agregados. Use el formulario de arriba para agregar productos a la compra.</p>
      </div>

      <div class="info-box">
        <mat-icon>info</mat-icon>
        <p>El voucher con todos los productos se imprimirá automáticamente al registrar la compra.</p>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancelar</button>
      <button mat-raised-button color="primary" (click)="onConfirm()"
              [disabled]="!generalForm.valid || productosAgregados.length === 0">
        <mat-icon>save</mat-icon> Registrar Compra ({{ productosAgregados.length }} productos)
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 12px;
      color: #2d3748;
    }
    mat-dialog-content {
      padding: 20px 24px;
      max-height: 80vh;
      overflow-y: auto;
    }
    .form-section {
      border: 1px solid #e2e8f0;
      padding: 20px;
      border-radius: 8px;
      background: #f7fafc;
      margin-bottom: 20px;
    }
    .form-section h3 {
      margin: 0 0 16px 0;
      font-size: 16px;
      font-weight: 600;
      color: #2d3748;
      display: flex;
      align-items: center;
      gap: 8px;
      border-bottom: 2px solid #cbd5e0;
      padding-bottom: 8px;
    }
    .form-section h3 mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }
    .agregar-producto {
      background: #fff7ed;
      border-color: #fed7aa;
    }
    .producto-form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
    }
    .producto-select {
      grid-column: 1 / -1;
    }
    .tipo-pesado {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .tipo-pesado label {
      font-size: 12px;
      color: #718096;
      font-weight: 500;
    }
    .tipo-pesado mat-radio-group {
      display: flex;
      gap: 12px;
    }
    .resumen-producto {
      grid-column: 1 / -1;
      display: flex;
      gap: 24px;
      background: #edf2f7;
      padding: 16px;
      border-radius: 8px;
    }
    .resumen-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .resumen-item .label {
      font-size: 11px;
      font-weight: 600;
      color: #718096;
      text-transform: uppercase;
    }
    .resumen-item .value {
      font-size: 18px;
      font-weight: 700;
      color: #2b6cb0;
    }
    .resumen-item .value.money {
      color: #2f855a;
    }
    .boton-agregar {
      grid-column: 1 / -1;
      display: flex;
      justify-content: center;
    }
    .boton-agregar button {
      padding: 8px 32px;
      font-size: 16px;
    }
    .tabla-productos {
      background: #f0fdf4;
      border-color: #86efac;
    }
    .productos-table {
      width: 100%;
      background: white;
    }
    .productos-table th {
      background: #f7fafc;
      font-weight: 600;
      color: #2d3748;
    }
    .totales-compra {
      display: flex;
      justify-content: flex-end;
      gap: 32px;
      margin-top: 16px;
      padding: 16px;
      background: white;
      border-radius: 8px;
      border: 2px solid #cbd5e0;
    }
    .total-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
      text-align: right;
    }
    .total-item .label {
      font-size: 12px;
      font-weight: 600;
      color: #718096;
      text-transform: uppercase;
    }
    .total-item .value {
      font-size: 20px;
      font-weight: 700;
      color: #2b6cb0;
    }
    .total-item.final .value {
      font-size: 24px;
      color: #2f855a;
    }
    .empty-state {
      text-align: center;
      padding: 40px;
      color: #718096;
    }
    .empty-state mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #cbd5e0;
    }
    .info-box {
      display: flex;
      align-items: center;
      gap: 12px;
      background: #ebf8ff;
      padding: 12px 16px;
      border-radius: 8px;
      margin-top: 16px;
    }
    .info-box mat-icon {
      color: #3182ce;
      font-size: 24px;
      width: 24px;
      height: 24px;
    }
    .info-box p {
      margin: 0;
      color: #2c5282;
      font-size: 13px;
    }
    mat-dialog-actions {
      padding: 16px 24px;
    }
    .reniec-section {
      margin-top: 16px;
      display: flex;
      justify-content: center;
    }
    .btn-reniec {
      background-color: #9f7aea !important;
      color: white !important;
      font-weight: 600;
      padding: 8px 24px;
      font-size: 14px;
    }
    .btn-reniec:hover:not([disabled]) {
      background-color: #805ad5 !important;
    }
    .btn-reniec[disabled] {
      background-color: #d6bcfa !important;
    }
    .nombre-section {
      margin-top: 16px;
      display: flex;
      flex-direction: column;
      gap: 12px;
    }
    .full-width {
      width: 100%;
    }
    .success-icon {
      color: #38a169 !important;
    }
    .manual-icon {
      color: #ed8936 !important;
    }
    mat-progress-bar {
      margin-top: 12px;
    }
    .divider-text {
      text-align: center;
      margin: 20px 0 16px 0;
      position: relative;
    }
    .divider-text span {
      background: #f7fafc;
      padding: 0 12px;
      color: #718096;
      font-size: 13px;
      font-weight: 500;
      position: relative;
      z-index: 1;
    }
    .divider-text::before {
      content: '';
      position: absolute;
      left: 0;
      right: 0;
      top: 50%;
      height: 1px;
      background: #cbd5e0;
      z-index: 0;
    }
    .manual-hint {
      color: #ed8936 !important;
      font-weight: 500 !important;
    }
    .success-hint {
      color: #38a169 !important;
      font-weight: 500 !important;
    }
  `]
})
export class RegistrarCompraDialogComponent implements OnInit {
  generalForm: FormGroup;
  productoForm: FormGroup;
  productos: any[] = [];

  // Producto actual
  productoSeleccionado: any = null;
  nivelesSecado: string[] = [];
  calidades: string[] = [];
  permiteValdeo: boolean = false;
  pesoNetoProducto: number = 0;
  subtotalProducto: number = 0;

  // Productos agregados a la compra
  productosAgregados: any[] = [];
  columnasProductos: string[] = ['producto', 'caracteristicas', 'pesoNeto', 'precio', 'subtotal', 'acciones'];

  // Totales generales
  pesoTotalCompra: number = 0;
  montoTotalCompra: number = 0;

  // Estados para RENIEC
  mostrarBotonReniec: boolean = false;
  mostrarCampoNombre: boolean = false;
  consultandoReniec: boolean = false;
  reniecExitoso: boolean = false;
  nombreEditable: boolean = false;
  dniIngresado: string = '';

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<RegistrarCompraDialogComponent>,
    private clientesService: ClientesService,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.productos = data.productos || [];

    // Form para datos generales (cliente + datos para nuevo cliente)
    this.generalForm = this.fb.group({
      clienteProveedorId: [''],
      nuevoClienteDni: [''],
      nuevoClienteNombre: ['']
    });

    // Form para agregar productos
    this.productoForm = this.fb.group({
      productoId: ['', Validators.required],
      nivelSecado: ['', Validators.required],
      calidad: ['', Validators.required],
      tipoPesado: [0, Validators.required],
      pesoBruto: ['', [Validators.required, CustomValidators.peso()]],
      descuentoKg: [0, [Validators.min(0)]],
      precioPorKg: ['', [Validators.required, CustomValidators.monto()]]
    });
  }

  ngOnInit(): void {
    // Escuchar cambios en el campo de clienteProveedorId
    this.generalForm.get('clienteProveedorId')?.valueChanges.subscribe(value => {
      // Si se selecciona un cliente (valor numérico), ocultar flujo RENIEC
      if (value && typeof value === 'number') {
        this.mostrarBotonReniec = false;
        this.mostrarCampoNombre = false;
        this.dniIngresado = '';
        this.generalForm.patchValue({
          nuevoClienteDni: '',
          nuevoClienteNombre: ''
        }, { emitEvent: false });
      }
    });
  }

  onSearchTextChange(searchText: string): void {
    const clienteId = this.generalForm.get('clienteProveedorId')?.value;

    // Solo procesar si no hay cliente seleccionado
    if (clienteId && typeof clienteId === 'number') {
      this.mostrarBotonReniec = false;
      this.dniIngresado = '';
      return;
    }

    // Verificar si es un DNI de 8 dígitos
    const esDniValido = searchText.length === 8 && /^\d{8}$/.test(searchText);

    if (esDniValido) {
      this.dniIngresado = searchText;
      this.mostrarBotonReniec = true;

      // Ocultar campo de nombre si cambia el DNI
      if (this.mostrarCampoNombre) {
        this.mostrarCampoNombre = false;
        this.generalForm.patchValue({ nuevoClienteNombre: '' }, { emitEvent: false });
      }
    } else {
      this.dniIngresado = '';
      this.mostrarBotonReniec = false;
    }
  }

  consultarReniec(): void {
    if (!this.dniIngresado || this.dniIngresado.length !== 8) {
      return;
    }

    this.consultandoReniec = true;
    this.mostrarCampoNombre = false;

    this.clientesService.consultarReniec(this.dniIngresado).subscribe({
      next: (response) => {
        this.consultandoReniec = false;

        if (response.success && response.nombreCompleto) {
          // RENIEC exitoso - nombre no editable
          this.reniecExitoso = true;
          this.nombreEditable = false;
          this.mostrarCampoNombre = true;
          this.generalForm.patchValue({
            nuevoClienteDni: this.dniIngresado,
            nuevoClienteNombre: response.nombreCompleto
          }, { emitEvent: false });
        } else {
          // RENIEC falló - permitir ingreso manual
          this.reniecExitoso = false;
          this.nombreEditable = true;
          this.mostrarCampoNombre = true;
          console.log('RENIEC falló - Variables:', {
            reniecExitoso: this.reniecExitoso,
            nombreEditable: this.nombreEditable,
            mostrarCampoNombre: this.mostrarCampoNombre
          });
          this.generalForm.patchValue({
            nuevoClienteDni: this.dniIngresado,
            nuevoClienteNombre: ''
          }, { emitEvent: false });
        }
      },
      error: (error) => {
        this.consultandoReniec = false;
        // Error en la consulta - permitir ingreso manual
        this.reniecExitoso = false;
        this.nombreEditable = true;
        this.mostrarCampoNombre = true;
        this.generalForm.patchValue({
          nuevoClienteDni: this.dniIngresado,
          nuevoClienteNombre: ''
        }, { emitEvent: false });
        console.error('Error al consultar RENIEC:', error);
      }
    });
  }

  onProductoChange(): void {
    const productoId = this.productoForm.get('productoId')?.value;
    this.productoSeleccionado = this.productos.find(p => p.id === productoId);

    if (this.productoSeleccionado) {
      this.nivelesSecado = this.productoSeleccionado.nivelesSecado || [];
      this.calidades = this.productoSeleccionado.calidades || [];
      this.permiteValdeo = this.productoSeleccionado.permiteValdeo || false;

      // Reset caracteristicas cuando cambia el producto
      this.productoForm.patchValue({
        nivelSecado: this.nivelesSecado.length > 0 ? this.nivelesSecado[0] : '',
        calidad: this.calidades.length > 0 ? this.calidades[0] : '',
        tipoPesado: 0,
        pesoBruto: '',
        descuentoKg: 0,
        precioPorKg: this.productoSeleccionado.precioSugeridoPorKg || ''
      });

      this.calcularPesoNetoProducto();
    }
  }

  calcularPesoNetoProducto(): void {
    const pesoBruto = parseFloat(this.productoForm.get('pesoBruto')?.value) || 0;
    const descuento = parseFloat(this.productoForm.get('descuentoKg')?.value) || 0;
    this.pesoNetoProducto = Math.max(0, pesoBruto - descuento);
    this.calcularSubtotalProducto();
  }

  calcularSubtotalProducto(): void {
    const precioPorKg = parseFloat(this.productoForm.get('precioPorKg')?.value) || 0;
    this.subtotalProducto = this.pesoNetoProducto * precioPorKg;
  }

  agregarProducto(): void {
    if (this.productoForm.valid && this.pesoNetoProducto > 0 && this.subtotalProducto > 0) {
      const formValue = this.productoForm.value;

      const detalleProducto = {
        productoId: formValue.productoId,
        productoNombre: this.productoSeleccionado.nombre,
        nivelSecado: formValue.nivelSecado,
        calidad: formValue.calidad,
        tipoPesado: formValue.tipoPesado,
        pesoBruto: formValue.pesoBruto,
        descuentoKg: formValue.descuentoKg,
        pesoNeto: this.pesoNetoProducto,
        precioPorKg: formValue.precioPorKg,
        subtotal: this.subtotalProducto
      };

      this.productosAgregados = [...this.productosAgregados, detalleProducto];
      this.calcularTotales();
      this.resetearFormularioProducto();
    }
  }

  eliminarProducto(index: number): void {
    this.productosAgregados = this.productosAgregados.filter((_, i) => i !== index);
    this.calcularTotales();
  }

  calcularTotales(): void {
    this.pesoTotalCompra = this.productosAgregados.reduce((sum, p) => sum + p.pesoNeto, 0);
    this.montoTotalCompra = this.productosAgregados.reduce((sum, p) => sum + p.subtotal, 0);
  }

  resetearFormularioProducto(): void {
    this.productoForm.reset({
      productoId: '',
      nivelSecado: '',
      calidad: '',
      tipoPesado: 0,
      pesoBruto: '',
      descuentoKg: 0,
      precioPorKg: ''
    });
    this.productoSeleccionado = null;
    this.pesoNetoProducto = 0;
    this.subtotalProducto = 0;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    // Validar que haya un cliente seleccionado O datos de nuevo cliente
    const clienteId = this.generalForm.value.clienteProveedorId;
    const nuevoClienteDni = this.generalForm.value.nuevoClienteDni;
    const nuevoClienteNombre = this.generalForm.value.nuevoClienteNombre;

    const tieneClienteSeleccionado = clienteId && clienteId > 0;
    const tieneNuevoCliente = nuevoClienteDni && nuevoClienteDni.length === 8 && nuevoClienteNombre && nuevoClienteNombre.trim().length > 0;

    if (!tieneClienteSeleccionado && !tieneNuevoCliente) {
      alert('Debe seleccionar un cliente existente o ingresar DNI y nombre de un nuevo cliente');
      return;
    }

    if (this.productosAgregados.length === 0) {
      alert('Debe agregar al menos un producto a la compra');
      return;
    }

    const request: any = {
      detalles: this.productosAgregados.map(p => ({
        productoId: p.productoId,
        nivelSecado: p.nivelSecado,
        calidad: p.calidad,
        tipoPesado: p.tipoPesado,
        pesoBruto: p.pesoBruto,
        descuentoKg: p.descuentoKg,
        precioPorKg: p.precioPorKg
      }))
    };

    // Si hay cliente seleccionado, usar su ID
    if (tieneClienteSeleccionado) {
      request.clienteProveedorId = clienteId;
    } else if (tieneNuevoCliente) {
      // Si es nuevo cliente, enviar sus datos
      request.nuevoCliente = {
        dni: nuevoClienteDni,
        nombreCompleto: nuevoClienteNombre.trim()
      };
    }

    this.dialogRef.close(request);
  }
}

// ==================== DIALOG: DETALLE COMPRA ====================

@Component({
  selector: 'app-detalle-compra-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    FormatoMonedaPipe,
    FormatoFechaPipe,
    NumeroVoucherPipe
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>receipt_long</mat-icon>
      Detalle de Compra - Voucher {{ compra.numeroVoucher | numeroVoucher }}
    </h2>
    <mat-dialog-content>
      <div class="detalle-grid">
        <!-- Información General -->
        <div class="detalle-section">
          <h3>Información General</h3>
          <div class="info-item">
            <span class="label">Voucher:</span>
            <span class="value voucher">{{ compra.numeroVoucher | numeroVoucher }}</span>
          </div>
          <div class="info-item">
            <span class="label">Fecha:</span>
            <span class="value">{{ compra.fechaCompra | formatoFecha: true }}</span>
          </div>
          <div class="info-item">
            <span class="label">Caja ID:</span>
            <span class="value">{{ compra.cajaId }}</span>
          </div>
        </div>

        <!-- Proveedor -->
        <div class="detalle-section">
          <h3>Proveedor</h3>
          <div class="info-item">
            <span class="label">Nombre:</span>
            <span class="value">{{ compra.clienteNombre }}</span>
          </div>
          <div class="info-item">
            <span class="label">DNI:</span>
            <span class="value">{{ compra.clienteDni || 'N/A' }}</span>
          </div>
        </div>

        <!-- Productos -->
        <div class="detalle-section productos-detalle">
          <h3>Productos ({{ compra.detalles?.length || 0 }})</h3>
          <div *ngFor="let detalle of compra.detalles; let i = index" class="producto-item">
            <div class="producto-header">
              <strong>{{ i + 1 }}. {{ detalle.productoNombre }}</strong>
            </div>
            <div class="info-item">
              <span class="label">Nivel de Secado:</span>
              <span class="value">{{ detalle.nivelSecado }}</span>
            </div>
            <div class="info-item">
              <span class="label">Calidad:</span>
              <span class="value">{{ detalle.calidad }}</span>
            </div>
            <div class="info-item">
              <span class="label">Tipo Pesado:</span>
              <span class="value">{{ detalle.tipoPesado === 0 ? 'Kg' : 'Valdeo' }}</span>
            </div>
            <div class="info-item">
              <span class="label">Peso Bruto:</span>
              <span class="value">{{ detalle.pesoBruto }} kg</span>
            </div>
            <div class="info-item">
              <span class="label">Descuento:</span>
              <span class="value">{{ detalle.descuentoKg }} kg</span>
            </div>
            <div class="info-item">
              <span class="label">Peso Neto:</span>
              <span class="value peso-neto">{{ detalle.pesoNeto }} kg</span>
            </div>
            <div class="info-item">
              <span class="label">Precio por Kg:</span>
              <span class="value">{{ detalle.precioPorKg | formatoMoneda }}</span>
            </div>
            <div class="info-item subtotal-item">
              <span class="label">Subtotal:</span>
              <span class="value">{{ detalle.subtotal | formatoMoneda }}</span>
            </div>
          </div>
        </div>

        <!-- Totales -->
        <div class="detalle-section">
          <h3>Totales</h3>
          <div class="info-item">
            <span class="label">Peso Total:</span>
            <span class="value peso-neto">{{ compra.pesoTotal }} kg</span>
          </div>
          <div class="info-item total">
            <span class="label">MONTO TOTAL:</span>
            <span class="value">{{ compra.montoTotal | formatoMoneda }}</span>
          </div>
        </div>

        <!-- Estado -->
        <div class="detalle-section" *ngIf="compra.editada || compra.esAjustePosterior">
          <h3>Estado</h3>
          <div class="estado-chips">
            <mat-chip *ngIf="compra.editada" class="chip-editada">
              <mat-icon>edit</mat-icon>
              Editada
            </mat-chip>
            <mat-chip *ngIf="compra.esAjustePosterior" class="chip-ajuste">
              <mat-icon>history</mat-icon>
              Ajuste Posterior
            </mat-chip>
          </div>
          <div class="info-item" *ngIf="compra.fechaEdicion">
            <span class="label">Fecha Edición:</span>
            <span class="value">{{ compra.fechaEdicion | formatoFecha: true }}</span>
          </div>
        </div>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-raised-button color="primary" (click)="onClose()">Cerrar</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 12px;
      color: #2d3748;
    }
    mat-dialog-content {
      min-width: 600px;
      max-height: 70vh;
      padding: 20px 24px;
      overflow-y: auto;
    }
    .detalle-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 20px;
    }
    .detalle-section {
      background: #f7fafc;
      padding: 16px;
      border-radius: 8px;
      border-left: 4px solid #4299e1;
    }
    .detalle-section h3 {
      margin: 0 0 16px 0;
      font-size: 14px;
      font-weight: 600;
      color: #2d3748;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .productos-detalle {
      grid-column: 1 / -1;
      background: #f0fdf4;
      border-left-color: #86efac;
    }
    .producto-item {
      background: white;
      padding: 12px;
      border-radius: 6px;
      margin-bottom: 12px;
      border: 1px solid #e2e8f0;
    }
    .producto-item:last-child {
      margin-bottom: 0;
    }
    .producto-header {
      font-size: 15px;
      color: #2d3748;
      margin-bottom: 8px;
      padding-bottom: 8px;
      border-bottom: 2px solid #e2e8f0;
    }
    .subtotal-item {
      margin-top: 8px;
      padding-top: 8px;
      border-top: 1px solid #cbd5e0;
    }
    .info-item {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      border-bottom: 1px solid #e2e8f0;
    }
    .info-item:last-child {
      border-bottom: none;
    }
    .info-item .label {
      font-size: 13px;
      color: #718096;
      font-weight: 500;
    }
    .info-item .value {
      font-size: 14px;
      color: #2d3748;
      font-weight: 600;
    }
    .info-item .value.voucher {
      font-family: 'Courier New', monospace;
      color: #2b6cb0;
    }
    .info-item .value.peso-neto {
      color: #38a169;
    }
    .info-item.total {
      margin-top: 8px;
      padding-top: 12px;
      border-top: 2px solid #cbd5e0;
    }
    .info-item.total .label {
      font-size: 14px;
      font-weight: 700;
      color: #2d3748;
    }
    .info-item.total .value {
      font-size: 18px;
      color: #2b6cb0;
    }
    .estado-chips {
      display: flex;
      gap: 8px;
      margin-bottom: 12px;
      flex-wrap: wrap;
    }
    .chip-editada {
      background-color: #fed7aa !important;
      color: #7c2d12 !important;
    }
    .chip-ajuste {
      background-color: #d6bcfa !important;
      color: #44337a !important;
    }
    mat-chip mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
      margin-right: 4px;
    }
    mat-dialog-actions {
      padding: 16px 24px;
    }
  `]
})
export class DetalleCompraDialogComponent {
  compra: any;

  constructor(
    private dialogRef: MatDialogRef<DetalleCompraDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.compra = data;
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
