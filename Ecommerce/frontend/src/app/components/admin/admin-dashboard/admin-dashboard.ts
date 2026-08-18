import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../services/admin-dashboard.service';
import { KpiGridComponent } from './widgets/kpi-grid/kpi-grid';
import { RevenueBreakdownComponent } from './widgets/revenue-breakdown/revenue-breakdown';
import { OrderStatusComponent } from './widgets/order-status/order-status';
import { RecentOrdersComponent } from './widgets/recent-orders/recent-orders';
import { TopProductsComponent } from './widgets/top-products/top-products';
import { DiscountDistributionComponent } from './widgets/discount-distribution/discount-distribution';
import { RecentNotificationsComponent } from './widgets/recent-notifications/recent-notifications';
import { PerformanceMetricsComponent } from './widgets/performance-metrics/performance-metrics';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    KpiGridComponent,
    RevenueBreakdownComponent,
    OrderStatusComponent,
    RecentOrdersComponent,
    TopProductsComponent,
    DiscountDistributionComponent,
    RecentNotificationsComponent,
    PerformanceMetricsComponent
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css',
})
export class AdminDashboard {
  private dashboardService = inject(DashboardService);
  selectedMonth = signal<string>('2026-06');

  constructor() {
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = String(now.getMonth() + 1).padStart(2, '0');
    this.selectedMonth.set(`${currentYear}-${currentMonth}`);
  }

  onMonthChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target && target.value) {
      this.selectedMonth.set(target.value);
    }
  }

  exportReport(): void {
    const monthStr = this.selectedMonth();
    this.dashboardService.exportReport(monthStr).subscribe({
      next: (blob: any) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `dashboard_report_${monthStr}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err: any) => {
        console.error('Failed to export report:', err);
        alert('Failed to export report.');
      }
    });
  }
}
