# Plan de Implementación: Compras Multi-Producto + Corrección Monto Esperado Caja

## Resumen de Cambios Necesarios

### Problema 1: Monto Esperado de Caja No Se Actualiza
**Causa Raíz**: El método `CalcularMontoEsperado()` en `CajaService.cs` suma TODOS los movimientos (ingresos y egresos) en lugar de calcular: MontoInicial + Ingresos - Egresos.

**Solución**: Corregir la lógica de cálculo para:
```
MontoEsperado = MontoInicial + SumaIngresos - SumaEgresos
```

### Problema 2: Una Compra Debe Permitir Múltiples Productos
**Cambio Arquitectónico**: Transformar la relación 1:1 (Compra → Producto) a 1:N (Compra → DetalleCompra → Producto).

**Impacto**: Requiere nueva tabla `DetalleCompra`, migración de datos, cambios en DTOs, servicios y componente Angular.

---

## FASE 1: Corrección Monto Esperado de Caja (Rápido)

### Backend

#### 1.1 Corregir `CajaService.cs:CalcularMontoEsperado()`

**Archivo**: `Backend/Services/CajaService.cs`

**Lógica Actual (INCORRECTA)**:
```csharp
var montoMovimientos = movimientos.Sum(m => m.Monto);
return cajaDb.MontoInicial + montoMovimientos;
```

**Lógica Corregida**:
```csharp
var totalIngresos = movimientos
    .Where(m => m.TipoMovimiento == TipoMovimiento.Ingreso)
    .Sum(m => m.Monto);

var totalEgresos = movimientos
    .Where(m => m.TipoMovimiento == TipoMovimiento.Egreso)
    .Sum(m => m.Monto);

return cajaDb.MontoInicial + totalIngresos - totalEgresos;
```

**Validación**: Después de este cambio, al registrar una compra (egreso), el monto esperado debe DISMINUIR correctamente.

---

## FASE 2: Implementación Compras Multi-Producto (Complejo)

### Arquitectura Nueva

```
Compra (1) ──┬──> DetalleCompra (N) ──> Producto (1)
             │
             ├──> ClienteProveedor (1)
             ├──> Caja (1)
             └──> MovimientoCaja (1)
```

**Reglas de Negocio**:
- Una `Compra` puede tener N `DetalleCompra` (mínimo 1)
- Cada `DetalleCompra` tiene: Producto, Características específicas, PesoNeto, PrecioUnitario, Subtotal
- `Compra.MontoTotal` = suma de todos los `DetalleCompra.Subtotal`
- `Compra.PesoTotal` = suma de todos los `DetalleCompra.PesoNeto`
- Se genera UN solo voucher con todos los productos listados
- Se crea UN solo `MovimientoCaja` por el `MontoTotal`

### Backend

#### 2.1 Crear Nueva Entidad `DetalleCompra`

**Archivo**: `Backend/Models/DetalleCompra.cs` (NUEVO)

```csharp
public class DetalleCompra
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public Compra Compra { get; set; }

    public int ProductoId { get; set; }
    public Producto Producto { get; set; }

    // Datos específicos por producto
    public decimal PesoBruto { get; set; }
    public decimal Tara { get; set; }
    public decimal PesoNeto { get; set; }

    // Características JSON específicas (varía por producto)
    public string? Caracteristicas { get; set; } // JSON: { "NivelSecado": "12%", "Calidad": "Primera", etc }

    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; } // PesoNeto * PrecioUnitario

    public DateTime FechaCreacion { get; set; }
}
```

#### 2.2 Modificar Entidad `Compra`

**Archivo**: `Backend/Models/Compra.cs`

**Eliminar** campos que se mueven a `DetalleCompra`:
- ~~ProductoId, Producto~~ (ahora en DetalleCompra)
- ~~PesoBruto, Tara, PesoNeto~~ (ahora en DetalleCompra)
- ~~PrecioUnitario~~ (ahora en DetalleCompra)
- ~~Caracteristicas~~ (ahora en DetalleCompra)

**Mantener** campos de la compra general:
- MontoTotal (suma de subtotales)
- PesoTotal (suma de pesos netos)
- FormaPago, MontoPagado, SaldoPendiente, etc.

**Agregar**:
```csharp
public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
```

#### 2.3 Actualizar `AppDbContext.cs`

**Archivo**: `Backend/Data/AppDbContext.cs`

Agregar DbSet:
```csharp
public DbSet<DetalleCompra> DetallesCompra { get; set; }
```

Configurar relación en `OnModelCreating`:
```csharp
modelBuilder.Entity<DetalleCompra>(entity =>
{
    entity.HasKey(d => d.Id);

    entity.Property(d => d.PesoBruto).HasColumnType("decimal(18,1)");
    entity.Property(d => d.Tara).HasColumnType("decimal(18,1)");
    entity.Property(d => d.PesoNeto).HasColumnType("decimal(18,1)");
    entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
    entity.Property(d => d.Subtotal).HasColumnType("decimal(18,2)");

    entity.HasOne(d => d.Compra)
        .WithMany(c => c.Detalles)
        .HasForeignKey(d => d.CompraId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(d => d.Producto)
        .WithMany()
        .HasForeignKey(d => d.ProductoId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

#### 2.4 Crear Migración

**Comando**:
```bash
cd Backend
dotnet ef migrations add ComprasMultiProducto
```

**Migración incluirá**:
- Crear tabla `DetallesCompra`
- Migrar datos existentes de `Compras` a `DetallesCompra` (un detalle por compra existente)
- Eliminar columnas obsoletas de `Compras` (ProductoId, PesoBruto, etc.)

**Script de migración de datos** (en el método `Up`):
```csharp
migrationBuilder.Sql(@"
    INSERT INTO DetallesCompra (CompraId, ProductoId, PesoBruto, Tara, PesoNeto, Caracteristicas, PrecioUnitario, Subtotal, FechaCreacion)
    SELECT Id, ProductoId, PesoBruto, Tara, PesoNeto, Caracteristicas, PrecioUnitario, MontoTotal, FechaCompra
    FROM Compras;
");
```

#### 2.5 Crear DTOs para Detalles

**Archivo**: `Backend/DTOs/Requests/DetalleCompraRequest.cs` (NUEVO)

```csharp
public class DetalleCompraRequest
{
    public int ProductoId { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal Tara { get; set; }
    public string? Caracteristicas { get; set; } // JSON string
    public decimal PrecioUnitario { get; set; }
}
```

**Archivo**: `Backend/DTOs/Responses/DetalleCompraResponse.cs` (NUEVO)

```csharp
public class DetalleCompraResponse
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; }
    public decimal PesoBruto { get; set; }
    public decimal Tara { get; set; }
    public decimal PesoNeto { get; set; }
    public string? Caracteristicas { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
```

#### 2.6 Modificar DTOs de Compra

**Archivo**: `Backend/DTOs/Requests/RegistrarCompraRequest.cs`

**Cambios**:
```csharp
public class RegistrarCompraRequest
{
    // Datos generales de la compra
    public int ClienteProveedorId { get; set; }
    public string FormaPago { get; set; }
    public decimal MontoPagado { get; set; }

    // NUEVO: Lista de productos/detalles
    public List<DetalleCompraRequest> Detalles { get; set; } = new();

    // ELIMINADO: ProductoId, PesoBruto, Tara, Caracteristicas, PrecioUnitario (ahora en Detalles)
}
```

**Archivo**: `Backend/DTOs/Responses/CompraResponse.cs`

```csharp
public class CompraResponse
{
    public int Id { get; set; }
    public DateTime FechaCompra { get; set; }
    public string ClienteProveedorNombre { get; set; }

    // NUEVO: Lista de detalles
    public List<DetalleCompraResponse> Detalles { get; set; } = new();

    public decimal PesoTotal { get; set; } // Suma de detalles
    public decimal MontoTotal { get; set; } // Suma de detalles
    public string FormaPago { get; set; }
    public string NumeroVoucher { get; set; }
    // ... otros campos
}
```

#### 2.7 Actualizar `ComprasService.cs`

**Archivo**: `Backend/Services/ComprasService.cs`

**Método `RegistrarCompraAsync` - Lógica nueva**:

```csharp
public async Task<CompraResponse> RegistrarCompraAsync(RegistrarCompraRequest request)
{
    // Validar que hay al menos un detalle
    if (request.Detalles == null || !request.Detalles.Any())
        throw new BusinessException("Debe incluir al menos un producto en la compra");

    // Validar caja abierta
    var cajaActual = await cajaService.ObtenerCajaActualAsync();
    if (cajaActual == null || cajaActual.Estado != "Abierta")
        throw new BusinessException("No hay caja abierta para registrar compras");

    using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        // Obtener siguiente número de voucher
        var config = await context.ConfiguracionNegocio.FirstAsync();
        var numeroVoucher = config.UltimoNumeroVoucher + 1;

        // Crear compra base
        var compra = new Compra
        {
            FechaCompra = DateTime.Now,
            ClienteProveedorId = request.ClienteProveedorId,
            CajaId = cajaActual.Id,
            NumeroVoucher = numeroVoucher.ToString("D8"),
            FormaPago = request.FormaPago,
            MontoPagado = request.MontoPagado,
            MontoTotal = 0, // Se calculará
            PesoTotal = 0,  // Se calculará
            Detalles = new List<DetalleCompra>()
        };

        // Procesar cada detalle
        foreach (var detalleReq in request.Detalles)
        {
            var producto = await context.Productos.FindAsync(detalleReq.ProductoId);
            if (producto == null)
                throw new BusinessException($"Producto {detalleReq.ProductoId} no encontrado");

            var pesoNeto = detalleReq.PesoBruto - detalleReq.Tara;
            var subtotal = pesoNeto * detalleReq.PrecioUnitario;

            var detalle = new DetalleCompra
            {
                ProductoId = detalleReq.ProductoId,
                PesoBruto = detalleReq.PesoBruto,
                Tara = detalleReq.Tara,
                PesoNeto = pesoNeto,
                Caracteristicas = detalleReq.Caracteristicas,
                PrecioUnitario = detalleReq.PrecioUnitario,
                Subtotal = subtotal,
                FechaCreacion = DateTime.Now
            };

            compra.Detalles.Add(detalle);
            compra.MontoTotal += subtotal;
            compra.PesoTotal += pesoNeto;
        }

        compra.SaldoPendiente = compra.MontoTotal - request.MontoPagado;

        // Guardar compra con detalles
        context.Compras.Add(compra);

        // Crear movimiento de caja (UN SOLO egreso por el total)
        var movimiento = new MovimientoCaja
        {
            CajaId = cajaActual.Id,
            Fecha = DateTime.Now,
            TipoMovimiento = TipoMovimiento.Egreso,
            Monto = compra.MontoTotal,
            Concepto = $"Compra #{compra.NumeroVoucher} - {compra.Detalles.Count} producto(s)",
            ReferenciaId = compra.Id,
            TipoReferencia = "Compra"
        };
        context.MovimientosCaja.Add(movimiento);

        // Actualizar contador de voucher
        config.UltimoNumeroVoucher = numeroVoucher;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        // Mapear respuesta con detalles
        return await MapearCompraResponse(compra);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

**Nuevo método auxiliar**:
```csharp
private async Task<CompraResponse> MapearCompraResponse(Compra compra)
{
    var compraDb = await context.Compras
        .Include(c => c.ClienteProveedor)
        .Include(c => c.Detalles)
        .ThenInclude(d => d.Producto)
        .FirstAsync(c => c.Id == compra.Id);

    return new CompraResponse
    {
        Id = compraDb.Id,
        FechaCompra = compraDb.FechaCompra,
        ClienteProveedorNombre = compraDb.ClienteProveedor.Nombre,
        Detalles = compraDb.Detalles.Select(d => new DetalleCompraResponse
        {
            Id = d.Id,
            ProductoId = d.ProductoId,
            ProductoNombre = d.Producto.Nombre,
            PesoBruto = d.PesoBruto,
            Tara = d.Tara,
            PesoNeto = d.PesoNeto,
            Caracteristicas = d.Caracteristicas,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal
        }).ToList(),
        PesoTotal = compraDb.PesoTotal,
        MontoTotal = compraDb.MontoTotal,
        FormaPago = compraDb.FormaPago,
        NumeroVoucher = compraDb.NumeroVoucher,
        // ... otros campos
    };
}
```

#### 2.8 Actualizar `ComprasRepository.cs`

**Archivo**: `Backend/Repositories/ComprasRepository.cs`

Actualizar todos los métodos que cargan compras para incluir `.Include(c => c.Detalles).ThenInclude(d => d.Producto)`:

```csharp
public async Task<Compra?> ObtenerPorIdAsync(int id)
{
    return await context.Compras
        .Include(c => c.ClienteProveedor)
        .Include(c => c.Detalles)
        .ThenInclude(d => d.Producto)
        .FirstOrDefaultAsync(c => c.Id == id);
}

public async Task<List<Compra>> ObtenerPorCajaAsync(int cajaId)
{
    return await context.Compras
        .Include(c => c.ClienteProveedor)
        .Include(c => c.Detalles)
        .ThenInclude(d => d.Producto)
        .Where(c => c.CajaId == cajaId)
        .OrderByDescending(c => c.FechaCompra)
        .ToListAsync();
}
```

#### 2.9 Actualizar `VoucherService.cs`

**Archivo**: `Backend/Services/VoucherService.cs`

Modificar generación de voucher para listar múltiples productos:

```csharp
public async Task<byte[]> GenerarVoucherCompraAsync(int compraId)
{
    var compra = await context.Compras
        .Include(c => c.ClienteProveedor)
        .Include(c => c.Detalles)
        .ThenInclude(d => d.Producto)
        .FirstAsync(c => c.Id == compraId);

    var sb = new StringBuilder();

    // Header
    sb.AppendLine("========================================");
    sb.AppendLine("     COMERCIAL ALIAGA");
    sb.AppendLine("     VOUCHER DE COMPRA");
    sb.AppendLine("========================================");
    sb.AppendLine($"Voucher: {compra.NumeroVoucher}");
    sb.AppendLine($"Fecha: {compra.FechaCompra:dd/MM/yyyy HH:mm}");
    sb.AppendLine($"Proveedor: {compra.ClienteProveedor.Nombre}");
    sb.AppendLine($"DNI: {compra.ClienteProveedor.DocumentoIdentidad}");
    sb.AppendLine("========================================");

    // PRODUCTOS (tabla multi-línea)
    sb.AppendLine("PRODUCTOS:");
    foreach (var detalle in compra.Detalles)
    {
        sb.AppendLine($"- {detalle.Producto.Nombre}");
        sb.AppendLine($"  Peso Bruto: {detalle.PesoBruto:F1} kg");
        sb.AppendLine($"  Tara: {detalle.Tara:F1} kg");
        sb.AppendLine($"  Peso Neto: {detalle.PesoNeto:F1} kg");
        sb.AppendLine($"  Precio: S/ {detalle.PrecioUnitario:F2}/kg");
        sb.AppendLine($"  Subtotal: S/ {detalle.Subtotal:F2}");

        // Características si existen
        if (!string.IsNullOrEmpty(detalle.Caracteristicas))
        {
            var caract = JsonSerializer.Deserialize<Dictionary<string, string>>(detalle.Caracteristicas);
            foreach (var c in caract)
                sb.AppendLine($"  {c.Key}: {c.Value}");
        }
        sb.AppendLine();
    }

    sb.AppendLine("========================================");
    sb.AppendLine($"PESO TOTAL: {compra.PesoTotal:F1} kg");
    sb.AppendLine($"MONTO TOTAL: S/ {compra.MontoTotal:F2}");
    sb.AppendLine($"Forma Pago: {compra.FormaPago}");
    sb.AppendLine($"Pagado: S/ {compra.MontoPagado:F2}");
    sb.AppendLine($"Saldo: S/ {compra.SaldoPendiente:F2}");
    sb.AppendLine("========================================");

    return Encoding.UTF8.GetBytes(sb.ToString());
}
```

### Frontend

#### 2.10 Actualizar Modelos TypeScript

**Archivo**: `Frontend/src/app/core/models/compra.model.ts`

```typescript
export interface DetalleCompra {
  id?: number;
  productoId: number;
  productoNombre?: string;
  pesoBruto: number;
  tara: number;
  pesoNeto: number;
  caracteristicas?: string; // JSON string
  precioUnitario: number;
  subtotal: number;
}

export interface RegistrarCompraRequest {
  clienteProveedorId: number;
  formaPago: string;
  montoPagado: number;
  detalles: DetalleCompraRequest[]; // NUEVO: array de productos
}

export interface DetalleCompraRequest {
  productoId: number;
  pesoBruto: number;
  tara: number;
  caracteristicas?: string;
  precioUnitario: number;
}

export interface CompraResponse {
  id: number;
  fechaCompra: string;
  clienteProveedorNombre: string;
  detalles: DetalleCompra[]; // NUEVO
  pesoTotal: number;
  montoTotal: number;
  formaPago: string;
  numeroVoucher: string;
  // ... otros campos
}
```

#### 2.11 Rediseñar `ComprasComponent` UI

**Archivo**: `Frontend/src/app/features/compras/compras.component.ts`

**Estructura de datos del componente**:

```typescript
export class ComprasComponent implements OnInit {
  // Form para datos generales
  compraForm!: FormGroup;

  // Array temporal de productos agregados
  productosAgregados: DetalleCompraTemp[] = [];

  // Form para agregar producto individual
  productoForm!: FormGroup;

  // Totales calculados
  get pesoTotal(): number {
    return this.productosAgregados.reduce((sum, p) => sum + p.pesoNeto, 0);
  }

  get montoTotal(): number {
    return this.productosAgregados.reduce((sum, p) => sum + p.subtotal, 0);
  }

  ngOnInit() {
    this.inicializarForms();
  }

  inicializarForms() {
    // Form para datos generales de la compra
    this.compraForm = this.fb.group({
      clienteProveedorId: [null, Validators.required],
      formaPago: ['Efectivo', Validators.required],
      montoPagado: [0, [Validators.required, Validators.min(0)]]
    });

    // Form para agregar cada producto
    this.productoForm = this.fb.group({
      productoId: [null, Validators.required],
      pesoBruto: [0, [Validators.required, Validators.min(0)]],
      tara: [0, [Validators.required, Validators.min(0)]],
      precioUnitario: [0, [Validators.required, Validators.min(0)]],
      // Campos dinámicos según producto (nivelSecado, calidad, etc.)
    });
  }

  agregarProducto() {
    if (this.productoForm.invalid) {
      this.productoForm.markAllAsTouched();
      return;
    }

    const valores = this.productoForm.value;
    const pesoNeto = valores.pesoBruto - valores.tara;
    const subtotal = pesoNeto * valores.precioUnitario;

    const producto: DetalleCompraTemp = {
      productoId: valores.productoId,
      productoNombre: this.obtenerNombreProducto(valores.productoId),
      pesoBruto: valores.pesoBruto,
      tara: valores.tara,
      pesoNeto: pesoNeto,
      caracteristicas: this.construirCaracteristicas(),
      precioUnitario: valores.precioUnitario,
      subtotal: subtotal
    };

    this.productosAgregados.push(producto);
    this.productoForm.reset();
  }

  eliminarProducto(index: number) {
    this.productosAgregados.splice(index, 1);
  }

  registrarCompra() {
    if (this.compraForm.invalid || this.productosAgregados.length === 0) {
      this.mostrarError('Debe agregar al menos un producto');
      return;
    }

    const request: RegistrarCompraRequest = {
      clienteProveedorId: this.compraForm.value.clienteProveedorId,
      formaPago: this.compraForm.value.formaPago,
      montoPagado: this.compraForm.value.montoPagado,
      detalles: this.productosAgregados.map(p => ({
        productoId: p.productoId,
        pesoBruto: p.pesoBruto,
        tara: p.tara,
        caracteristicas: p.caracteristicas,
        precioUnitario: p.precioUnitario
      }))
    };

    this.comprasService.registrarCompra(request).subscribe({
      next: (response) => {
        this.mostrarExito('Compra registrada exitosamente');
        this.limpiarFormulario();
        this.cargarCompras();
      },
      error: (err) => this.mostrarError(err.error?.message || 'Error al registrar compra')
    });
  }

  private construirCaracteristicas(): string {
    // Construir JSON según producto seleccionado
    // Ejemplo: { "nivelSecado": "12%", "calidad": "Primera" }
    return JSON.stringify({
      // ... campos dinámicos
    });
  }
}
```

#### 2.12 Rediseñar Template HTML

**Archivo**: `Frontend/src/app/features/compras/compras.component.html`

**Estructura nueva**:

```html
<div class="compras-container">
  <!-- Sección 1: Datos Generales -->
  <mat-card>
    <mat-card-header>
      <mat-card-title>Datos Generales de la Compra</mat-card-title>
    </mat-card-header>
    <mat-card-content>
      <form [formGroup]="compraForm">
        <!-- ClienteProveedor autocomplete -->
        <!-- Forma de pago -->
        <!-- Monto pagado -->
      </form>
    </mat-card-content>
  </mat-card>

  <!-- Sección 2: Agregar Productos -->
  <mat-card>
    <mat-card-header>
      <mat-card-title>Agregar Producto</mat-card-title>
    </mat-card-header>
    <mat-card-content>
      <form [formGroup]="productoForm">
        <!-- Select Producto -->
        <!-- Peso Bruto, Tara -->
        <!-- Campos dinámicos según producto (nivel secado, calidad, etc.) -->
        <!-- Precio Unitario -->

        <button mat-raised-button color="primary" (click)="agregarProducto()">
          ➕ Agregar Producto
        </button>
      </form>
    </mat-card-content>
  </mat-card>

  <!-- Sección 3: Tabla de Productos Agregados -->
  <mat-card *ngIf="productosAgregados.length > 0">
    <mat-card-header>
      <mat-card-title>Productos a Registrar ({{ productosAgregados.length }})</mat-card-title>
    </mat-card-header>
    <mat-card-content>
      <table mat-table [dataSource]="productosAgregados">
        <ng-container matColumnDef="producto">
          <th mat-header-cell *matHeaderCellDef>Producto</th>
          <td mat-cell *matCellDef="let detalle">{{ detalle.productoNombre }}</td>
        </ng-container>

        <ng-container matColumnDef="pesoNeto">
          <th mat-header-cell *matHeaderCellDef>Peso Neto</th>
          <td mat-cell *matCellDef="let detalle">{{ detalle.pesoNeto | number:'1.1-1' }} kg</td>
        </ng-container>

        <ng-container matColumnDef="precioUnitario">
          <th mat-header-cell *matHeaderCellDef>Precio/kg</th>
          <td mat-cell *matCellDef="let detalle">{{ detalle.precioUnitario | currency:'S/ ' }}</td>
        </ng-container>

        <ng-container matColumnDef="subtotal">
          <th mat-header-cell *matHeaderCellDef>Subtotal</th>
          <td mat-cell *matCellDef="let detalle">{{ detalle.subtotal | currency:'S/ ' }}</td>
        </ng-container>

        <ng-container matColumnDef="acciones">
          <th mat-header-cell *matHeaderCellDef>Acciones</th>
          <td mat-cell *matCellDef="let detalle; let i = index">
            <button mat-icon-button color="warn" (click)="eliminarProducto(i)">
              🗑️
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
      </table>

      <!-- Fila de Totales -->
      <div class="totales">
        <p><strong>Peso Total:</strong> {{ pesoTotal | number:'1.1-1' }} kg</p>
        <p><strong>Monto Total:</strong> {{ montoTotal | currency:'S/ ' }}</p>
      </div>
    </mat-card-content>
  </mat-card>

  <!-- Sección 4: Botón Final de Registro -->
  <div class="acciones-finales">
    <button mat-raised-button color="primary"
            [disabled]="compraForm.invalid || productosAgregados.length === 0"
            (click)="registrarCompra()">
      💾 Registrar Compra ({{ productosAgregados.length }} producto(s))
    </button>
    <button mat-button (click)="cancelar()">Cancelar</button>
  </div>
</div>
```

---

## Orden de Implementación

### ✅ Paso 1: FASE 1 Completa (Corrección Monto Esperado)
- Duración estimada: 10 minutos
- Archivos: `Backend/Services/CajaService.cs` (1 método)
- Testing: Registrar una compra y verificar que MontoEsperado disminuye

### ✅ Paso 2: Backend FASE 2 - Modelos y Migración
- Crear `DetalleCompra.cs`
- Modificar `Compra.cs`
- Actualizar `AppDbContext.cs`
- Crear migración con script de migración de datos
- Aplicar migración: `dotnet ef database update`

### ✅ Paso 3: Backend FASE 2 - DTOs
- Crear `DetalleCompraRequest.cs` y `DetalleCompraResponse.cs`
- Modificar `RegistrarCompraRequest.cs` y `CompraResponse.cs`

### ✅ Paso 4: Backend FASE 2 - Servicios y Repositorios
- Actualizar `ComprasService.cs` (método `RegistrarCompraAsync` completo)
- Actualizar `ComprasRepository.cs` (agregar `.Include(c => c.Detalles)`)
- Actualizar `VoucherService.cs` (listado multi-producto)

### ✅ Paso 5: Testing Backend
- Probar endpoint POST `/api/compras` con Postman/Swagger
- Request de prueba con 2 productos (Café y Cacao)
- Verificar creación de detalles, voucher, movimiento caja

### ✅ Paso 6: Frontend - Modelos
- Actualizar `compra.model.ts` con interfaces de detalles

### ✅ Paso 7: Frontend - Componente
- Rediseñar `ComprasComponent` (lógica multi-producto)
- Rediseñar template HTML (3 secciones: datos generales, agregar producto, tabla)
- Estilos CSS para tabla de productos agregados

### ✅ Paso 8: Testing E2E
- Registrar compra con 1 producto (caso simple)
- Registrar compra con 3 productos diferentes
- Verificar voucher impreso con todos los productos
- Verificar monto esperado de caja se actualiza correctamente

---

## Consideraciones Importantes

### Migración de Datos Existentes
- Las compras actuales tienen 1 producto cada una
- La migración creará automáticamente 1 `DetalleCompra` por cada `Compra` existente
- No se perderá información histórica

### Compatibilidad con Reportes
- Los reportes que usan `Compra` deben actualizarse para acceder a `Detalles`
- Ejemplo: Reporte de compras por producto debe iterar sobre `DetalleCompra`

### Validaciones Nuevas
- Mínimo 1 producto por compra (frontend y backend)
- Suma de subtotales debe coincidir con `MontoTotal`
- Validar que `MontoPagado <= MontoTotal`

### Impacto en Edición de Compras
- Si se permite editar compras del mismo día, debe permitir:
  - Agregar/eliminar productos
  - Modificar cantidades/precios de productos existentes
  - Recalcular totales

---

## Archivos a Modificar/Crear

### Backend (14 archivos)
1. ✅ `Backend/Services/CajaService.cs` (FASE 1)
2. 🆕 `Backend/Models/DetalleCompra.cs` (NUEVO)
3. ✏️ `Backend/Models/Compra.cs`
4. ✏️ `Backend/Data/AppDbContext.cs`
5. 🆕 `Backend/Migrations/XXXXXX_ComprasMultiProducto.cs` (GENERADA)
6. 🆕 `Backend/DTOs/Requests/DetalleCompraRequest.cs` (NUEVO)
7. 🆕 `Backend/DTOs/Responses/DetalleCompraResponse.cs` (NUEVO)
8. ✏️ `Backend/DTOs/Requests/RegistrarCompraRequest.cs`
9. ✏️ `Backend/DTOs/Responses/CompraResponse.cs`
10. ✏️ `Backend/Services/ComprasService.cs`
11. ✏️ `Backend/Repositories/ComprasRepository.cs`
12. ✏️ `Backend/Services/VoucherService.cs`

### Frontend (3 archivos)
13. ✏️ `Frontend/src/app/core/models/compra.model.ts`
14. ✏️ `Frontend/src/app/features/compras/compras.component.ts`
15. ✏️ `Frontend/src/app/features/compras/compras.component.html`
16. ✏️ `Frontend/src/app/features/compras/compras.component.css` (nuevos estilos tabla)

---

## Riesgos y Mitigaciones

### ⚠️ Riesgo 1: Pérdida de datos en migración
**Mitigación**: Backup de `miapp.db` antes de aplicar migración. Script de migración probado con datos de prueba.

### ⚠️ Riesgo 2: Performance con muchos detalles
**Mitigación**: Eager loading con `.Include()` en todas las queries. Índices en `CompraId` y `ProductoId` en `DetallesCompra`.

### ⚠️ Riesgo 3: UI confusa para el usuario
**Mitigación**: UX clara con 3 secciones separadas, tabla visual de productos agregados, totales en tiempo real.

---

## Testing Checklist

- [ ] FASE 1: Monto esperado disminuye al registrar compra
- [ ] FASE 1: Monto esperado aumenta al registrar venta
- [ ] Migración ejecuta sin errores
- [ ] Datos históricos migrados correctamente (1 detalle por compra)
- [ ] Backend: Registrar compra con 1 producto
- [ ] Backend: Registrar compra con 3 productos diferentes
- [ ] Backend: Validación rechaza compra sin productos
- [ ] Backend: Totales calculados correctamente (suma subtotales)
- [ ] Voucher muestra todos los productos en formato legible
- [ ] Frontend: Agregar múltiples productos a la tabla
- [ ] Frontend: Eliminar producto de la tabla antes de registrar
- [ ] Frontend: Totales en tiempo real se actualizan
- [ ] Frontend: Validación no permite registrar sin productos
- [ ] E2E: Flujo completo con 2 productos (café y cacao)
- [ ] E2E: Verificar en módulo Caja que egreso se registra correctamente
