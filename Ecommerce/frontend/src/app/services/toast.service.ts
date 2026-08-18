import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'info' | 'warning' | 'error';
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  toasts = signal<Toast[]>([]);
  private nextId = 0;

  show(message: string, type: 'success' | 'info' | 'warning' | 'error' = 'success', duration = 3000) {
    const id = this.nextId++;
    const toast: Toast = { id, message, type, duration };
    
    this.toasts.update(current => [...current, toast]);

    setTimeout(() => {
      this.dismiss(id);
    }, duration);
  }

  success(message: string, duration?: number) {
    this.show(message, 'success', duration);
  }

  info(message: string, duration?: number) {
    this.show(message, 'info', duration);
  }

  warning(message: string, duration?: number) {
    this.show(message, 'warning', duration);
  }

  error(message: string, duration?: number) {
    this.show(message, 'error', duration);
  }

  dismiss(id: number) {
    this.toasts.update(current => current.filter(t => t.id !== id));
  }
}
