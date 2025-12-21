import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'info' | 'warning' | 'danger'; // Para colorear el botón
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>
      <mat-icon [class]="'icon-' + data.type">
        {{ getIcon() }}
      </mat-icon>
      {{ data.title }}
    </h2>

    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">
        {{ data.cancelText || 'Cancelar' }}
      </button>
      <button
        mat-raised-button
        [color]="getButtonColor()"
        (click)="onConfirm()">
        {{ data.confirmText || 'Confirmar' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 {
      display: flex;
      align-items: center;
      gap: 10px;
      color: #2c3e50;
    }

    mat-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    .icon-info { color: #3498db; }
    .icon-warning { color: #f39c12; }
    .icon-danger { color: #e74c3c; }

    mat-dialog-content {
      padding: 20px 0;
      min-width: 300px;
    }

    mat-dialog-content p {
      margin: 0;
      line-height: 1.6;
      color: #34495e;
    }

    mat-dialog-actions {
      padding: 16px 0 0 0;
      margin: 0;
    }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {
    // Establecer tipo por defecto
    if (!this.data.type) {
      this.data.type = 'info';
    }
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  getIcon(): string {
    switch (this.data.type) {
      case 'warning': return 'warning';
      case 'danger': return 'error';
      default: return 'help_outline';
    }
  }

  getButtonColor(): 'primary' | 'accent' | 'warn' {
    if (this.data.type === 'danger') return 'warn';
    return 'primary';
  }
}
