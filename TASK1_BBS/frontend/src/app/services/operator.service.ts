import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  BusDto, AddBusDto, LayoutDto, UploadLayoutDto, ScheduleDto,
  CreateScheduleDto, UpdatePriceDto, BookingList, OperatorProfile, MessageResponse, RouteDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class OperatorService {
  private BASE = '/api/operator';
  constructor(private http: HttpClient) {}

  getProfile(): Observable<OperatorProfile>        { return this.http.get<OperatorProfile>(`${this.BASE}/profile`); }
  getBuses(): Observable<BusDto[]>                 { return this.http.get<BusDto[]>(`${this.BASE}/buses`); }
  addBus(dto: AddBusDto): Observable<BusDto>       { return this.http.post<BusDto>(`${this.BASE}/buses`, dto); }
  bringDownBus(id: number): Observable<MessageResponse> { return this.http.put<MessageResponse>(`${this.BASE}/buses/${id}/down`, {}); }

  getLayouts(): Observable<LayoutDto[]>             { return this.http.get<LayoutDto[]>(`${this.BASE}/layouts`); }
  uploadLayout(dto: UploadLayoutDto): Observable<LayoutDto> { return this.http.post<LayoutDto>(`${this.BASE}/layouts`, dto); }

  getSchedules(): Observable<ScheduleDto[]>         { return this.http.get<ScheduleDto[]>(`${this.BASE}/schedules`); }
  createSchedule(dto: CreateScheduleDto): Observable<ScheduleDto> { return this.http.post<ScheduleDto>(`${this.BASE}/schedules`, dto); }
  getRoutes(): Observable<RouteDto[]>                { return this.http.get<RouteDto[]>(`${this.BASE}/routes`); }
  updatePrice(scheduleId: number, dto: UpdatePriceDto): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.BASE}/schedules/${scheduleId}/price`, dto);
  }

  getBookings(): Observable<BookingList[]> { return this.http.get<BookingList[]>(`${this.BASE}/bookings`); }
}
