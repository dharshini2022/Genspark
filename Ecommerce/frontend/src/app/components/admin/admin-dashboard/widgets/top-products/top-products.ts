import { Component, input, effect, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { TopSellingProduct } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-top-products',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './top-products.html',
  styleUrl: './top-products.css'
})
export class TopProductsComponent {
  private dashboardService = inject(DashboardService);
  month = input.required<string>();

  topProducts = signal<TopSellingProduct[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  constructor() {
    effect(() => {
      const currentMonth = this.month();
      if (currentMonth) {
        this.fetchTopProducts(currentMonth);
      }
    });
  }

  fetchTopProducts(month: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getPerformanceMetrics(month).subscribe({
      next: (res: any) => {
        this.topProducts.set(res.topSellingProducts || []);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load top products:', err);
        this.error.set('Failed to load top selling products.');
        this.loading.set(false);
      }
    });
  }
}
