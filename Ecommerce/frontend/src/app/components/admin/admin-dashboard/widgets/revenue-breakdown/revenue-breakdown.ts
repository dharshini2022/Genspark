import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { RevenuePoint } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-revenue-breakdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './revenue-breakdown.html',
  styleUrl: './revenue-breakdown.css'
})
export class RevenueBreakdownComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  revenueBreakdown = signal<{ monthly: RevenuePoint[] } | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  revenueChartData = computed(() => {
    const currentData = this.revenueBreakdown();
    if (!currentData) return [];
    const rawPoints = currentData.monthly;

    if (!rawPoints) return [];
    return rawPoints.map(p => ({
      label: p.label,
      value: p.revenue
    }));
  });

  ngOnInit(): void {
    this.fetchRevenueBreakdown();
  }

  fetchRevenueBreakdown(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getRevenueBreakdown().subscribe({
      next: (res: any) => {
        this.revenueBreakdown.set(res);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load revenue breakdown:', err);
        this.error.set('Failed to load revenue data.');
        this.loading.set(false);
      }
    });
  }

  barHeightPercent(value: number): number {
    const chartData = this.revenueChartData();
    if (!chartData || chartData.length === 0) return 0;
    const maxVal = Math.max(...chartData.map(d => d.value), 0);
    if (maxVal === 0) return 0;
    const ratio = (value / maxVal) * 100;
    return ratio > 0 ? Math.max(ratio, 5) : 0;
  }
}
