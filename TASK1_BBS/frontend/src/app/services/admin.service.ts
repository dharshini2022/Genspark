import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  OperatorProfile, BusPendingDto, RouteDto, CreateRouteDto,
  RevenueDto, PlatformSettingDto, BookingList, MessageResponse
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private BASE = '/api/admin';
  constructor(private http: HttpClient) {}

  getOperators(status?: string): Observable<OperatorProfile[]> {
    const params = status ? { status } : {};
    return this.http.get<OperatorProfile[]>(`${this.BASE}/operators`, { params });
  }
  approveOperator(id: number): Observable<MessageResponse>  { return this.http.put<MessageResponse>(`${this.BASE}/operators/${id}/approve`, {}); }
  disableOperator(id: number): Observable<MessageResponse>  { return this.http.put<MessageResponse>(`${this.BASE}/operators/${id}/disable`, {}); }

  getBuses(status?: string): Observable<BusPendingDto[]> {
    const params = status ? { status } : {};
    return this.http.get<BusPendingDto[]>(`${this.BASE}/buses`, { params });
  }
  approveBus(id: number): Observable<MessageResponse>  { return this.http.put<MessageResponse>(`${this.BASE}/buses/${id}/approve`, {}); }
  disableBus(id: number): Observable<MessageResponse>  { return this.http.put<MessageResponse>(`${this.BASE}/buses/${id}/disable`, {}); }

  getRoutes(): Observable<RouteDto[]>                        { return this.http.get<RouteDto[]>(`${this.BASE}/routes`); }
  createRoute(dto: CreateRouteDto): Observable<RouteDto>     { return this.http.post<RouteDto>(`${this.BASE}/routes`, dto); }
  toggleRoute(id: number): Observable<MessageResponse>       { return this.http.put<MessageResponse>(`${this.BASE}/routes/${id}/toggle`, {}); }

  getRevenue(): Observable<RevenueDto>                       { return this.http.get<RevenueDto>(`${this.BASE}/revenue`); }

  getSettings(): Observable<PlatformSettingDto[]>            { return this.http.get<PlatformSettingDto[]>(`${this.BASE}/settings`); }
  updateSetting(key: string, value: string): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.BASE}/settings/${key}`, JSON.stringify(value), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
