import { Component, input, effect, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { DashboardStats } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-kpi-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './kpi-grid.html',
  styleUrl: './kpi-grid.css'
})
export class KpiGridComponent {
  private dashboardService = inject(DashboardService);
  month = input.required<string>();

  kpis = signal<DashboardStats | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  constructor() {
    effect(() => {
      const currentMonth = this.month();
      if (currentMonth) {
        this.fetchKpis(currentMonth);
      }
    });
  }

  fetchKpis(month: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getKpis(month).subscribe({
      next: (res: any) => {
        this.kpis.set(res);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load KPIs:', err);
        this.error.set(err.error?.message || err.error || 'Failed to load dashboard statistics.');
        this.loading.set(false);
      }
    });
  }
}
