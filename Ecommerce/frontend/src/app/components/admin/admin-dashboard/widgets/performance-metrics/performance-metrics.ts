import { Component, input, effect, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { CategoryPerformance, VendorPerformance } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-performance-metrics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './performance-metrics.html',
  styleUrl: './performance-metrics.css'
})
export class PerformanceMetricsComponent {
  private dashboardService = inject(DashboardService);
  month = input.required<string>();

  categoryPerformance = signal<CategoryPerformance[]>([]);
  vendorPerformance = signal<VendorPerformance[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  constructor() {
    effect(() => {
      const currentMonth = this.month();
      if (currentMonth) {
        this.fetchPerformance(currentMonth);
      }
    });
  }

  fetchPerformance(month: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getPerformanceMetrics(month).subscribe({
      next: (res: any) => {
        this.categoryPerformance.set(res.categoryPerformance || []);
        this.vendorPerformance.set(res.vendorPerformance || []);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load performance metrics:', err);
        this.error.set('Failed to load performance metrics.');
        this.loading.set(false);
      }
    });
  }
}
