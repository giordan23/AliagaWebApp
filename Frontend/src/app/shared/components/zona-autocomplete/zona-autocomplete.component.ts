import { Component, Input, OnInit, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, startWith, catchError, map } from 'rxjs/operators';
import { ZonasService } from '../../../core/services/zonas.service';

export interface ZonaAutocomplete {
  id: number;
  nombre: string;
}

@Component({
  selector: 'app-zona-autocomplete',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ZonaAutocompleteComponent),
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
        <mat-icon *ngIf="!loading && searchControl.value && isNewZona" matSuffix class="new-icon" matTooltip="Nueva zona">add_circle</mat-icon>
        <mat-autocomplete
          #auto="matAutocomplete"
          [displayWith]="displayFn"
          (optionSelected)="onOptionSelected($event.option.value)">
          <mat-option *ngFor="let zona of filteredZonas$ | async" [value]="zona">
            <div class="option-content">
              <mat-icon class="zona-icon">place</mat-icon>
              <span>{{ zona.nombre }}</span>
            </div>
          </mat-option>
          <mat-option *ngIf="showCreateOption && isNewZona" [value]="{ id: 0, nombre: searchControl.value }" class="create-option">
            <div class="option-content new-zona">
              <mat-icon class="add-icon">add_circle</mat-icon>
              <span>Crear zona: <strong>{{ searchControl.value }}</strong></span>
            </div>
          </mat-option>
          <mat-option *ngIf="(filteredZonas$ | async)?.length === 0 && !isNewZona" disabled>
            <em>No se encontraron zonas</em>
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
      align-items: center;
      gap: 8px;
      padding: 4px 0;
    }

    .zona-icon {
      color: #4299e1;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .new-icon {
      color: #48bb78;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .create-option {
      background-color: #f0fdf4;
      border-top: 1px solid #86efac;
    }

    .new-zona {
      color: #22543d;
    }

    .add-icon {
      color: #48bb78;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    :host ::ng-deep .autocomplete-container .mat-mdc-form-field-subscript-wrapper {
      display: none;
    }
  `]
})
export class ZonaAutocompleteComponent implements OnInit, ControlValueAccessor {
  @Input() label: string = 'Zona';
  @Input() placeholder: string = 'Buscar o crear zona...';
  @Input() showCreateOption: boolean = true;

  searchControl = new FormControl('');
  filteredZonas$: Observable<ZonaAutocomplete[]> = of([]);
  loading = false;
  isNewZona = false;

  // ControlValueAccessor
  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};
  disabled = false;

  constructor(private zonasService: ZonasService) {}

  ngOnInit(): void {
    this.filteredZonas$ = this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(value => {
        // Si el valor es un objeto (zona seleccionada), no buscar
        if (typeof value === 'object' && value !== null) {
          this.isNewZona = false;
          return of([]);
        }

        // Si es string y tiene al menos 1 caracter, buscar
        if (typeof value === 'string' && value.trim().length >= 1) {
          this.loading = true;
          return this.buscarZonas(value.trim());
        }

        this.loading = false;
        this.isNewZona = false;
        return of([]);
      }),
      catchError(() => {
        this.loading = false;
        this.isNewZona = false;
        return of([]);
      })
    );
  }

  private buscarZonas(termino: string): Observable<ZonaAutocomplete[]> {
    return this.zonasService.obtenerTodas().pipe(
      map(response => {
        this.loading = false;
        const zonas = response.items || response.data || response || [];
        const terminoUpper = termino.toUpperCase();

        // Filtrar zonas que coincidan con el término de búsqueda
        const filtered = zonas.filter((z: any) =>
          z.nombre && z.nombre.toUpperCase().includes(terminoUpper)
        );

        // Verificar si el texto ingresado coincide exactamente con alguna zona existente
        const existeExacto = filtered.some((z: any) =>
          z.nombre.toUpperCase() === terminoUpper
        );

        // Si no existe exactamente, marcar como nueva zona
        this.isNewZona = !existeExacto && this.showCreateOption;

        return filtered;
      }),
      catchError(() => {
        this.loading = false;
        this.isNewZona = false;
        return of([]);
      })
    );
  }

  displayFn = (zona: ZonaAutocomplete | null): string => {
    if (!zona) return '';
    return zona.nombre || '';
  };

  onOptionSelected(zona: ZonaAutocomplete): void {
    if (zona.id === 0) {
      // Es una nueva zona, pasar el nombre para que se cree
      this.onChange(zona.nombre);
    } else {
      // Es una zona existente, pasar el ID
      this.onChange(zona.id);
    }
    this.onTouched();
  }

  // ControlValueAccessor implementation
  writeValue(value: any): void {
    if (value === null || value === undefined) {
      this.searchControl.setValue('', { emitEvent: false });
    }
    // Si se recibe un ID o nombre, podríamos cargar la zona, pero por simplicidad solo limpiamos
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
