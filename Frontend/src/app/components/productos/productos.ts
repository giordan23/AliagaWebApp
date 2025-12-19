import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductoService } from '../../services/producto';
import { Producto } from '../../models/producto.model';

@Component({
  selector: 'app-productos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './productos.html',
  styleUrl: './productos.css'
})
export class ProductosComponent implements OnInit {
  productos: Producto[] = [];
  nuevoProducto: Producto = {
    id: 0,
    nombre: '',
    precio: 0,
    stock: 0
  };
  editando: boolean = false;

  constructor(private productoService: ProductoService) { }

  ngOnInit(): void {
    this.cargarProductos();
  }

  cargarProductos(): void {
    this.productoService.getProductos().subscribe({
      next: (data) => {
        this.productos = data;
      },
      error: (error) => {
        console.error('Error al cargar productos:', error);
      }
    });
  }

  guardarProducto(): void {
    if (this.editando) {
      this.productoService.updateProducto(this.nuevoProducto.id, this.nuevoProducto).subscribe({
        next: () => {
          this.cargarProductos();
          this.limpiarFormulario();
        },
        error: (error) => console.error('Error al actualizar:', error)
      });
    } else {
      this.productoService.createProducto(this.nuevoProducto).subscribe({
        next: () => {
          this.cargarProductos();
          this.limpiarFormulario();
        },
        error: (error) => console.error('Error al crear:', error)
      });
    }
  }

  editarProducto(producto: Producto): void {
    this.nuevoProducto = { ...producto };
    this.editando = true;
  }

  eliminarProducto(id: number): void {
    if (confirm('¿Estás seguro de eliminar este producto?')) {
      this.productoService.deleteProducto(id).subscribe({
        next: () => {
          this.cargarProductos();
        },
        error: (error) => console.error('Error al eliminar:', error)
      });
    }
  }

  limpiarFormulario(): void {
    this.nuevoProducto = {
      id: 0,
      nombre: '',
      precio: 0,
            stock: 0
    };
    this.editando = false;
  }
}