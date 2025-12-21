import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';

export type AlertType = 'success' | 'error' | 'warning' | 'info';

@Component({
  selector: 'app-alert-banner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      *ngIf="visible"
      [@slideIn]
      [class]="'alert alert-' + type"
      role="alert">
      <div class="alert-content">
        <span class="alert-icon">{{ getIcon() }}</span>
        <span class="alert-message">{{ message }}</span>
      </div>
      <button class="alert-close" (click)="close()" aria-label="Cerrar">&times;</button>
    </div>
  `,
  styles: [`
    .alert {
      position: fixed;
      top: 80px;
      right: 20px;
      z-index: 9999;
      min-width: 300px;
      max-width: 500px;
      padding: 16px 20px;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      display: flex;
      justify-content: space-between;
      align-items: center;
      animation: shake 0.5s;
    }

    .alert-content {
      display: flex;
      align-items: center;
      gap: 12px;
      flex: 1;
    }

    .alert-icon {
      font-size: 24px;
      font-weight: bold;
    }

    .alert-message {
      flex: 1;
      font-size: 14px;
      line-height: 1.5;
    }

    .alert-close {
      background: none;
      border: none;
      font-size: 28px;
      font-weight: bold;
      cursor: pointer;
      color: inherit;
      opacity: 0.7;
      padding: 0;
      margin-left: 12px;
      line-height: 1;
      transition: opacity 0.2s;
    }

    .alert-close:hover {
      opacity: 1;
    }

    /* Tipos de alerta */
    .alert-success {
      background-color: #d4edda;
      color: #155724;
      border-left: 4px solid #28a745;
    }

    .alert-error {
      background-color: #f8d7da;
      color: #721c24;
      border-left: 4px solid #dc3545;
    }

    .alert-warning {
      background-color: #fff3cd;
      color: #856404;
      border-left: 4px solid #ffc107;
    }

    .alert-info {
      background-color: #d1ecf1;
      color: #0c5460;
      border-left: 4px solid #17a2b8;
    }

    /* Animación de entrada */
    @keyframes shake {
      0%, 100% { transform: translateX(0); }
      10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
      20%, 40%, 60%, 80% { transform: translateX(5px); }
    }
  `],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
      ])
    ])
  ]
})
export class AlertBannerComponent implements OnInit, OnDestroy {
  @Input() type: AlertType = 'info';
  @Input() message: string = '';
  @Input() autoDismiss: boolean = true;
  @Input() dismissDelay: number = 5000; // 5 segundos por defecto

  visible: boolean = true;
  private timeoutId: any;

  ngOnInit(): void {
    if (this.autoDismiss) {
      this.timeoutId = setTimeout(() => {
        this.close();
      }, this.dismissDelay);
    }
  }

  ngOnDestroy(): void {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
  }

  close(): void {
    this.visible = false;
  }

  getIcon(): string {
    switch (this.type) {
      case 'success': return '✓';
      case 'error': return '✕';
      case 'warning': return '⚠';
      case 'info': return 'ℹ';
      default: return '';
    }
  }
}
