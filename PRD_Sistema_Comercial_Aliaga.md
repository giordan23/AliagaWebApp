# PRD - Sistema Comercial Aliaga
## Product Requirements Document

**Versión:** 1.0  
**Fecha:** 18 de Diciembre, 2025  
**Proyecto:** Sistema de Gestión de Acopio de Granos  
**Stack Tecnológico:** .NET 10 + Angular + SQLite

---

## 1. RESUMEN EJECUTIVO

### 1.1 Visión General
Sistema de gestión integral para un negocio de acopio de granos (café, cacao, maíz, achiote) que permite controlar compras a proveedores, ventas a compradores, gestión de caja diaria, préstamos a clientes sin intereses para fidelización, y generación de reportes detallados.

### 1.2 Problema a Resolver
El negocio "Comercial Aliaga" necesita digitalizar y centralizar el control de:
- Compras diarias de granos a productores locales
- Registro de clientes proveedores por zonas geográficas
- Control de caja diaria con apertura/cierre y arqueo
- Préstamos sin intereses a clientes para fidelización
- Ventas de granos procesados
- Generación de reportes y exportación a Excel
- Emisión de vouchers de compra

### 1.3 Alcance del MVP
Aplicación de escritorio standalone (Windows) que funciona offline con base de datos local SQLite, con capacidad de consulta a API externa (RENIEC) para validación de clientes cuando hay conexión a internet.

---

## 2. USUARIOS Y ROLES

### 2.1 Usuario Único
**Rol:** Dueño/Operador único  
**Acceso:** Sin sistema de autenticación (uso personal en laptop dedicada)  
**Permisos:** Acceso completo a todas las funcionalidades

---

## 3. ARQUITECTURA TÉCNICA

### 3.1 Stack Tecnológico
- **Backend:** .NET 10 (API)
- **Frontend:** Angular
- **Base de Datos:** SQLite (local)
- **Arquitectura:** Cliente-Servidor en misma máquina (standalone)
- **Impresión:** Compatible con impresoras térmicas estándar 80mm

### 3.2 Infraestructura
- **Deployment:** Aplicación de escritorio en Windows
- **Base de Datos:** Archivo SQLite local en laptop
- **Conectividad:** 
  - Offline por defecto
  - Internet requerido solo para consultas a API RENIEC
- **Backup:** Exportación manual de archivo SQLite con timestamp

### 3.3 Integraciones Externas
- **API RENIEC:** Consulta de datos de ciudadanos peruanos por DNI de 8 dígitos
  - Configuración a cargo del cliente
  - Fallback a registro manual si API no responde

---

## 4. MODELO DE DATOS

### 4.1 Entidades Principales

#### 4.1.1 Zona
```
- Id (PK)
- Nombre (único, varchar)
- CantidadClientes (calculado)
- FechaCreacion
- FechaModificacion
```

#### 4.1.2 ClienteProveedor
```
- Id (PK)
- DNI (único, 8 dígitos, obligatorio)
- NombreCompleto (obligatorio)
- Telefono (opcional)
- Direccion (opcional)
- FechaNacimiento (opcional)
- ZonaId (FK, opcional)
- SaldoPrestamo (decimal, default: 0)
- EsAnonimo (boolean, default: false)
- FechaCreacion
- FechaModificacion
```
**Cliente Especial:** DNI 00000000 - "Anónimo" (precargado, inmutable, sin zona)

#### 4.1.3 ClienteComprador
```
- Id (PK)
- Nombre (obligatorio, varchar)
- FechaCreacion
- FechaModificacion
```

#### 4.1.4 Producto
```
- Id (PK)
- Nombre (enum: Café, Cacao, Maíz, Achiote)
- PrecioSugeridoPorKg (decimal)
- NivelesSecado (JSON/array)
- Calidades (JSON/array)
- PermiteValdeo (boolean)
- FechaModificacion
```

**Configuración por Producto:**

| Producto | Niveles Secado | Calidades | Permite Valdeo | Tipo Pesado |
|----------|----------------|-----------|----------------|-------------|
| Café | Mote-baba, Húmedo, Estándar, Seco | Bajo, Medio, Alto | Sí | Kg / Valdeo |
| Cacao | Mote-baba, Húmedo, Estándar, Seco | Normal, Alto | Sí | Kg / Valdeo |
| Maíz | Estándar (fijo) | Normal (fijo) | No | Kg (fijo) |
| Achiote | Estándar (fijo) | Normal (fijo) | No | Kg (fijo) |

#### 4.1.5 Caja
```
- Id (PK)
- Fecha (date, único por día)
- MontoInicial (decimal, obligatorio)
- MontoEsperado (calculado)
- ArqueoReal (decimal, nullable hasta cierre)
- Diferencia (decimal, calculado: ArqueoReal - MontoEsperado)
- Estado (enum: Abierta, CerradaManual, CerradaAutomatica)
- FechaApertura (timestamp)
- FechaCierre (timestamp, nullable)
- UsuarioApertura
- UsuarioCierre
```

#### 4.1.6 Compra
```
- Id (PK)
- NumeroVoucher (único, correlativo global, 8 dígitos)
- ClienteProveedorId (FK, obligatorio)
- ProductoId (FK, obligatorio)
- CajaId (FK, obligatorio)
- NivelSecado (varchar)
- Calidad (varchar)
- TipoPesado (enum: Kg, Valdeo)
- PesoBruto (decimal, obligatorio)
- DescuentoKg (decimal, default: 0)
- PesoNeto (calculado: PesoBruto - DescuentoKg)
- PrecioPorKg (decimal, obligatorio)
- MontoTotal (calculado: PesoNeto * PrecioPorKg)
- FechaCompra (timestamp)
- Editada (boolean, default: false)
- FechaEdicion (timestamp, nullable)
- EsAjustePosterior (boolean, default: false)
```

#### 4.1.7 Venta
```
- Id (PK)
- ClienteCompradorId (FK, obligatorio)
- ProductoId (FK, obligatorio)
- CajaId (FK, obligatorio)
- PesoBruto (decimal, ingreso manual)
- PesoNeto (decimal, ingreso manual)
- PrecioPorKg (decimal, ingreso manual)
- MontoTotal (decimal, ingreso manual)
- FechaVenta (timestamp)
- Editada (boolean, default: false)
- FechaEdicion (timestamp, nullable)
- EsAjustePosterior (boolean, default: false)
```

#### 4.1.8 Prestamo
```
- Id (PK)
- ClienteProveedorId (FK, obligatorio)
- CajaId (FK, obligatorio)
- TipoMovimiento (enum: Prestamo, Abono)
- Monto (decimal, obligatorio)
- Descripcion (varchar, opcional)
- FechaMovimiento (timestamp)
- SaldoDespues (decimal, calculado)
- EsAjustePosterior (boolean, default: false)
```

#### 4.1.9 MovimientoCaja
```
- Id (PK)
- CajaId (FK, obligatorio)
- TipoMovimiento (enum: Compra, Venta, Prestamo, Abono, Inyeccion, Retiro, GastoOperativo)
- ReferenciaId (int, nullable - ID de Compra/Venta/Prestamo)
- Concepto (varchar)
- Monto (decimal, obligatorio)
- TipoOperacion (enum: Ingreso, Egreso)
- FechaMovimiento (timestamp)
- EsAjustePosterior (boolean, default: false)
```

#### 4.1.10 ConfiguracionNegocio
```
- Id (PK, único registro)
- NombreNegocio (hardcoded: "Comercial Aliaga")
- Direccion (hardcoded)
- Telefono (hardcoded)
- RUC (hardcoded)
- MensajeVoucher (hardcoded, editable en futuro)
- ContadorVoucher (int, inicia en 1)
```

---

## 5. FUNCIONALIDADES PRINCIPALES

### 5.1 Módulo: Dashboard / Inicio

#### 5.1.1 Información Mostrada
- **Estado de Caja:**
  - Abierta/Cerrada
  - Monto inicial (si abierta)
  - Saldo actual esperado (si abierta)
  
- **Resumen del Día:**
  - Total ingresos
  - Total egresos
  - Número de compras
  - Número de ventas
  - Monto total prestado (activo)

- **Alertas:**
  - Top 5 clientes con mayor deuda
  - Caja del día anterior sin cerrar (si aplica)

- **Accesos Rápidos:**
  - Abrir Caja
  - Registrar Compra
  - Registrar Préstamo

---

### 5.2 Módulo: Caja

#### 5.2.1 Funcionalidad: Abrir Caja
**Validaciones:**
- Solo una caja activa por día
- Si existe caja abierta del mismo día, mostrar alerta y no permitir
- Si existe caja no cerrada de día anterior, cerrarla automáticamente con:
  - Estado: "CerradaAutomatica"
  - ArqueoReal = MontoEsperado
  - Diferencia = (calculada si hay descuadre)

**Flujo:**
1. Verificar que no haya caja abierta del día
2. Verificar cajas anteriores sin cerrar → cerrar automáticamente
3. Solicitar Monto Inicial (obligatorio)
4. Crear registro de Caja con estado "Abierta"
5. Registrar fecha/hora de apertura
6. Mostrar modal de confirmación exitosa
7. Redirigir a Dashboard

#### 5.2.2 Funcionalidad: Cerrar Caja
**Disponible:** Solo si hay caja abierta del día actual

**Flujo:**
1. Calcular automáticamente:
   - Monto Inicial
   - Total Ingresos (compras + abonos + inyecciones)
   - Total Egresos (ventas + préstamos + retiros + gastos)
   - Saldo Esperado = Inicial + Ingresos - Egresos

2. Solicitar Arqueo Real (monto físico contado)

3. Calcular Diferencia = Arqueo Real - Saldo Esperado

4. Si Diferencia ≠ 0:
   - Mostrar modal de confirmación:
     ```
     Saldo Esperado: S/ X.XX
     Arqueo Real: S/ Y.YY
     Diferencia: S/ Z.ZZ (Faltante/Sobrante)
     ¿Está seguro de cerrar con esta diferencia?
     ```

5. Al confirmar:
   - Actualizar estado a "CerradaManual"
   - Registrar ArqueoReal y Diferencia
   - Registrar fecha/hora cierre
   - Mostrar confirmación exitosa
   - Redirigir a Dashboard

#### 5.2.3 Funcionalidad: Reabrir Caja
**Disponible:** Solo si la caja fue cerrada el mismo día

**Flujo:**
1. Cambiar estado a "Abierta"
2. Limpiar fecha de cierre
3. Mantener todos los movimientos registrados
4. Permitir continuar operaciones

#### 5.2.4 Funcionalidad: Registrar Movimientos de Caja
**Tipos de Movimientos Directos:**

**A. Inyección de Dinero**
- Aumenta el saldo de caja
- Sin límite de monto
- Campos: Monto, Descripción (opcional)

**B. Retiro de Dinero**
- Disminuye el saldo de caja
- Validación: No puede retirar más del saldo disponible
- Campos: Monto, Descripción (opcional)

**C. Gasto Operativo**
- Disminuye el saldo de caja
- Sin categorización (descripción libre)
- Campos: Monto, Descripción (obligatorio)

**Validaciones Generales:**
- Requiere caja abierta
- Movimientos inmutables (no se pueden eliminar)

#### 5.2.5 Vista: Caja Actual
**Información en Tiempo Real:**
- Monto Inicial
- Total Ingresos
- Total Egresos
- Saldo Esperado Actual
- Botones de acción según estado

#### 5.2.6 Vista: Historial de Cajas
**Listado:**
- Fecha
- Monto Inicial
- Saldo Final
- Diferencia (si cerrada)
- Estado (Abierta/CerradaManual/CerradaAutomatica)
- Acciones: Ver Detalle

**Vista Detalle de Caja Histórica:**
- Resumen de totales
- Lista de todos los movimientos del día (paginado 50 registros)
- Si tiene diferencia ≠ 0: Botón "Agregar Ajuste Posterior"

#### 5.2.7 Funcionalidad: Ajustes Posteriores
**Disponible:** Solo en cajas cerradas con diferencia

**Flujo:**
1. Acceder a caja histórica con diferencia
2. Botón "Agregar Ajuste Posterior"
3. Permite registrar cualquier tipo de operación:
   - Compra
   - Venta
   - Préstamo
   - Abono
   - Inyección
   - Retiro
   - Gasto Operativo
4. Todas marcadas con `EsAjustePosterior = true`
5. Recalcula diferencia de esa caja específica
6. NO afecta caja actual
7. Objetivo: llevar diferencia a 0 para regularizar

---

### 5.3 Módulo: Compras

#### 5.3.1 Funcionalidad: Registrar Compra

**Validaciones Previas:**
- Requiere caja abierta del día actual

**Flujo Completo:**

**Paso 1: Seleccionar/Registrar Cliente**
1. Campo de búsqueda con autocompletado en tiempo real
   - Búsqueda por DNI o Nombre (coincidencias parciales)
   - Muestra primeros 5 resultados
   - Formato: "Nombre Completo - DNI"

2. Si DNI existe en BD → cargar datos automáticamente

3. Si DNI NO existe en BD → Botón "Buscar en RENIEC"
   - Consultar API RENIEC
   - Si API responde → autocompletar Nombre
   - Si API falla → permitir registro manual

4. Registro de Cliente Nuevo:
   - DNI (obligatorio, 8 dígitos)
   - Nombre Completo (obligatorio)
   - Zona (selección de lista, obligatorio)
   - Teléfono (opcional)
   - Dirección (opcional)
   - Fecha de Nacimiento (opcional)

**Paso 2: Alerta de Préstamo**
- Si cliente tiene saldo de préstamo > 0:
  - Mostrar banner destacado arriba del formulario:
    ```
    ⚠️ Cliente tiene préstamo pendiente: S/ X.XX
    ```
  - Banner siempre visible durante el registro
  - No bloquea operación (solo informativo)

**Paso 3: Datos de la Compra**

1. **Seleccionar Producto** (obligatorio)
   - Dropdown: Café, Cacao, Maíz, Achiote

2. **Características según Producto:**
   
   **Para Café/Cacao:**
   - Nivel de Secado (dropdown): Mote-baba, Húmedo, Estándar, Seco
   - Tipo de Pesado (dropdown): Kg, Valdeo
   - Calidad:
     - Café: Bajo, Medio, Alto
     - Cacao: Normal, Alto
   
   **Para Maíz/Achiote:**
   - Nivel de Secado: "Estándar" (campo deshabilitado)
   - Tipo de Pesado: "Kg" (campo deshabilitado)
   - Calidad: "Normal" (campo deshabilitado)

3. **Peso y Precio:**
   - Peso Bruto (kg, obligatorio, 1 decimal, formato: 150.5)
   - Descuento en Kg (opcional, default: 0, 1 decimal)
   - Precio por Kg (obligatorio, 2 decimales, autocompletado del precio sugerido, editable)

**Paso 4: Resumen Pre-Confirmación**
- Mostrar vista de resumen:
  ```
  Producto: Café
  Peso Bruto: 150.5 kg
  Descuento: -2.5 kg
  -------------------------
  Peso Neto: 148.0 kg
  Precio: S/ 8.50 / kg
  -------------------------
  TOTAL A PAGAR: S/ 1,258.00
  ```

**Paso 5: Confirmación**
1. Botón "Confirmar y Generar Voucher"
2. Guardar registro de Compra
3. Incrementar contador de voucher
4. Registrar movimiento en Caja (Ingreso)
5. Generar e imprimir voucher automáticamente
6. Mostrar modal "Operación exitosa"
7. Redirigir a listado de Compras

**Manejo de Errores:**
- Si falla guardado → mantener formulario con datos
- Mostrar: "Error al guardar compra"
- Permitir reintentar

#### 5.3.2 Funcionalidad: Editar Compra

**Disponible:** Solo compras del día actual

**Campos Editables:**
- Peso Bruto
- Descuento en Kg
- Precio por Kg

**Campos NO Editables:**
- Cliente
- Producto
- Características (secado, calidad, tipo pesado)
- Fecha

**Flujo:**
1. Desde lista de compras → acción "Editar"
2. Modal de confirmación: "¿Está seguro de editar esta compra?"
3. Formulario con campos editables
4. Al guardar:
   - Marcar `Editada = true`
   - Registrar `FechaEdicion`
   - Recalcular PesoNeto y MontoTotal
   - Actualizar movimiento en Caja
   - Voucher mantiene mismo número (puede reimprimirse)
5. Modal de confirmación exitosa
6. Volver a lista

#### 5.3.3 Vista: Lista de Compras

**Columnas:**
- Fecha/Hora
- N° Voucher
- Cliente
- Producto
- Peso Neto (kg)
- Total (S/)
- Estado (ícono si editada)
- Acciones

**Funcionalidades:**
- Ordenamiento: Más recientes primero
- Paginación: 50 registros por página
- Acciones por registro:
  - Ver detalle
  - Editar (solo día actual)
  - Reimprimir voucher

**Filtros:** (para reportes, ver sección 5.9)

#### 5.3.4 Vista: Detalle de Compra

**Información Completa:**
- Número de voucher
- Fecha y hora
- Cliente (nombre, DNI, zona)
- Producto y todas sus características
- Peso bruto, descuento, peso neto
- Precio por kg
- Total
- Estado (si fue editada, mostrar fecha de edición)
- Botón: Reimprimir Voucher

---

### 5.4 Módulo: Ventas

#### 5.4.1 Funcionalidad: Registrar Venta

**Validaciones Previas:**
- Requiere caja abierta del día actual

**Flujo:**

**Paso 1: Seleccionar/Registrar Cliente Comprador**
1. Dropdown simple (pocos clientes, 3-4)
2. Opción: "+ Nuevo Cliente"
3. Registro simple:
   - Nombre (obligatorio, texto libre)
   - Sin validación de duplicados

**Paso 2: Datos de la Venta**

1. **Seleccionar Producto** (obligatorio)
   - Dropdown: Café, Cacao, Maíz, Achiote

2. **Pesos y Precio (TODO MANUAL):**
   - Peso Bruto (kg, obligatorio, ingreso manual)
   - Peso Neto (kg, obligatorio, ingreso manual)
   - Precio por Kg (obligatorio, ingreso manual)
   - Total (S/, obligatorio, ingreso manual)

**Nota:** NO hay cálculos automáticos ni validaciones entre campos. El dueño controla manualmente todo.

**Paso 3: Confirmación**
1. Botón "Confirmar Venta"
2. Guardar registro de Venta
3. Registrar movimiento en Caja (Egreso)
4. Mostrar modal "Operación exitosa"
5. Redirigir a listado de Ventas

**Manejo de Errores:**
- Si falla guardado → mantener formulario con datos
- Mostrar: "Error al guardar venta"

#### 5.4.2 Funcionalidad: Editar Venta

**Disponible:** Solo ventas del día actual

**Campos Editables:**
- Peso Bruto
- Peso Neto
- Precio por Kg
- Total

**Flujo:**
1. Modal de confirmación: "¿Está seguro de editar esta venta?"
2. Formulario con campos editables
3. Al guardar:
   - Marcar `Editada = true`
   - Registrar `FechaEdicion`
   - Actualizar movimiento en Caja
4. Modal de confirmación exitosa

#### 5.4.3 Vista: Lista de Ventas

**Columnas:**
- Fecha/Hora
- Cliente
- Producto
- Peso Neto (kg)
- Total (S/)
- Estado (ícono si editada)
- Acciones

**Funcionalidades:**
- Ordenamiento: Más recientes primero
- Paginación: 50 registros por página
- Acciones:
  - Ver detalle
  - Editar (solo día actual)

---

### 5.5 Módulo: Clientes

#### 5.5.1 Vista: Listado de Clientes Proveedores

**Navegación:** Pestaña "Proveedores" (activa por defecto)

**Búsqueda:**
- Autocompletado en tiempo real
- Por DNI o Nombre (coincidencias parciales)
- Primeros 5 resultados mientras escribe

**Columnas:**
- DNI
- Nombre Completo
- Zona
- Teléfono
- Saldo Préstamo (S/)
- Acciones

**Funcionalidades:**
- Ordenamiento: Más recientes primero
- Paginación: 50 registros
- Filtro por Zona (dropdown)
- Acciones:
  - Ver detalle
  - Editar

#### 5.5.2 Vista: Detalle de Cliente Proveedor

**Información General:**
- DNI
- Nombre Completo
- Zona
- Teléfono
- Dirección
- Fecha de Nacimiento
- Fecha de Registro
- Botón: Editar Datos

**Sección: Estado de Préstamo**
- Saldo Actual Pendiente: S/ X.XX
- Último Movimiento:
  - Fecha: DD/MM/YYYY
  - Tipo: Préstamo / Abono
  - Monto: S/ X.XX

**Sección: Historial de Compras**
- Filtros:
  - Rango de fechas
  - Producto
- Paginación: 50 registros
- Columnas:
  - Fecha
  - Producto
  - Calidad
  - Nivel Secado
  - Tipo Pesado
  - Peso Neto (kg)
  - Precio/kg
  - Total (S/)
- Totales Acumulados:
  - Total Kg Comprados: X,XXX.X kg
  - Monto Total Histórico: S/ X,XXX.XX

**Sección: Historial de Préstamos**
- Columnas:
  - Fecha
  - Tipo (Préstamo / Abono)
  - Monto (S/)
  - Descripción
- Ordenamiento: Más reciente primero
- Paginación: 50 registros

#### 5.5.3 Funcionalidad: Editar Cliente Proveedor

**Campos Editables:**
- Nombre Completo
- Zona
- Teléfono
- Dirección
- Fecha de Nacimiento

**Campos NO Editables:**
- DNI
- Saldo Préstamo (se modifica solo desde módulo Préstamos)

**Validación:**
- Si DNI ya existe en otro cliente → error (no debería ocurrir)

#### 5.5.4 Vista: Listado de Clientes Compradores

**Navegación:** Pestaña "Compradores"

**Funcionalidades:**
- Lista simple (pocos registros: 3-4 clientes)
- Columnas:
  - Nombre
  - Fecha de Registro
  - Acciones: Editar

**Edición:**
- Solo campo: Nombre

---

### 5.6 Módulo: Préstamos

#### 5.6.1 Vista: Listado de Préstamos Activos

**Columnas:**
- Nombre Cliente
- DNI
- Saldo Actual Pendiente (S/)
- Fecha Último Movimiento
- Acciones

**Funcionalidades:**
- Ordenamiento: Más recientes primero
- Paginación: 50 registros
- Búsqueda: Por DNI o Nombre
- Filtro: Mostrar solo con saldo > 0 (por defecto)
- Acciones:
  - Nuevo Préstamo
  - Registrar Abono
  - Ver Historial

#### 5.6.2 Funcionalidad: Registrar Nuevo Préstamo

**Validaciones Previas:**
- Requiere caja abierta del día actual

**Flujo:**
1. Seleccionar Cliente (autocompletado)
2. Mostrar saldo actual si > 0
3. Ingresar:
   - Monto del Préstamo (obligatorio)
   - Descripción (opcional)
4. Confirmar
5. Acciones del sistema:
   - Crear registro en Prestamo (TipoMovimiento: Prestamo)
   - Incrementar SaldoPrestamo del cliente
   - Registrar movimiento en Caja (Egreso)
   - Calcular SaldoDespues
6. Modal de confirmación exitosa
7. Volver a lista de préstamos

**Sin Validaciones:**
- No hay monto mínimo ni máximo
- No valida si cliente ya tiene préstamo alto
- El dueño decide libremente

#### 5.6.3 Funcionalidad: Registrar Abono

**Validaciones Previas:**
- Requiere caja abierta del día actual
- Cliente debe tener saldo > 0

**Flujo:**
1. Seleccionar Cliente con préstamo pendiente
2. Mostrar Saldo Actual: S/ X.XX
3. Ingresar Monto del Abono
4. Validación: Monto ≤ Saldo Actual
   - Si monto > saldo → error: "El monto del abono no puede ser mayor al saldo pendiente"
5. Descripción (opcional)
6. Confirmar
7. Acciones del sistema:
   - Crear registro en Prestamo (TipoMovimiento: Abono)
   - Reducir SaldoPrestamo del cliente
   - Registrar movimiento en Caja (Ingreso)
   - Calcular SaldoDespues
8. Modal de confirmación exitosa
9. Volver a lista de préstamos

#### 5.6.4 Vista: Historial de Préstamos de Cliente

**Información:**
- Datos del cliente
- Saldo Actual Pendiente: S/ X.XX
- Último Movimiento (fecha, tipo, monto)

**Tabla de Movimientos:**
- Columnas:
  - Fecha
  - Tipo (Préstamo / Abono)
  - Monto (S/)
  - Descripción
- Ordenamiento: Más reciente primero
- Paginación: 50 registros

---

### 5.7 Módulo: Productos

#### 5.7.1 Vista: Lista de Productos

**Productos Fijos (no se pueden agregar/eliminar):**
1. Café
2. Cacao
3. Maíz
4. Achiote

**Columnas:**
- Nombre Producto
- Precio Sugerido (S/ / kg)
- Acciones: Editar Precio

#### 5.7.2 Funcionalidad: Editar Precio Sugerido

**Flujo:**
1. Click en "Editar Precio"
2. Modal simple:
   - Producto: [Nombre] (readonly)
   - Precio Actual: S/ X.XX
   - Nuevo Precio: [input]
3. Validación: Precio > 0
4. Confirmar
5. Actualizar PrecioSugeridoPorKg
6. Registrar FechaModificacion
7. Modal de confirmación
8. Actualizar lista

**Nota:** Este precio se autocompleta al registrar compras, pero siempre es editable por operación.

---

### 5.8 Módulo: Zonas

#### 5.8.1 Vista: Lista de Zonas

**Columnas:**
- Nombre Zona
- Cantidad de Clientes
- Acciones

**Funcionalidades:**
- Búsqueda por nombre
- Ordenamiento: Alfabético por nombre
- Paginación: 50 registros
- Acciones:
  - Ver Clientes
  - Editar (inline)
  - NO hay eliminación

#### 5.8.2 Funcionalidad: Crear Nueva Zona

**Flujo:**
1. Botón "+ Nueva Zona"
2. Modal simple:
   - Nombre (obligatorio, único)
3. Validación: No debe existir zona con ese nombre exacto
4. Confirmar
5. Modal de confirmación
6. Actualizar lista

#### 5.8.3 Funcionalidad: Editar Zona (Inline)

**Flujo:**
1. Click en nombre de zona
2. Campo se vuelve editable
3. Al perder foco o Enter:
   - Validar unicidad
   - Guardar cambio
   - Actualizar FechaModificacion

#### 5.8.4 Funcionalidad: Ver Clientes de Zona

**Drill-down:**
1. Click en "Ver Clientes" o en cantidad
2. Mostrar lista filtrada de clientes de esa zona
3. Misma vista que listado de clientes, pero pre-filtrada

---

### 5.9 Módulo: Reportes

#### 5.9.1 Funcionalidad General de Reportes

**Flujo Común:**
1. Seleccionar tipo de reporte
2. Aplicar filtros específicos del reporte
3. Botón "Generar Reporte"
4. Vista previa en tabla (pantalla)
5. Botón "Exportar a Excel"
6. Descarga archivo con timestamp: `reporte_[tipo]_YYYYMMDD_HHMMSS.xlsx`

**Formato Excel:**
- Simple: solo encabezados y bordes
- Sin estilos elaborados
- Formato de números con separador de miles: 1,250.50

#### 5.9.2 Reporte: Compras por Cliente

**Filtros:**
- Rango de Fechas (obligatorio)
- Cliente Específico (opcional)
- Producto (opcional)
- Zona (opcional)

**Columnas:**
- Fecha
- Cliente
- DNI
- Zona
- Producto
- Calidad
- Nivel Secado
- Tipo Pesado
- Peso Neto (kg)
- Precio/kg (S/)
- Total (S/)

**Totales:**
- Total Kg: X,XXX.X kg
- Total Monto: S/ X,XXX.XX

**Agrupación:** Si se filtra por cliente, subtotales por cliente

#### 5.9.3 Reporte: Compras por Producto

**Filtros:**
- Rango de Fechas (obligatorio)
- Producto (opcional - si no se selecciona, muestra todos)

**Columnas:**
- Producto
- Calidad
- Nivel Secado
- Tipo Pesado
- Cantidad Total (kg)
- Monto Total (S/)

**Agrupación:**
- Por Producto → Calidad → Tipo Pesado

**Totales Generales:**
- Total Kg por Producto
- Monto Total por Producto
- Total General

#### 5.9.4 Reporte: Resumen por Zonas

**Filtros:**
- Rango de Fechas (obligatorio)
- Zona Específica (opcional)
- Producto (opcional)

**Columnas:**
- Zona
- Número de Clientes (activos en el período)
- Número de Compras
- Kg Totales
- Monto Total (S/)

**Si se filtra por Producto:** añadir columna "Producto"

**Totales:**
- Total Clientes Únicos
- Total Compras
- Total Kg
- Monto Total

#### 5.9.5 Reporte: Movimientos de Caja

**Filtros:**
- Rango de Fechas (obligatorio)

**Columnas:**
- Fecha
- Tipo de Movimiento (Compra, Venta, Préstamo, Abono, Inyección, Retiro, Gasto Operativo)
- Concepto/Descripción
- Ingreso (S/)
- Egreso (S/)

**Separación por Tipo:** Opcional - checkbox "Agrupar por tipo"

**Totales:**
- Total Ingresos por Tipo
- Total Egresos por Tipo
- Total Ingresos General
- Total Egresos General
- Diferencia Neta

#### 5.9.6 Reporte: Ventas

**Filtros:**
- Rango de Fechas (obligatorio)
- Producto (opcional)
- Cliente Comprador (opcional)

**Columnas:**
- Fecha
- Cliente
- Producto
- Peso Bruto (kg)
- Peso Neto (kg)
- Precio/kg (S/)
- Total (S/)

**Totales por Producto:**
- Kg Totales por Producto
- Monto Total por Producto

**Totales Generales:**
- Total Kg
- Total Monto

---

### 5.10 Funcionalidad: Voucher de Compra

#### 5.10.1 Formato y Contenido

**Tipo:** Ticket térmico 80mm

**Estructura:**

```
================================
    COMERCIAL ALIAGA
RUC: [XXXXXXXXXX]
[Dirección]
[Teléfono]
================================

VOUCHER DE COMPRA
N° 00000123

Fecha: 18/12/2025 14:35:20
================================

Cliente: Juan Pérez García
DNI: 12345678

--------------------------------
Producto: Café

Peso Bruto:     150.5 kg
Descuento:       -2.5 kg
                --------
Peso Neto:      148.0 kg

Precio x kg:  S/ 8.50
                ========
TOTAL:       S/ 1,258.00
================================

[Mensaje del Negocio]
(Por definir)

================================
        GRACIAS POR SU VENTA
================================
```

**Características:**
- Fuente: Monospace/Courier (compatible con impresoras térmicas)
- Ancho: 32 caracteres
- Alineación: Centrado para encabezado, izquierda para datos, derecha para montos
- Separadores: líneas simples (=, -)
- Formato de montos: S/ 1,258.00 (con separador de miles)

#### 5.10.2 Numeración de Vouchers

**Sistema:**
- Correlativo global único
- 8 dígitos: 00000001, 00000002, ...
- Inicia en 1
- Almacenado en ConfiguracionNegocio.ContadorVoucher
- Se incrementa al confirmar cada compra

**Reimpresión:**
- Mantiene número original
- Incluye marca "DUPLICADO" en encabezado:
  ```
  VOUCHER DE COMPRA - DUPLICADO
  N° 00000123
  ```

#### 5.10.3 Funcionalidad: Imprimir Voucher

**Triggers:**
1. Automático al confirmar compra
2. Manual desde:
   - Lista de compras → botón "Reimprimir"
   - Detalle de compra → botón "Reimprimir Voucher"

**Proceso:**
1. Generar contenido del voucher (HTML o comando ESC/POS)
2. Enviar a impresora térmica configurada
3. No requiere confirmación de impresión exitosa (confianza en hardware)

**Compatibilidad:**
- Impresoras térmicas estándar 80mm (ESC/POS compatible)
- Drivers del sistema operativo

---

### 5.11 Funcionalidad: Backup

#### 5.11.1 Exportar Base de Datos

**Ubicación:** Menú principal o módulo de Configuración

**Flujo:**
1. Click en "Exportar Backup"
2. Sistema genera copia del archivo SQLite
3. Nombre: `comercial_aliaga_backup_YYYYMMDD_HHMMSS.db`
4. Abrir diálogo de sistema para seleccionar ubicación de guardado
5. Usuario selecciona carpeta (USB, disco local, etc.)
6. Copiar archivo
7. Modal de confirmación: "Backup generado exitosamente"

**Frecuencia:** Manual, a demanda del usuario

**Nota:** Restauración la realiza personal técnico directamente en el sistema operativo.

---

## 6. REGLAS DE NEGOCIO Y VALIDACIONES

### 6.1 Reglas Generales

#### 6.1.1 Fecha y Hora del Sistema
- Todas las operaciones usan fecha/hora del sistema operativo
- Cambio de día: 00:00 (medianoche)
- Sin validación automática de fecha incorrecta (confianza en capacitación del usuario)

#### 6.1.2 Formatos Numéricos
- **Pesos:** 1 decimal (múltiplos de 0.1): 150.5 kg, 200.7 kg
- **Precios:** 2 decimales: S/ 8.50
- **Totales:** 2 decimales: S/ 1,258.00
- **Separador decimal:** Punto (.)
- **Separador de miles:** Coma (,)

#### 6.1.3 Moneda
- Única: Soles Peruanos (PEN)
- Formato: S/ 1,250.50
- Sin conversión de monedas

### 6.2 Reglas de Caja

#### R-CAJ-001: Una Caja Activa por Día
- Solo puede existir una caja con estado "Abierta" por fecha
- Al intentar abrir caja si ya existe una del mismo día → error

#### R-CAJ-002: Cierre Automático
- Si al abrir caja del día actual existe caja sin cerrar de día anterior:
  - Cerrar automáticamente la anterior
  - Estado: "CerradaAutomatica"
  - ArqueoReal = MontoEsperado
  - Calcular Diferencia si hay descuadre

#### R-CAJ-003: Requerimiento de Caja Abierta
- Todas estas operaciones requieren caja abierta del día actual:
  - Registrar compra
  - Registrar venta
  - Registrar préstamo
  - Registrar abono
  - Inyección de dinero
  - Retiro de dinero
  - Gasto operativo
- Si no hay caja abierta → error: "Debe abrir la caja del día para realizar esta operación"

#### R-CAJ-004: Validación de Retiros
- Retiro de dinero ≤ Saldo disponible en caja
- Si intenta retirar más → error: "El monto a retirar supera el saldo disponible en caja"

#### R-CAJ-005: Inyecciones Sin Límite
- Inyecciones de dinero no tienen límite máximo

#### R-CAJ-006: Movimientos Inmutables
- Movimientos de caja NO se pueden eliminar
- Solo se pueden agregar nuevos movimientos

#### R-CAJ-007: Cálculo de Saldo Esperado
```
SaldoEsperado = MontoInicial 
              + TotalCompras 
              + TotalAbonos 
              + TotalInyecciones 
              - TotalVentas 
              - TotalPrestamos 
              - TotalRetiros 
              - TotalGastosOperativos
```

#### R-CAJ-008: Cierre con Diferencia
- Se permite cerrar caja aunque Diferencia ≠ 0
- Requiere confirmación explícita del usuario
- Diferencia se registra para posterior regularización

#### R-CAJ-009: Reapertura de Caja
- Solo se puede reabrir caja cerrada del mismo día
- Cajas de días anteriores NO se pueden reabrir

#### R-CAJ-010: Ajustes Posteriores
- Solo disponible en cajas cerradas
- Operaciones marcadas con `EsAjustePosterior = true`
- NO afectan caja actual
- Objetivo: llevar Diferencia a 0

### 6.3 Reglas de Clientes

#### R-CLI-001: DNI Obligatorio para Proveedores
- Todo cliente proveedor debe tener DNI de 8 dígitos
- DNI debe ser único en la base de datos

#### R-CLI-002: Cliente Anónimo
- Existe cliente predefinido:
  - DNI: 00000000
  - Nombre: "Anónimo"
  - Sin zona
  - Inmutable (no se puede editar ni eliminar)
- Aparece en reportes normalmente

#### R-CLI-003: Búsqueda de DNI - Prioridad
1. Buscar en BD local
2. Si no existe → consultar API RENIEC
3. Si API falla → permitir registro manual

#### R-CLI-004: Clientes Compradores
- Solo requieren Nombre (sin DNI)
- Pueden existir duplicados (no hay validación)
- Pocos clientes (3-4 típicamente)

#### R-CLI-005: Zonas Opcionales
- Cliente proveedor puede no tener zona asignada
- Si se asigna zona, debe existir en catálogo

### 6.4 Reglas de Compras

#### R-COM-001: Cálculo de Peso Neto
```
PesoNeto = PesoBruto - DescuentoKg
```

#### R-COM-002: Cálculo de Total
```
MontoTotal = PesoNeto × PrecioPorKg
```
- Resultado con 2 decimales
- Redondeo: estándar (0.5 redondea hacia arriba)

#### R-COM-003: Características según Producto
| Producto | Secado | Calidad | Tipo Pesado |
|----------|--------|---------|-------------|
| Café | Seleccionable: Mote-baba, Húmedo, Estándar, Seco | Seleccionable: Bajo, Medio, Alto | Seleccionable: Kg, Valdeo |
| Cacao | Seleccionable: Mote-baba, Húmedo, Estándar, Seco | Seleccionable: Normal, Alto | Seleccionable: Kg, Valdeo |
| Maíz | Fijo: Estándar | Fijo: Normal | Fijo: Kg |
| Achiote | Fijo: Estándar | Fijo: Normal | Fijo: Kg |

#### R-COM-004: Precio Sugerido
- Al seleccionar producto, autocompletar con PrecioSugeridoPorKg
- Usuario puede modificar el precio en cada compra
- Modificación no afecta precio sugerido del producto

#### R-COM-005: Voucher Automático
- Al confirmar compra, generar e imprimir voucher automáticamente
- Número correlativo único e inmutable

#### R-COM-006: Edición Limitada
- Solo se pueden editar compras del mismo día
- Campos editables: Peso, Descuento, Precio
- Campos NO editables: Cliente, Producto, Características
- Al editar, marcar registro como `Editada = true`

#### R-COM-007: Alerta de Préstamo
- Si cliente tiene SaldoPrestamo > 0:
  - Mostrar banner destacado durante registro de compra
  - No bloquea operación (solo informativo)
- Banner se mantiene visible todo el proceso

### 6.5 Reglas de Ventas

#### R-VEN-001: Sin Cálculos Automáticos
- PesoBruto, PesoNeto, PrecioPorKg, MontoTotal son ingreso manual
- Sin validación de coherencia entre campos
- Confianza total en el usuario

#### R-VEN-002: Sin Voucher
- Ventas NO generan voucher impreso
- Solo registro en sistema

#### R-VEN-003: Sin Características
- Ventas no registran calidad, secado, ni tipo de pesado
- Solo: Cliente, Producto, Pesos, Precio, Total

#### R-VEN-004: Edición Limitada
- Solo se pueden editar ventas del mismo día
- Todos los campos numéricos son editables
- Al editar, marcar como `Editada = true`

### 6.6 Reglas de Préstamos

#### R-PRE-001: Un Saldo por Cliente
- Cliente tiene un único saldo acumulado (SaldoPrestamo)
- Nuevos préstamos incrementan el saldo existente
- Abonos reducen el saldo

#### R-PRE-002: Sin Límites en Préstamos
- No hay monto mínimo ni máximo para préstamos
- No hay validación de cantidad de préstamos activos
- El dueño decide libremente

#### R-PRE-003: Validación de Abonos
```
MontoAbono ≤ SaldoPrestamo
```
- Si MontoAbono > SaldoPrestamo → error: "El monto del abono no puede ser mayor al saldo pendiente"

#### R-PRE-004: Sin Intereses
- Préstamos son sin interés
- Objetivo: fidelización de clientes proveedores

#### R-PRE-005: Movimientos en Caja
- Préstamo → Egreso de caja
- Abono → Ingreso de caja
- Ambos requieren caja abierta

#### R-PRE-006: Cálculo de Saldo
```
SaldoDespues = SaldoAnterior + MontoPrestamo
SaldoDespues = SaldoAnterior - MontoAbono
```

### 6.7 Reglas de Productos

#### R-PRO-001: Catálogo Fijo
- 4 productos fijos: Café, Cacao, Maíz, Achiote
- No se pueden agregar ni eliminar productos
- Características codificadas en sistema

#### R-PRO-002: Precio Sugerido Editable
- Único campo editable por producto
- Se puede actualizar en cualquier momento
- No afecta compras ya registradas (no recalcula)

### 6.8 Reglas de Zonas

#### R-ZON-001: Nombre Único
- No pueden existir dos zonas con el mismo nombre
- Validación case-insensitive

#### R-ZON-002: Sin Eliminación
- Zonas no se pueden eliminar
- Solo crear y editar nombre

#### R-ZON-003: Zonas con Clientes
- Si zona tiene clientes asignados, se puede editar nombre
- Cambio de nombre se refleja automáticamente en todos los clientes

### 6.9 Reglas de Reportes

#### R-REP-001: Rango de Fechas Obligatorio
- Todos los reportes requieren rango de fechas
- Sin rango máximo de días

#### R-REP-002: Filtros Opcionales Acumulativos
- Si no se selecciona filtro opcional, incluye todos los registros
- Filtros se aplican con operador AND

#### R-REP-003: Formato de Exportación
- Excel simple: encabezados + bordes
- Nombre archivo: `reporte_[tipo]_YYYYMMDD_HHMMSS.xlsx`
- Formato de números: separador de miles

---

## 7. INTERFAZ DE USUARIO Y EXPERIENCIA

### 7.1 Arquitectura de Navegación

#### 7.1.1 Estructura Principal
**Layout:** Menú lateral fijo (sidebar) + Área de contenido

**Menú Lateral:**
- Logo/Nombre: Sistema Comercial Aliaga
- Dashboard
- Caja
- Compras
- Ventas
- Clientes
- Préstamos
- Productos
- Zonas
- Reportes
- Configuración (Backup)

### 7.2 Componentes Comunes

#### 7.2.1 Autocompletado de Clientes
- Input con búsqueda en tiempo real
- Búsqueda por: DNI (numérico) o Nombre (texto)
- Coincidencias parciales
- Mostrar primeros 5 resultados
- Formato: "Nombre Completo - DNI"
- Si DNI no existe: botón "Buscar en RENIEC"
- Si API falla o botón no usado: "Registrar Manualmente"

#### 7.2.2 Modales de Confirmación
**Estilo:** Centrado, overlay oscuro

**Estructura:**
- Título (según acción)
- Mensaje descriptivo
- Botones: Confirmar (primario) + Cancelar (secundario)

**Uso:**
- Cierre de caja con diferencia
- Edición de compra/venta
- Confirmación de operaciones exitosas

#### 7.2.3 Tablas y Listados
- Encabezados fijos
- Ordenamiento por defecto: más recientes primero
- Paginación: 50 registros por página
- Controles: << < [1] 2 3 ... N > >>
- Columna de acciones: iconos (ver, editar, eliminar si aplica)

#### 7.2.4 Formularios
**Estructura estándar:**
1. Título del formulario
2. Campos en orden lógico
3. Campos obligatorios marcados con *
4. Validación en tiempo real (para numéricos y formatos)
5. Botones al final: Cancelar (izq) + Guardar (der, primario)

**Manejo de errores:**
- Mensajes debajo del campo con error
- Color rojo para indicar campo inválido
- No permitir submit si hay errores

### 7.3 Paleta de Colores y Estilo

#### 7.3.1 Colores Funcionales
- **Primario:** Azul (#2563eb) - Botones principales, enlaces
- **Éxito:** Verde (#16a34a) - Confirmaciones, estados positivos
- **Advertencia:** Amarillo/Naranja (#f59e0b) - Alertas, préstamos pendientes
- **Error:** Rojo (#dc2626) - Errores, validaciones fallidas
- **Neutro:** Grises (#f3f4f6, #6b7280, #1f2937) - Backgrounds, texto, bordes

#### 7.3.2 Tipografía
- **Familia:** Sans-serif moderna (Inter, Roboto, o similar)
- **Tamaños:**
  - Títulos H1: 24px
  - Títulos H2: 20px
  - Títulos H3: 18px
  - Texto normal: 14px
  - Texto pequeño: 12px

### 7.4 Comportamientos de Interacción

#### 7.4.1 Feedback Visual
- **Hover:** Cambio sutil de color en botones/enlaces
- **Active:** Efecto de presión en botones
- **Loading:** Spinner o skeleton para cargas
- **Disabled:** Opacidad reducida + cursor not-allowed

#### 7.4.2 Validación de Inputs Numéricos
- Bloquear caracteres no válidos al escribir
- Permitir copiar/pegar (validar al pegar)
- Autoformatear mientras se escribe:
  - Agregar separador de miles
  - Limitar decimales según campo

#### 7.4.3 Flujo Post-Operación
- Operación exitosa → Modal de confirmación → Redirigir a lista del módulo
- Error → Mantener formulario con datos → Mostrar mensaje → Permitir reintentar

#### 7.4.4 Navegación
- Breadcrumbs en módulos con drill-down (ej: Clientes > Detalle Cliente > Historial)
- Botón "Volver" en vistas de detalle
- Confirmación al salir de formulario con cambios sin guardar

### 7.5 Responsive (Opcional)

**Prioridad:** Diseño para Desktop (laptop)
- Resolución mínima: 1366x768
- Orientación: Landscape
- No es crítico soporte mobile (uso exclusivo en laptop)

---

## 8. LIMITACIONES Y CONSIDERACIONES

### 8.1 Límites Técnicos

#### 8.1.1 Capacidad de Base de Datos
- SQLite sin límite práctico para el volumen esperado
- Estimación de crecimiento:
  - 300 clientes
  - ~100 operaciones/día
  - ~36,500 operaciones/año
  - Volumen manejable indefinidamente en SQLite

#### 8.1.2 Backup y Recuperación
- Backup: manual, a demanda
- Restauración: manual por personal técnico
- Sin backup automático programado
- Sin sincronización en nube

#### 8.1.3 Concurrencia
- Usuario único: sin problemas de concurrencia
- Sin control de transacciones complejas

### 8.2 Dependencias Externas

#### 8.2.1 API RENIEC
- Requerida solo para registro de clientes nuevos
- Fallback a registro manual
- No crítica para operación diaria

#### 8.2.2 Impresora Térmica
- Requerida para vouchers
- Compatible con impresoras estándar ESC/POS
- Driver del SO necesario

#### 8.2.3 Sistema Operativo
- Windows (laptop)
- Fecha/hora correcta del sistema (responsabilidad del usuario)

### 8.3 No Incluido en MVP

#### 8.3.1 Funcionalidades Futuras (Fuera de Alcance)
- Multiusuario / Roles de acceso
- Sistema de autenticación
- Sincronización en nube
- Backup automático programado
- Acceso desde dispositivos móviles
- Gestión de inventario físico
- Trazabilidad de lotes (ventas vinculadas a compras)
- Reportes con gráficos/dashboards visuales
- Notificaciones push
- Integración con sistemas contables externos
- Facturación electrónica (SUNAT)
- Gestión de proveedores (diferente a clientes proveedores)
- Control de gastos categorizados
- Proyecciones financieras
- Recordatorios de préstamos vencidos

#### 8.3.2 Optimizaciones No Prioritarias
- Búsqueda avanzada con múltiples criterios
- Exportación a otros formatos (PDF, CSV)
- Personalización de temas/colores por usuario
- Atajos de teclado avanzados
- Modo offline con sincronización posterior

### 8.4 Supuestos y Dependencias

#### 8.4.1 Supuestos del Usuario
- Usuario capacitado en el uso del sistema
- Usuario mantiene fecha/hora correcta del sistema
- Usuario realiza backups periódicos
- Usuario tiene conocimientos básicos de Windows
- Usuario tiene acceso a internet para consultas RENIEC

#### 8.4.2 Dependencias del Negocio
- Procesos de compra/venta no cambiarán significativamente
- 4 productos son suficientes (no hay planes de agregar más)
- Estructura de características por producto es estable
- Préstamos sin interés es política permanente
- No hay requerimientos legales adicionales (facturación electrónica, etc.)

---

## 9. CRITERIOS DE ACEPTACIÓN

### 9.1 Funcionalidad Básica
- [ ] Sistema inicia sin errores en Windows
- [ ] Base de datos SQLite se crea automáticamente en primera ejecución
- [ ] Datos del negocio (hardcoded) son correctos
- [ ] Cliente anónimo (DNI 00000000) existe por defecto

### 9.2 Módulo Caja
- [ ] Se puede abrir caja con monto inicial
- [ ] No permite abrir segunda caja del mismo día
- [ ] Cierra automáticamente caja anterior al abrir nueva
- [ ] Se puede cerrar caja con arqueo y registra diferencia
- [ ] Se puede reabrir caja del mismo día
- [ ] Registra inyecciones, retiros y gastos correctamente
- [ ] Validaciones de saldo funcionan (no retiro mayor a disponible)
- [ ] Historial de cajas muestra todas las cajas cerradas
- [ ] Ajustes posteriores solo disponibles en cajas cerradas
- [ ] Ajustes no afectan caja actual

### 9.3 Módulo Compras
- [ ] No permite registrar compra sin caja abierta
- [ ] Autocompletado de clientes funciona por DNI y nombre
- [ ] Consulta API RENIEC correctamente
- [ ] Permite registro manual si API falla
- [ ] Cliente con DNI existente carga datos automáticamente
- [ ] Banner de alerta muestra préstamo pendiente si existe
- [ ] Características varían según producto seleccionado
- [ ] Campos fijos deshabilitados para maíz/achiote
- [ ] Cálculos automáticos (peso neto, total) son correctos
- [ ] Resumen pre-confirmación muestra datos correctos
- [ ] Voucher se genera e imprime automáticamente
- [ ] Numeración de voucher es correlativa y única
- [ ] Edición solo disponible el mismo día
- [ ] Edición solo permite cambiar peso, descuento, precio
- [ ] Edición marca registro como editado
- [ ] Reimpresión mantiene número y marca "DUPLICADO"

### 9.4 Módulo Ventas
- [ ] No permite registrar venta sin caja abierta
- [ ] Permite ingresar todos los campos manualmente
- [ ] No hace cálculos automáticos
- [ ] No genera voucher
- [ ] Registra movimiento de caja correctamente
- [ ] Edición solo disponible el mismo día

### 9.5 Módulo Clientes
- [ ] Lista clientes proveedores correctamente
- [ ] Búsqueda en tiempo real funciona
- [ ] Filtro por zona funciona
- [ ] Detalle de cliente muestra información completa
- [ ] Historial de compras con filtros funciona
- [ ] Totales acumulados son correctos
- [ ] Historial de préstamos ordenado por reciente
- [ ] Edición de cliente actualiza datos
- [ ] No permite editar DNI ni saldo de préstamo
- [ ] Lista clientes compradores separadamente
- [ ] Cliente anónimo es inmutable

### 9.6 Módulo Préstamos
- [ ] Lista clientes con préstamos activos
- [ ] No permite registrar préstamo sin caja abierta
- [ ] Préstamo incrementa saldo del cliente
- [ ] Préstamo registra egreso en caja
- [ ] No permite registrar abono sin caja abierta
- [ ] Abono reduce saldo del cliente
- [ ] Abono registra ingreso en caja
- [ ] Valida que abono no supere saldo pendiente
- [ ] Historial muestra todos los movimientos ordenados

### 9.7 Módulo Productos
- [ ] Lista 4 productos fijos
- [ ] Permite editar solo precio sugerido
- [ ] Cambio de precio no afecta compras anteriores

### 9.8 Módulo Zonas
- [ ] Permite crear nueva zona
- [ ] Valida que nombre sea único
- [ ] Permite editar nombre inline
- [ ] No permite eliminar zonas
- [ ] Muestra cantidad de clientes por zona
- [ ] Drill-down a clientes de zona funciona

### 9.9 Módulo Reportes
- [ ] Todos los reportes requieren rango de fechas
- [ ] Filtros opcionales funcionan correctamente
- [ ] Vista previa en tabla muestra datos correctos
- [ ] Exportación a Excel funciona
- [ ] Excel tiene formato simple (encabezados + bordes)
- [ ] Nombre de archivo incluye timestamp
- [ ] Totales y subtotales son correctos
- [ ] Todos los 5 tipos de reportes funcionan

### 9.10 Funcionalidad Backup
- [ ] Botón de exportar backup funciona
- [ ] Archivo generado tiene timestamp en nombre
- [ ] Archivo es copia exacta de BD SQLite
- [ ] Diálogo de guardado se abre correctamente

### 9.11 Validaciones y Reglas de Negocio
- [ ] Todas las validaciones de caja funcionan
- [ ] Formatos numéricos son correctos (1 decimal pesos, 2 decimales montos)
- [ ] Separadores de miles se muestran correctamente
- [ ] Fechas y horas se registran correctamente
- [ ] Cierre automático a medianoche funciona
- [ ] Todas las reglas de negocio documentadas se cumplen

### 9.12 Interfaz de Usuario
- [ ] Menú lateral funciona correctamente
- [ ] Dashboard muestra información correcta
- [ ] Modales de confirmación funcionan
- [ ] Tablas paginan correctamente (50 registros)
- [ ] Ordenamiento por defecto (recientes primero) funciona
- [ ] Formularios validan en tiempo real
- [ ] Mensajes de error son claros
- [ ] Flujo post-operación (modal + redireccionamiento) funciona
- [ ] Autocompletados funcionan en tiempo real

---

## 10. PLAN DE IMPLEMENTACIÓN (SUGERIDO)

### Fase 1: Fundamentos (Semanas 1-2)
1. Setup del proyecto (.NET 10 + Angular)
2. Configuración de SQLite
3. Modelos de datos y migraciones
4. Estructura base de Angular (layout, routing, menú)
5. Cliente anónimo precargado

### Fase 2: Módulo Caja (Semanas 3-4)
1. Abrir/cerrar caja
2. Movimientos de caja (inyección, retiro, gasto)
3. Vista de caja actual
4. Historial de cajas
5. Cierre automático

### Fase 3: Módulos Maestros (Semanas 5-6)
1. Zonas (CRUD básico)
2. Productos (lista + editar precio)
3. Clientes proveedores (con API RENIEC)
4. Clientes compradores (simple)

### Fase 4: Operaciones Core (Semanas 7-9)
1. Compras (registro completo + voucher)
2. Ventas (registro + movimiento caja)
3. Edición de compras/ventas
4. Integración con caja

### Fase 5: Préstamos (Semana 10)
1. Registro de préstamos y abonos
2. Historial de movimientos
3. Integración con módulo clientes y caja

### Fase 6: Reportes (Semanas 11-12)
1. Implementar 5 tipos de reportes
2. Filtros dinámicos
3. Exportación a Excel

### Fase 7: Ajustes y Pulido (Semanas 13-14)
1. Ajustes posteriores en cajas históricas
2. Backup/exportación BD
3. Mejoras de UX
4. Optimización de rendimiento
5. Testing integral

### Fase 8: Testing y Capacitación (Semana 15)
1. Pruebas de aceptación con usuario final
2. Corrección de bugs
3. Capacitación del dueño
4. Documentación de usuario

---

## 11. NOTAS TÉCNICAS PARA DESARROLLO

### 11.1 Estructura de Carpetas Sugerida (Backend)

```
/ComercialAliaga.API
  /Controllers
    - CajaController.cs
    - ComprasController.cs
    - VentasController.cs
    - ClientesController.cs
    - PrestamosController.cs
    - ProductosController.cs
    - ZonasController.cs
    - ReportesController.cs
  /Models
    - Zona.cs
    - ClienteProveedor.cs
    - ClienteComprador.cs
    - Producto.cs
    - Caja.cs
    - Compra.cs
    - Venta.cs
    - Prestamo.cs
    - MovimientoCaja.cs
    - ConfiguracionNegocio.cs
  /DTOs
    - [DTOs para request/response]
  /Services
    - CajaService.cs
    - ComprasService.cs
    - VentasService.cs
    - PrestamosService.cs
    - ReportesService.cs
    - VoucherService.cs
    - ReniecService.cs
  /Data
    - AppDbContext.cs
    - Migrations/
  /Helpers
    - CalculosHelper.cs
    - FormateadorHelper.cs
```

### 11.2 Estructura de Carpetas Sugerida (Frontend)

```
/src
  /app
    /core
      /models
      /services
      /guards
    /shared
      /components
        - autocomplete-cliente/
        - modal-confirmacion/
        - tabla-paginada/
      /pipes
      /directives
    /modules
      /dashboard
      /caja
      /compras
      /ventas
      /clientes
      /prestamos
      /productos
      /zonas
      /reportes
      /configuracion
```

### 11.3 Librerías Recomendadas

**Backend:**
- Entity Framework Core (para SQLite)
- AutoMapper (mapeo DTO)
- FluentValidation (validaciones)
- EPPlus o ClosedXML (exportación Excel)

**Frontend:**
- Angular Material o PrimeNG (componentes UI)
- RxJS (manejo de streams)
- Date-fns o Moment.js (manejo de fechas)
- SheetJS (lectura Excel si se necesita en futuro)

**Impresión:**
- Librería de impresión ESC/POS para .NET (ej: ESCPOS-NET)

### 11.4 Consideraciones de Seguridad

- Validación de inputs en backend (nunca confiar solo en frontend)
- Sanitización de queries SQL (usar Entity Framework correctamente)
- Validación de tipos de archivo en backup
- Manejo seguro de errores (no exponer detalles técnicos al usuario)

### 11.5 Performance

- Índices en BD:
  - Caja.Fecha (único)
  - Compra.NumeroVoucher (único)
  - ClienteProveedor.DNI (único)
  - Prestamo.ClienteProveedorId + FechaMovimiento
- Paginación en todas las listas
- Lazy loading en historial de compras/préstamos

---

## 12. GLOSARIO

- **Acopio:** Proceso de compra de productos agrícolas (granos) directamente a productores locales
- **Anónimo:** Cliente especial con DNI 00000000 usado cuando no se captura datos del cliente real
- **Arqueo:** Conteo físico del dinero en caja
- **Ajuste Posterior:** Operación registrada en caja histórica para regularizar diferencias
- **Baldeo/Valdeo:** Método de pesaje local para granos mote-baba
- **Calidad/Rendimiento:** Clasificación del grano según su calidad
- **Cliente Comprador:** Entidad a quien el negocio vende granos procesados
- **Cliente Proveedor:** Agricultor/productor que vende granos al negocio
- **Descuento en Kg:** Cantidad de peso que se resta del peso bruto (merma, impurezas)
- **Mote-baba:** Grano de café o cacao recién cosechado, con alto contenido de humedad
- **Nivel de Secado:** Estado de humedad del grano (mote-baba, húmedo, estándar, seco)
- **Peso Neto:** Peso final después de aplicar descuentos
- **Tipo de Pesado:** Método usado para pesar (kilogramos o valdeo)
- **Voucher:** Comprobante de compra impreso (no es factura)
- **Zona:** Área geográfica de procedencia del cliente (comunidad, distrito)

---

## 13. CONTACTO Y APROBACIONES

**Documento Preparado Por:** [Nombre del Analista]  
**Fecha de Creación:** 18 de Diciembre, 2025  
**Versión:** 1.0

**Aprobaciones Requeridas:**
- [ ] Dueño del Negocio (Cliente Final)
- [ ] Product Owner / Líder del Proyecto
- [ ] Arquitecto de Software
- [ ] Líder de Desarrollo

**Historial de Cambios:**
| Versión | Fecha | Autor | Cambios |
|---------|-------|-------|---------|
| 1.0 | 18/12/2025 | [Nombre] | Documento inicial |

---

**FIN DEL DOCUMENTO PRD**
