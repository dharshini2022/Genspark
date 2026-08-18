import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardStats, RevenuePoint, OrderStatus, PerformanceMetrics, RecentActivity, DiscountDistribution } from '../models/dashboard.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private baseUrl = `${environment.baseUrl}/AdminDashboard`;

  constructor(private http: HttpClient) {}

  getKpis(month: string): Observable<DashboardStats> {
    const params = new HttpParams().set('month', month);
    return this.http.get<DashboardStats>(`${this.baseUrl}/kpis`, { params });
  }

  getRevenueBreakdown(): Observable<{ monthly: RevenuePoint[] }> {
    return this.http.get<{ monthly: RevenuePoint[] }>(`${this.baseUrl}/revenue-breakdown`);
  }

  getOrderStatus(): Observable<OrderStatus> {
    return this.http.get<OrderStatus>(`${this.baseUrl}/order-status`);
  }

  getPerformanceMetrics(month: string): Observable<PerformanceMetrics> {
    const params = new HttpParams().set('month', month);
    return this.http.get<PerformanceMetrics>(`${this.baseUrl}/performance`, { params });
  }

  getRecentActivity(): Observable<RecentActivity> {
    return this.http.get<RecentActivity>(`${this.baseUrl}/activity`);
  }

  getDiscountDistribution(): Observable<DiscountDistribution> {
    return this.http.get<DiscountDistribution>(`${this.baseUrl}/discount-distribution`);
  }

  exportReport(month: string): Observable<Blob> {
    const params = new HttpParams().set('month', month);
    return this.http.get(`${this.baseUrl}/export`, {
      params,
      responseType: 'blob'
    });
  }

  markAllNotificationsRead(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/notifications/mark-all-read`, {});
  }
}