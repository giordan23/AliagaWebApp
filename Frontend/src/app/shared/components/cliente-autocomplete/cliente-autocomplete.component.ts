import { Component, Input, Output, EventEmitter, OnInit, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, startWith, catchError } from 'rxjs/operators';
import { ClientesService } from '../../../core/services/clientes.service';
import { FormatoMonedaPipe } from '../../pipes/formato-moneda.pipe';

export interface ClienteAutocomplete {
  id: number;
  dni?: string;
  nombreCompleto?: string;
  nombre?: string; // Para compradores
  razonSocial?: string;
  saldoPrestamo?: number;
}

@Component({
  selector: 'app-cliente-autocomplete',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
    FormatoMonedaPipe
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ClienteAutocompleteComponent),
      multi: true
    }
  ],
  template: `
    <div class="autocomplete-container">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>{{ label }}</mat-label>
        <input
          type="text"
          matInput
          [formControl]="searchControl"
          [matAutocomplete]="auto"
          [placeholder]="placeholder">
        <mat-spinner *ngIf="loading" matSuffix diameter="20"></mat-spinner>
        <button
          *ngIf="showNewButton"
          mat-icon-button
          matSuffix
          (click)="onNuevoCliente()"
          type="button"
          matTooltip="Nuevo Cliente">
          <mat-icon>add_circle</mat-icon>
        </button>
        <mat-autocomplete
          #auto="matAutocomplete"
          [displayWith]="displayFn"
          (optionSelected)="onOptionSelected($event.option.value)">
          <mat-option *ngFor="let cliente of filteredClientes$ | async" [value]="cliente">
            <div class="option-content">
              <div class="option-main" *ngIf="tipoCliente === 'proveedor'">
                <strong>{{ cliente.dni }}</strong>
                <span class="separator">-</span>
                <span>{{ getNombre(cliente) }}</span>
              </div>
              <div class="option-main" *ngIf="tipoCliente === 'comprador'">
                <span>{{ getNombre(cliente) }}</span>
              </div>
              <div *ngIf="mostrarSaldo && cliente.saldoPrestamo && cliente.saldoPrestamo > 0" class="option-saldo">
                <small>Saldo: {{ cliente.saldoPrestamo | formatoMoneda }}</small>
              </div>
            </div>
          </mat-option>
          <mat-option *ngIf="(filteredClientes$ | async)?.length === 0" disabled>
            <em>No se encontraron clientes</em>
          </mat-option>
        </mat-autocomplete>
      </mat-form-field>
    </div>
  `,
  styles: [`
    .autocomplete-container {
      width: 100%;
    }

    .full-width {
      width: 100%;
    }

    .option-content {
      display: flex;
      flex-direction: column;
      padding: 4px 0;
    }

    .option-main {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .separator {
      color: #999;
    }

    .option-saldo {
      margin-top: 4px;
      color: #e74c3c;
      font-weight: 600;
    }

    :host ::ng-deep .autocomplete-container .mat-mdc-form-field-subscript-wrapper {
      display: none;
    }
  `]
})
export class ClienteAutocompleteComponent implements OnInit, ControlValueAccessor {
  @Input() label: string = 'Cliente';
  @Input() placeholder: string = 'Buscar por DNI o nombre...';
  @Input() tipoCliente: 'proveedor' | 'comprador' = 'proveedor';
  @Input() mostrarSaldo: boolean = true;
  @Input() showNewButton: boolean = true;
  @Output() nuevoCliente = new EventEmitter<void>();
  @Output() searchTextChange = new EventEmitter<string>();

  searchControl = new FormControl('');
  filteredClientes$: Observable<ClienteAutocomplete[]> = of([]);
  loading = false;

  // ControlValueAccessor
  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};
  disabled = false;

  constructor(private clientesService: ClientesService) {}

  ngOnInit(): void {
    this.filteredClientes$ = this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(value => {
        // Emitir el texto escrito (solo si es string)
        if (typeof value === 'string') {
          this.searchTextChange.emit(value);
        }

        // Si el valor es un objeto (cliente seleccionado), no buscar
        if (typeof value === 'object' && value !== null) {
          return of([]);
        }

        // Si es string y tiene al menos 1 caracter, buscar
        if (typeof value === 'string' && value.trim().length >= 1) {
          this.loading = true;
          return this.buscarClientes(value.trim());
        }

        this.loading = false;
        return of([]);
      }),
      catchError(() => {
        this.loading = false;
        return of([]);
      })
    );
  }

  private buscarClientes(termino: string): Observable<ClienteAutocomplete[]> {
    if (this.tipoCliente === 'proveedor') {
      // El backend ya filtra por DNI o nombre con el parámetro 'search'
      return this.clientesService.obtenerProveedores(0, 1000, termino).pipe(
        switchMap(response => {
          this.loading = false;
          let clientes = response.items || response.data || response || [];

          // Priorizar cliente anónimo al inicio de los resultados
          clientes = clientes.sort((a: any, b: any) => {
            const esAnonimoA = a.dni === '00000000' || a.esAnonimo;
            const esAnonimoB = b.dni === '00000000' || b.esAnonimo;

            if (esAnonimoA && !esAnonimoB) return -1;
            if (!esAnonimoA && esAnonimoB) return 1;
            return 0;
          });

          return of(clientes);
        }),
        catchError(() => {
          this.loading = false;
          return of([]);
        })
      );
    } else {
      // Para compradores, no hay parámetro de búsqueda en el backend, así que filtramos localmente
      return this.clientesService.obtenerCompradores().pipe(
        switchMap(response => {
          this.loading = false;
          const clientes = response || [];
          const terminoLower = termino.toLowerCase();

          // Filtrar por nombre (los compradores solo tienen nombre, no DNI ni RUC)
          const filtered = clientes.filter((c: any) =>
            c.nombre && c.nombre.toLowerCase().includes(terminoLower)
          );

          return of(filtered);
        }),
        catchError(() => {
          this.loading = false;
          return of([]);
        })
      );
    }
  }

  displayFn = (cliente: ClienteAutocomplete | null): string => {
    if (!cliente) return '';
    if (this.tipoCliente === 'proveedor') {
      return `${cliente.dni} - ${cliente.nombreCompleto}`;
    } else {
      // Compradores solo tienen nombre
      return cliente.nombre || '';
    }
  };

  getNombre(cliente: ClienteAutocomplete): string {
    if (this.tipoCliente === 'proveedor') {
      return cliente.nombreCompleto || '';
    } else {
      return cliente.nombre || '';
    }
  }

  onOptionSelected(cliente: ClienteAutocomplete): void {
    this.onChange(cliente.id);
    this.onTouched();
  }

  onNuevoCliente(): void {
    this.nuevoCliente.emit();
  }

  // ControlValueAccessor implementation
  writeValue(value: any): void {
    if (value === null) {
      this.searchControl.setValue('', { emitEvent: false });
    }
    // Si se recibe un ID, podríamos cargar el cliente, pero por simplicidad solo limpiamos
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (isDisabled) {
      this.searchControl.disable();
    } else {
      this.searchControl.enable();
    }
  }
}
