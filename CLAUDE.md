# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Sistema Comercial Aliaga** - A grain purchasing management system for a local agricultural business in Peru. This is a full-stack application with:
- **Backend**: .NET 10 Web API with SQLite database
- **Frontend**: Angular 21 standalone components

The system manages daily cash register operations, grain purchases from farmers (with thermal voucher printing), sales, interest-free loans to customers, and comprehensive business reporting.

## Quick Start Commands

### Backend (.NET 10)

```bash
cd Backend

# Run the application (Development mode with Swagger)
dotnet run

# Build the project
dotnet build

# Apply database migrations
dotnet ef database update

# Create a new migration (when models change)
dotnet ef migrations add MigrationName

# Access Swagger UI
# Navigate to: http://localhost:5252/swagger
```

### Frontend (Angular 21)

```bash
cd Frontend

# Install dependencies
npm install

# Run development server
npm start
# Access at: http://localhost:4200

# Build for production
npm run build

# Run tests
npm test
```

## Architecture

### Backend Architecture Pattern

**Clean Architecture with Repository + Service + Controller layers**

```
Request Flow:
Controller → Service (business logic) → Repository (data access) → Database
         ← Response ← DTOs ←           ←                         ←
```

**Key Design Decisions:**

1. **Repository Pattern**: All data access goes through repositories (`ICajaRepository`, `ICompraRepository`, etc.)
2. **Service Layer**: Complex business logic lives in services (`CajaService`, `ComprasService`, etc.)
3. **DTOs**: Separate Request/Response objects to decouple API from domain models
4. **Dependency Injection**: All services and repositories registered in `Program.cs`
5. **Async/Await**: All database operations are asynchronous

**Critical Business Rules:**

- **One Caja (Cash Register) Per Day**: System enforces single cash register per date. Previous unclosed registers auto-close when opening new one.
- **Caja Must Be Open**: All operations (compras, ventas, préstamos) validate that current day's caja is open.
- **Cliente Anónimo**: Immutable anonymous customer (DNI: 00000000) for walk-in purchases.
- **Voucher Numbering**: Global sequential counter in `ConfiguracionNegocio` table. Never reset.
- **Same-Day Edits Only**: Compras and ventas can only be edited on registration day. Use "Ajustes Posteriores" for historical changes.
- **Decimal Precision**: Money (18,2), Weights (18,1) configured in `AppDbContext`.

### Frontend Architecture Pattern

**Feature-based modules with standalone components**

```
App Structure:
app/
├── core/               # Singleton services, models, interceptors
│   ├── services/       # HTTP services matching backend controllers
│   ├── interceptors/   # Global HTTP interceptors (error, loading)
│   └── models/         # TypeScript interfaces matching backend DTOs
├── shared/             # Reusable components, pipes, directives
│   ├── components/     # Sidebar, LoadingSpinner, etc.
│   └── pipes/          # formatoMoneda, formatoFecha, numeroVoucher
└── features/           # Business modules (lazy-loaded)
    ├── dashboard/      # Main overview with statistics
    ├── caja/           # Cash register management
    ├── compras/        # Purchase registration
    └── [otros]/        # Other business modules
```

**Key Patterns:**

- **Standalone Components**: All components use standalone: true (Angular 21)
- **Lazy Loading**: Feature modules loaded on-demand via routing
- **HTTP Interceptors**: Global error handling and loading state
- **Reactive Services**: RxJS Observables for all HTTP calls
- **Environment Config**: API URL in `environments/environment.ts`

### Database Schema Overview

**10 Core Entities:**

1. **ConfiguracionNegocio** (singleton): Business config, voucher counter
2. **Zona**: Geographic zones for customer organization
3. **ClienteProveedor**: Suppliers (farmers) with loan balance tracking
4. **ClienteComprador**: Buyers (wholesale customers)
5. **Producto**: Fixed products (Café, Cacao, Maíz, Achiote) with JSON characteristics
6. **Caja**: Daily cash register with open/close tracking
7. **MovimientoCaja**: Individual cash movements (all transaction types)
8. **Compra**: Purchase transactions with voucher generation
9. **Venta**: Sales transactions (simpler than compras)
10. **Prestamo**: Loan and payment records (interest-free)

**Critical Relationships:**

- All transactions (Compra, Venta, Prestamo) link to Caja (required)
- All transactions create corresponding MovimientoCaja record
- ClienteProveedor.SaldoPrestamo automatically updated on loan/payment
- Producto characteristics stored as JSON (NivelesSecado, Calidades)

### API Endpoints Structure

All controllers follow RESTful conventions:

- **CajaController** (8 endpoints): POST abrir, POST cerrar, POST reabrir, GET actual, GET historial, GET {id}, POST {id}/movimiento
- **ComprasController** (7 endpoints): CRUD + voucher reprint + filter by caja
- **VentasController** (5 endpoints): CRUD + filter by caja
- **PrestamosController** (6 endpoints): POST prestamo, POST abono, GET filtered
- **ClientesController** (11 endpoints): Proveedores CRUD, Compradores CRUD, GET reniec/{dni}
- **ProductosController** (3 endpoints): GET all, GET {id}, PUT {id}/precio (only price edit)
- **ZonasController** (5 endpoints): CRUD + GET {id}/clientes
- **ReportesController** (10 endpoints): 5 report types, each with view + Excel export
- **DashboardController** (5 endpoints): resumen-dia, estado-caja, alertas, estadisticas, top-deudores
- **ConfiguracionController** (4 endpoints): GET backup, POST ajuste-posterior/*

## Database Migrations

**Important**: Single comprehensive migration created with all tables and seed data.

```bash
# If you need to reset the database:
cd Backend
rm miapp.db               # Delete existing database
dotnet ef database update # Recreate from migrations

# Seed data automatically includes:
# - ConfiguracionNegocio (voucher counter = 1)
# - Cliente Anónimo (DNI: 00000000)
# - 4 Productos (Café, Cacao, Maíz, Achiote)
```

## Service Registration

All services must be registered in `Backend/Program.cs`:

```csharp
// Repositories
builder.Services.AddScoped<IXxxRepository, XxxRepository>();

// Services
builder.Services.AddScoped<IXxxService, XxxService>();
```

## Common Workflows

### Adding a New Feature Module

1. Create service in `Frontend/src/app/core/services/`
2. Create component in `Frontend/src/app/features/module-name/`
3. Add route in `Frontend/src/app/app.routes.ts`
4. Use lazy loading: `loadComponent: () => import('./features/...')`

### Adding a New Endpoint

1. Create DTOs in `Backend/DTOs/Requests` and `Backend/DTOs/Responses`
2. Add repository method if needed (data access)
3. Add service method (business logic)
4. Add controller endpoint
5. Register new services in `Program.cs`
6. Create corresponding method in Angular service

### Excel Report Export

All reports use ClosedXML. Pattern in `ReportesService.cs`:

```csharp
public async Task<byte[]> ExportarReporteAExcelAsync<T>(List<T> datos, string nombreHoja)
```

Frontend downloads via Blob:

```typescript
exportarReporte(filtros: any): Observable<Blob> {
  return this.http.post(url, filtros, { responseType: 'blob' });
}
```

## Technology Stack

**Backend:**
- .NET 10 Web API
- Entity Framework Core 10 with SQLite
- AutoMapper (DTO mapping)
- ClosedXML (Excel export)
- FluentValidation
- Swagger/OpenAPI

**Frontend:**
- Angular 21 (standalone components)
- TypeScript 5.9
- RxJS 7.8
- Angular Material 21
- Tailwind CSS 4

## CORS Configuration

Backend allows `http://localhost:4200` (configured in `Program.cs`). If frontend runs on different port, update CORS policy.

## Important Files

- `Backend/Data/AppDbContext.cs` - All entity configurations, relationships, seed data
- `Backend/Program.cs` - Service registration, DI configuration, CORS
- `Frontend/src/app/app.routes.ts` - All application routes
- `Frontend/src/app/app.config.ts` - HTTP interceptors, providers
- `Frontend/src/environments/environment.ts` - API base URL

## Business Logic Notes

**Voucher Generation**: Uses ESC/POS format for 80mm thermal printers. Service stub in `VoucherService.cs` - actual printer integration requires ESCPOS-NET library configuration.

**RENIEC Integration**: External API for Peruvian DNI validation. Fallback to manual entry if unavailable. Configure in `appsettings.json` → `ReniecApi:Url`.

**Transaction Safety**: Critical operations (compra, venta, préstamo) use database transactions to ensure atomicity. Always commit or rollback.

**Producto Characteristics**: Stored as JSON strings in database, deserialized in services. Each product has different attributes (NivelesSecado, Calidades, PermiteValdeo).

## Port Configuration

- **Backend**: http://localhost:5252
- **Frontend**: http://localhost:4200
- **Database**: SQLite file at `Backend/miapp.db`

Update `Frontend/src/environments/environment.ts` if backend port changes.
