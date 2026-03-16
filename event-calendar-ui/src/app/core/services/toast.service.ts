import { Injectable, signal } from '@angular/core';

export interface Toast {
    id: number;
    message: string;
    type: 'success' | 'error' | 'info' | 'warning';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
    private _toasts = signal<Toast[]>([]);
    readonly toasts = this._toasts.asReadonly();
    private nextId = 0;

    show(message: string, type: Toast['type'] = 'info', duration = 3500) {
        const id = ++this.nextId;
        this._toasts.update(t => [...t, { id, message, type }]);
        setTimeout(() => this.remove(id), duration);
    }

    success(message: string) { this.show(message, 'success'); }
    error(message: string) { this.show(message, 'error'); }
    info(message: string) { this.show(message, 'info'); }
    warning(message: string) { this.show(message, 'warning'); }

    remove(id: number) {
        this._toasts.update(t => t.filter(x => x.id !== id));
    }
}
