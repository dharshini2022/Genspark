import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { RecentOrder } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-recent-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './recent-orders.html',
  styleUrl: './recent-orders.css'
})
export class RecentOrdersComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  recentOrders = signal<RecentOrder[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchRecentOrders();
  }

  fetchRecentOrders(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getRecentActivity().subscribe({
      next: (res: any) => {
        this.recentOrders.set(res.recentOrders || []);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load recent activity:', err);
        this.error.set('Failed to load recent orders.');
        this.loading.set(false);
      }
    });
  }

  statusClass(status: string): string {
    const s = (status || '').toLowerCase();
    switch (s) {
      case 'delivered':
        return 'status-badge status-delivered';
      case 'shipped':
        return 'status-badge status-shipped';
      case 'confirmed':
        return 'status-badge status-confirmed';
      case 'cancelled':
        return 'status-badge status-cancelled';
      default:
        return 'status-badge status-confirmed';
    }
  }
}
