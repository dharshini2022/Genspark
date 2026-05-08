import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  BusDto, AddBusDto, LayoutDto, UploadLayoutDto, ScheduleDto,
  CreateScheduleDto, UpdatePriceDto, BookingList, OperatorProfile, MessageResponse, RouteDto,
  OperatorDetailedRevenueDto, SchedulePassengerDto, OfficeDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class OperatorService {
  private BASE = '/api/operator';
  constructor(private http: HttpClient) {}

  getProfile(): Observable<OperatorProfile>        { return this.http.get<OperatorProfile>(`${this.BASE}/profile`); }
  getBuses(): Observable<BusDto[]>                 { return this.http.get<BusDto[]>(`${this.BASE}/buses`); }
  addBus(data: FormData): Observable<BusDto>       { return this.http.post<BusDto>(`${this.BASE}/buses`, data); }
  bringDownBus(id: number): Observable<MessageResponse> { return this.http.put<MessageResponse>(`${this.BASE}/buses/${id}/down`, {}); }
  bringUpBus(id: number): Observable<MessageResponse> { return this.http.put<MessageResponse>(`${this.BASE}/buses/${id}/up`, {}); }

  getLayouts(): Observable<LayoutDto[]>             { return this.http.get<LayoutDto[]>(`${this.BASE}/layouts`); }
  uploadLayout(dto: UploadLayoutDto): Observable<LayoutDto> { return this.http.post<LayoutDto>(`${this.BASE}/layouts`, dto); }

  getSchedules(): Observable<ScheduleDto[]>         { return this.http.get<ScheduleDto[]>(`${this.BASE}/schedules`); }
  createSchedule(dto: CreateScheduleDto): Observable<ScheduleDto> { return this.http.post<ScheduleDto>(`${this.BASE}/schedules`, dto); }
  getRoutes(): Observable<RouteDto[]>                { return this.http.get<RouteDto[]>(`${this.BASE}/routes`); }
  updatePrice(scheduleId: number, dto: UpdatePriceDto): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.BASE}/schedules/${scheduleId}/price`, dto);
  }
  cancelSchedule(scheduleId: number): Observable<MessageResponse> {
    return this.http.put<MessageResponse>(`${this.BASE}/schedules/${scheduleId}/cancel`, {});
  }

  getOffices(): Observable<OfficeDto[]> { return this.http.get<OfficeDto[]>(`${this.BASE}/offices`); }
  addOffice(dto: OfficeDto): Observable<MessageResponse> { return this.http.post<MessageResponse>(`${this.BASE}/offices`, dto); }
  updateOffice(id: number, dto: OfficeDto): Observable<MessageResponse> { return this.http.put<MessageResponse>(`${this.BASE}/offices/${id}`, dto); }

  getBookings(): Observable<BookingList[]> { return this.http.get<BookingList[]>(`${this.BASE}/bookings`); }
  cancelBooking(bookingId: number, dto: { reason: string }): Observable<any> { return this.http.put<any>(`${this.BASE}/bookings/${bookingId}/cancel`, dto); }

  getDetailedRevenue(startDate?: string, endDate?: string, busId?: number): Observable<OperatorDetailedRevenueDto> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (busId) params = params.set('busId', busId.toString());
    return this.http.get<OperatorDetailedRevenueDto>(`${this.BASE}/revenue`, { params });
  }

  getSchedulePassengers(scheduleId: number): Observable<SchedulePassengerDto[]> {
    return this.http.get<SchedulePassengerDto[]>(`${this.BASE}/schedules/${scheduleId}/passengers`);
  }
}
