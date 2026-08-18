import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { DiscountDistribution, DonutSlice } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-discount-distribution',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './discount-distribution.html',
  styleUrl: './discount-distribution.css'
})
export class DiscountDistributionComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  discountDistribution = signal<DiscountDistribution | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchDiscountDistribution();
  }

  fetchDiscountDistribution(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getDiscountDistribution().subscribe({
      next: (res: any) => {
        this.discountDistribution.set(res);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load discount distribution:', err);
        this.error.set('Failed to load discount data.');
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
