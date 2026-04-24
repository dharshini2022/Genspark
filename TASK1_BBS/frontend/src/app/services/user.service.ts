import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  BusSearchResult, SeatBlockRequest, SeatBlockResponse,
  CreateBookingDto, BookingResponse, BookingList, CancelBookingDto,
  CancelResponse, PaymentRequest, PaymentResponse, ProfileDto, UpdateProfileDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class UserService {
  lastSearchResults: BusSearchResult[] = [];
  lastFilters: { source: string, destination: string, date: string } | null = null;
  constructor(private http: HttpClient) {}

  searchBuses(source: string, destination: string, date: string): Observable<BusSearchResult[]> {
    this.lastFilters = { source, destination, date };
    return this.http.get<BusSearchResult[]>(`/api/buses/search`, {
      params: { source, destination, date }
    });
  }

  getScheduleDetails(id: number): Observable<BusSearchResult> {
    return this.http.get<BusSearchResult>(`/api/buses/schedule/${id}`);
  }

  blockSeats(dto: SeatBlockRequest): Observable<SeatBlockResponse> {
    return this.http.post<SeatBlockResponse>('/api/seats/block', dto);
  }

  createBooking(dto: CreateBookingDto): Observable<BookingResponse> {
    return this.http.post<BookingResponse>('/api/bookings', dto);
  }

  getMyBookings(): Observable<BookingList[]> {
    return this.http.get<BookingList[]>('/api/bookings/my');
  }

  getBooking(id: number): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(`/api/bookings/${id}`);
  }

  cancelBooking(id: number, dto: CancelBookingDto): Observable<CancelResponse> {
    return this.http.delete<CancelResponse>(`/api/bookings/${id}`, { body: dto });
  }

  pay(dto: PaymentRequest): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>('/api/payments', dto);
  }

  getProfile(): Observable<ProfileDto> { return this.http.get<ProfileDto>('/api/profile'); }
  updateProfile(dto: UpdateProfileDto): Observable<ProfileDto> { return this.http.put<ProfileDto>('/api/profile', dto); }
}
