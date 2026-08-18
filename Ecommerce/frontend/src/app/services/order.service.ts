import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { OrderSummaryResponse } from '../models/order.model';
import { PageResponse } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private baseUrl = environment.baseUrl + '/Order';

  constructor(private http: HttpClient) { }

  private getQueryParams(pageNumber?: number, pageSize?: number, searchTerm?: string): HttpParams {
    let params = new HttpParams();
    if (pageNumber) params = params.set('pageNumber', pageNumber.toString());
    if (pageSize) params = params.set('pageSize', pageSize.toString());
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return params;
  }

  getVendorOrders(pageNumber?: number, pageSize?: number, searchTerm?: string): Observable<PageResponse<OrderSummaryResponse>> {
    const params = this.getQueryParams(pageNumber, pageSize, searchTerm);
    return this.http.get<PageResponse<OrderSummaryResponse>>(`${this.baseUrl}/vendor-orders`, { params });
  }

  getVendorOrdersById(vendorId: number, pageNumber?: number, pageSize?: number, searchTerm?: string): Observable<PageResponse<OrderSummaryResponse>> {
    const params = this.getQueryParams(pageNumber, pageSize, searchTerm);
    return this.http.get<PageResponse<OrderSummaryResponse>>(`${this.baseUrl}/vendor-orders/${vendorId}`, { params });
  }

  getMyOrders(pageNumber?: number, pageSize?: number, searchTerm?: string): Observable<PageResponse<OrderSummaryResponse>> {
    const params = this.getQueryParams(pageNumber, pageSize, searchTerm);
    return this.http.get<PageResponse<OrderSummaryResponse>>(`${this.baseUrl}/my-orders`, { params });
  }

  getAllOrders(pageNumber?: number, pageSize?: number, searchTerm?: string): Observable<PageResponse<OrderSummaryResponse>> {
    const params = this.getQueryParams(pageNumber, pageSize, searchTerm);
    return this.http.get<PageResponse<OrderSummaryResponse>>(`${this.baseUrl}/all`, { params });
  }

  getOrderDetail(orderId: number): Observable<OrderSummaryResponse> {
    return this.http.get<OrderSummaryResponse>(`${this.baseUrl}/${orderId}`);
  }

  placeOrder(request: { userAddressId: number; discountCode?: string }, idempotencyKey?: string): Observable<any> {
    let headers = new HttpHeaders();
    if (idempotencyKey) {
      headers = headers.set('Idempotency-Key', idempotencyKey);
    }
    return this.http.post<any>(this.baseUrl, request, { headers });
  }

  makePayment(orderId: number, paymentMethodId: string): Observable<any> {
    return this.http.post<any>(`${environment.baseUrl}/Payment/pay/${orderId}`, { paymentMethodId });
  }

  createCheckoutSession(orderId: number): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(`${environment.baseUrl}/Payment/checkout-session/${orderId}`, {});
  }
}
