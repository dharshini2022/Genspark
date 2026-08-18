import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { RecentNotification } from '../../../../../models/dashboard.model';

@Component({
  selector: 'app-recent-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './recent-notifications.html',
  styleUrl: './recent-notifications.css'
})
export class RecentNotificationsComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  recentNotifications = signal<RecentNotification[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchNotifications();
  }

  fetchNotifications(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getRecentActivity().subscribe({
      next: (res: any) => {
        this.recentNotifications.set(res.recentNotifications || []);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load notifications:', err);
        this.error.set('Failed to load notifications.');
        this.loading.set(false);
      }
    });
  }

  notificationDotClass(type: string): string {
    const t = (type || '').toLowerCase();
    switch (t) {
      case 'info':
        return 'dot dot-info';
      case 'warning':
        return 'dot dot-warning';
      case 'success':
        return 'dot dot-success';
      case 'error':
        return 'dot dot-error';
      default:
        return 'dot dot-info';
    }
  }

  markAllNotificationsRead(): void {
    this.dashboardService.markAllNotificationsRead().subscribe({
      next: () => {
        this.fetchNotifications();
      },
      error: (err: any) => {
        console.error('Failed to mark notifications read:', err);
      }
    });
  }
}
