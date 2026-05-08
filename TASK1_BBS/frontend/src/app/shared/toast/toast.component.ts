import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, Toast } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html'
})
export class ToastComponent implements OnInit {
  toasts: Toast[] = [];
  icons: Record<string, string> = {
    success: '✅', danger: '❌', warning: '⚠️', info: 'ℹ️'
  };

  constructor(private toastSvc: ToastService) {}

  ngOnInit(): void {
    this.toastSvc.toasts$.subscribe(t => this.toasts = t);
  }

  dismiss(id: number): void { this.toastSvc.remove(id); }
}
