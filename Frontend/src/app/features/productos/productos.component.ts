import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ProductosService } from '../../core/services/productos.service';
import { AlertBannerComponent, AlertType } from '../../shared/components/alert-banner/alert-banner.component';
import { FormatoMonedaPipe } from '../../shared/pipes/formato-moneda.pipe';
import { FormatoFechaPipe } from '../../shared/pipes/formato-fecha.pipe';
import { CustomValidators } from '../../core/validators/custom-validators';

interface ProductoCaracteristicas {
  nivelesSecado: string[];
  calidades: string[];
  permiteValdeo: boolean;
}

@Component({
  selector: 'app-productos',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    AlertBannerComponent,
    FormatoMonedaPipe,
    FormatoFechaPipe
  ],
  templateUrl: './productos.component.html',
  styleUrls: ['./productos.component.css']
})
export class ProductosComponent implements OnInit {
  productos: any[] = [];
  loading = false;
  editandoId: number | null = null;
  formPrecio: FormGroup;

  // Para el alert banner
  showAlert = false;
  alertType: AlertType = 'info';
  alertMessage = '';

  constructor(
    private productosService: ProductosService,
    private fb: FormBuilder
  ) {
    this.formPrecio = this.fb.group({
      precio: [null, [Validators.required, Validators.min(0.01), CustomValidators.decimal(2)]]
    });
  }

  ngOnInit(): void {
    this.cargarProductos();
  }

  cargarProductos(): void {
    this.loading = true;
    this.productosService.obtenerTodos().subscribe({
      next: (productos) => {
        this.productos = productos.map((p: any) => ({
          ...p,
          caracteristicas: this.parseCaracteristicas(p)
        }));
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar productos:', error);
        this.mostrarAlerta('error', 'Error al cargar los productos');
        this.loading = false;
      }
    });
  }

  parseCaracteristicas(producto: any): ProductoCaracteristicas {
    // El backend ya devuelve los arrays parseados, no necesitamos JSON.parse
    return {
      nivelesSecado: producto.nivelesSecado || [],
      calidades: producto.calidades || [],
      permiteValdeo: producto.permiteValdeo || false
    };
  }

  iniciarEdicionPrecio(producto: any): void {
    this.editandoId = producto.id;
    this.formPrecio.patchValue({
      precio: producto.precioSugeridoPorKg
    });
  }

  cancelarEdicion(): void {
    this.editandoId = null;
    this.formPrecio.reset();
  }

  guardarPrecio(producto: any): void {
    if (this.formPrecio.invalid) {
      return;
    }

    const nuevoPrecio = this.formPrecio.get('precio')?.value;

    this.productosService.actualizarPrecio(producto.id, nuevoPrecio).subscribe({
      next: () => {
        producto.precioSugeridoPorKg = nuevoPrecio;
        this.editandoId = null;
        this.formPrecio.reset();
        this.mostrarAlerta('success', `Precio de ${producto.nombre} actualizado exitosamente`);
      },
      error: (error) => {
        console.error('Error al actualizar precio:', error);
        this.mostrarAlerta('error', 'Error al actualizar el precio');
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

  getErrorMessage(field: string): string {
    return CustomValidators.getErrorMessage(this.formPrecio.get(field));
  }
}
