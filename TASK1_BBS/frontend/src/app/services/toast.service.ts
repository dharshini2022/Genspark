import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Toast {
  id: number;
  type: 'success' | 'danger' | 'warning' | 'info';
  title: string;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _toasts = new BehaviorSubject<Toast[]>([]);
  toasts$ = this._toasts.asObservable();
  private counter = 0;

  private add(type: Toast['type'], title: string, message?: string): void {
    const id = ++this.counter;
    const toast: Toast = { id, type, title, message };
    this._toasts.next([...this._toasts.value, toast]);
    setTimeout(() => this.remove(id), 4000);
  }

  success(title: string, message?: string): void { this.add('success', title, message); }
  error(title: string, message?: string): void   { this.add('danger', title, message); }
  warning(title: string, message?: string): void { this.add('warning', title, message); }
  info(title: string, message?: string): void    { this.add('info', title, message); }

  remove(id: number): void {
    this._toasts.next(this._toasts.value.filter(t => t.id !== id));
  }
}
