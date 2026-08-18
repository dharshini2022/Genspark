import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.html',
  styleUrl: './toast.css',
})
export class Toast {
  toasts = signal<any[]>([]);

  constructor(private toastService: ToastService) {
    this.toasts = this.toastService.toasts;
  }

  dismissToast(id: number) {
    this.toastService.dismiss(id);
  }
}
