import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../core/services/toast.service';

@Component({
    selector: 'app-toast',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="toast-container">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast toast--{{ toast.type }}" (click)="toastService.remove(toast.id)">
          <span class="toast__icon">{{ icons[toast.type] }}</span>
          <span class="toast__message">{{ toast.message }}</span>
        </div>
      }
    </div>
  `,
    styles: [`
    .toast-container { position: fixed; top: 1.25rem; right: 1.25rem; z-index: 9999; display: flex; flex-direction: column; gap: 0.5rem; max-width: 360px; }
    .toast { display: flex; align-items: center; gap: 0.75rem; padding: 0.875rem 1.25rem; border-radius: 8px; cursor: pointer; font-size: 0.9rem; font-weight: 500; box-shadow: 0 4px 12px rgba(0,0,0,0.15); animation: slideIn 0.3s ease; color: #fff; }
    .toast--success { background: var(--success); }
    .toast--error   { background: var(--danger); }
    .toast--warning { background: var(--warning); }
    .toast--info    { background: var(--info); }
    .toast__icon { font-size: 1.1rem; }
    @keyframes slideIn { from { transform: translateX(100%); opacity: 0; } to { transform: translateX(0); opacity: 1; } }
  `]
})
export class ToastComponent {
    toastService = inject(ToastService);
    icons: Record<string, string> = { success: '✓', error: '✕', warning: '⚠', info: 'ℹ' };
}
