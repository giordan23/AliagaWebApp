import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ClientesService } from '../../core/services/clientes.service';
import { ZonasService } from '../../core/services/zonas.service';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { ZonaAutocompleteComponent } from '../../shared/components/zona-autocomplete/zona-autocomplete.component';

interface ClienteProveedor {
  id: number;
  dni: string;
  nombreCompleto: string;
  telefono?: string;
  direccion?: string;
  fechaNacimiento?: Date;
  zonaId?: number;
  zonaNombre?: string;
  zona?: number | string; // Para autocomplete: ID o nombre de zona
  saldoPrestamo: number;
  totalKgVendidos: number;
  esAnonimo: boolean;
  fechaCreacion: Date;
}

interface Zona {
  id: number;
  nombre: string;
}

@Component({
  selector: 'app-clientes',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, FormatoMonedaPipe, ZonaAutocompleteComponent],
  templateUrl: './clientes.component.html',
  styleUrls: ['./clientes.component.css']
})
export class ClientesComponent implements OnInit {
  clientes: ClienteProveedor[] = [];
  zonas: Zona[] = [];

  // Filtros
  searchTerm: string = '';
  selectedZonaId: number | null = null;

  // Loading
  loading: boolean = false;

  // Modal de edición
  showEditModal: boolean = false;
  clienteEditando: ClienteProveedor | null = null;

  // Modal de nuevo cliente
  showNuevoModal: boolean = false;
  nuevoCliente: any = {
    dni: '',
    nombreCompleto: '',
    telefono: '',
    direccion: '',
    fechaNacimiento: null,
    zonaId: null,
    zonaNombre: null
  };
  consultandoReniec: boolean = false;
  reniecExitoso: boolean = false;
  nombreEditable: boolean = true;

  constructor(
    private clientesService: ClientesService,
    private zonasService: ZonasService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarZonas();
    this.cargarClientes();
  }

  @HostListener('document:keydown', ['$event'])
  handleEscapeKey(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      if (this.showEditModal) {
        this.cerrarModal();
      } else if (this.showNuevoModal) {
        this.cerrarModalNuevo();
      }
    }
  }

  cargarZonas(): void {
    this.zonasService.obtenerTodas().subscribe({
      next: (response: any) => {
        this.zonas = response.items || response;
      },
      error: (error: any) => {
        console.error('Error al cargar zonas:', error);
      }
    });
  }

  cargarClientes(): void {
    this.loading = true;
    const zonaId = this.selectedZonaId || undefined;
    const search = this.searchTerm.trim() || undefined;

    this.clientesService.obtenerProveedores(0, 1000, search, zonaId).subscribe({
      next: (response: any) => {
        this.clientes = response.items || [];
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar clientes:', error);
        this.loading = false;
      }
    });
  }

  buscar(): void {
    this.cargarClientes();
  }

  filtrarPorZona(): void {
    this.cargarClientes();
  }

  limpiarFiltros(): void {
    this.searchTerm = '';
    this.selectedZonaId = null;
    this.cargarClientes();
  }

  editarCliente(cliente: ClienteProveedor): void {
    if (cliente.esAnonimo) {
      alert('El cliente anónimo no se puede editar');
      return;
    }
    this.clienteEditando = { ...cliente };
    this.showEditModal = true;
  }

  cerrarModal(): void {
    this.showEditModal = false;
    this.clienteEditando = null;
  }

  guardarCliente(): void {
    if (!this.clienteEditando) return;

    // Determinar si es zonaId (number) o zonaNombre (string)
    let zonaId = null;
    let zonaNombre = null;

    if ((this.clienteEditando as any).zona) {
      if (typeof (this.clienteEditando as any).zona === 'number') {
        zonaId = (this.clienteEditando as any).zona;
      } else if (typeof (this.clienteEditando as any).zona === 'string') {
        zonaNombre = (this.clienteEditando as any).zona;
      }
    } else if (this.clienteEditando.zonaId) {
      // Si no hay campo zona pero sí zonaId (para compatibilidad)
      zonaId = this.clienteEditando.zonaId;
    }

    const request = {
      nombreCompleto: this.clienteEditando.nombreCompleto,
      telefono: this.clienteEditando.telefono || null,
      direccion: this.clienteEditando.direccion || null,
      fechaNacimiento: this.clienteEditando.fechaNacimiento || null,
      zonaId: zonaId,
      zonaNombre: zonaNombre
    };

    this.clientesService.actualizarProveedor(this.clienteEditando.id, request).subscribe({
      next: () => {
        alert('Cliente actualizado correctamente');
        this.cerrarModal();
        this.cargarClientes();
      },
      error: (error) => {
        console.error('Error al actualizar cliente:', error);
        alert('Error al actualizar cliente: ' + (error.error?.message || error.message));
      }
    });
  }

  eliminarCliente(cliente: ClienteProveedor): void {
    if (cliente.esAnonimo) {
      alert('El cliente anónimo no se puede eliminar');
      return;
    }

    if (!confirm(`¿Está seguro de eliminar al cliente "${cliente.nombreCompleto}"?`)) {
      return;
    }

    this.clientesService.eliminarProveedor(cliente.id).subscribe({
      next: () => {
        alert('Cliente eliminado exitosamente');
        this.cargarClientes();
      },
      error: (error) => {
        console.error('Error al eliminar cliente:', error);
        alert('Error al eliminar cliente: ' + (error.error?.message || error.message));
      }
    });
  }

  verPrestamos(cliente: ClienteProveedor): void {
    if (cliente.saldoPrestamo > 0) {
      // Navegar al módulo de préstamos con el ID del cliente
      this.router.navigate(['/prestamos'], { queryParams: { clienteId: cliente.id } });
    }
  }

  abrirModalNuevoCliente(): void {
    this.nuevoCliente = {
      dni: '',
      nombreCompleto: '',
      telefono: '',
      direccion: '',
      fechaNacimiento: null,
      zonaId: null,
      zonaNombre: null
    };
    this.consultandoReniec = false;
    this.reniecExitoso = false;
    this.nombreEditable = true;
    this.showNuevoModal = true;
  }

  cerrarModalNuevo(): void {
    this.showNuevoModal = false;
    this.nuevoCliente = {
      dni: '',
      nombreCompleto: '',
      telefono: '',
      direccion: '',
      fechaNacimiento: null,
      zonaId: null,
      zonaNombre: null
    };
    this.consultandoReniec = false;
    this.reniecExitoso = false;
    this.nombreEditable = true;
  }

  consultarReniec(): void {
    if (!this.nuevoCliente.dni || this.nuevoCliente.dni.length !== 8) {
      alert('Debe ingresar un DNI válido de 8 dígitos');
      return;
    }

    this.consultandoReniec = true;

    this.clientesService.consultarReniec(this.nuevoCliente.dni).subscribe({
      next: (response: any) => {
        this.consultandoReniec = false;

        if (response.success && response.nombreCompleto) {
          // RENIEC exitoso - nombre no editable
          this.reniecExitoso = true;
          this.nombreEditable = false;
          this.nuevoCliente.nombreCompleto = response.nombreCompleto;
          alert('Datos obtenidos de RENIEC correctamente');
        } else {
          // RENIEC falló - permitir ingreso manual
          this.reniecExitoso = false;
          this.nombreEditable = true;
          alert(response.message || 'No se pudo obtener datos de RENIEC. Puede ingresar el nombre manualmente.');
        }
      },
      error: (error: any) => {
        this.consultandoReniec = false;
        this.reniecExitoso = false;
        this.nombreEditable = true;
        console.error('Error al consultar RENIEC:', error);
        alert('Error al consultar RENIEC. Puede ingresar el nombre manualmente.');
      }
    });
  }

  crearCliente(): void {
    // Validaciones
    if (!this.nuevoCliente.dni || this.nuevoCliente.dni.length !== 8) {
      alert('Debe ingresar un DNI válido de 8 dígitos');
      return;
    }

    if (!this.nuevoCliente.nombreCompleto || this.nuevoCliente.nombreCompleto.trim() === '') {
      alert('Debe ingresar el nombre completo del cliente');
      return;
    }

    // Determinar si es zonaId (number) o zonaNombre (string)
    let zonaId = null;
    let zonaNombre = null;

    if (this.nuevoCliente.zona) {
      if (typeof this.nuevoCliente.zona === 'number') {
        zonaId = this.nuevoCliente.zona;
      } else if (typeof this.nuevoCliente.zona === 'string') {
        zonaNombre = this.nuevoCliente.zona;
      }
    }

    const request = {
      dni: this.nuevoCliente.dni,
      nombreCompleto: this.nuevoCliente.nombreCompleto,
      telefono: this.nuevoCliente.telefono || null,
      direccion: this.nuevoCliente.direccion || null,
      fechaNacimiento: this.nuevoCliente.fechaNacimiento || null,
      zonaId: zonaId,
      zonaNombre: zonaNombre
    };

    this.clientesService.crearProveedor(request).subscribe({
      next: () => {
        alert('Cliente creado correctamente');
        this.cerrarModalNuevo();
        this.cargarClientes();
      },
      error: (error: any) => {
        console.error('Error al crear cliente:', error);
        alert('Error al crear cliente: ' + (error.error?.message || error.message));
      }
    });
  }
}
