import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { OrderStatus, DonutSlice } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-order-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-status.html',
  styleUrl: './order-status.css'
})
export class OrderStatusComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  orderStatus = signal<OrderStatus | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchOrderStatus();
  }

  fetchOrderStatus(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getOrderStatus().subscribe({
      next: (res: any) => {
        this.orderStatus.set(res);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load order status:', err);
        this.error.set('Failed to load order status data.');
        this.loading.set(false);
      }
    });
  }

  donutGradient(slices: DonutSlice[]): string {
    if (!slices || slices.length === 0) return '#E5E7EB';
    let accum = 0;
    const gradientParts = slices.map(slice => {
      const start = accum;
      accum += Number(slice.percentage);
      return `${slice.color} ${start}% ${accum}%`;
    });
    return `conic-gradient(${gradientParts.join(', ')})`;
  }
}
