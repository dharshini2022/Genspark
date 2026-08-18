import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { VendorModel, VendorBasicResponse, VendorProfileResponse } from '../models/vendor.model';
import { PageResponse } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class VendorService {
  private baseUrl = environment.baseUrl + '/Vendor';

  constructor(private http: HttpClient) {}

  registerVendor(data: VendorModel): Observable<VendorModel> {
    return this.http.post<VendorModel>(`${this.baseUrl}/register`, data);
  }

  getVendorBasicProfileById(id: number): Observable<VendorBasicResponse> {
    return this.http.get<VendorBasicResponse>(`${this.baseUrl}/searchBasicById/${id}`);
  }

  getAllVendors(page: number = 1, size: number = 100): Observable<PageResponse<VendorProfileResponse>> {
    return this.http.get<PageResponse<VendorProfileResponse>>(`${this.baseUrl}/listVendors?pageNumber=${page}&pageSize=${size}`);
  }

  getVendorsByStatus(status: string): Observable<VendorProfileResponse[]> {
    return this.http.get<VendorProfileResponse[]>(`${this.baseUrl}/searchByStatus/${status}`);
  }

  approveVendor(id: number): Observable<VendorProfileResponse> {
    return this.http.get<VendorProfileResponse>(`${this.baseUrl}/approve/${id}`);
  }

  cancelVendor(id: number): Observable<VendorProfileResponse> {
    return this.http.get<VendorProfileResponse>(`${this.baseUrl}/cancel/${id}`);
  }

  getMyVendorProfile(): Observable<VendorProfileResponse> {
    return this.http.get<VendorProfileResponse>(`${this.baseUrl}/profile`);
  }

  toggleVendorStatus(vendorId?: number): Observable<VendorProfileResponse> {
    let url = this.baseUrl;
    if (vendorId !== undefined) {
      url += `?vendorId=${vendorId}`;
    }
    return this.http.put<VendorProfileResponse>(url, {});
  }

  getVendorProfileById(id: number): Observable<VendorProfileResponse> {
    return this.http.get<VendorProfileResponse>(`${this.baseUrl}/searchById/${id}`);
  }

  getAdminRevenueForVendor(vendorId: number): Observable<{ revenue: number }> {
    return this.http.get<{ revenue: number }>(`${this.baseUrl}/admin-revenue/${vendorId}`);
  }

  getMySettlements(pageNumber: number = 1, pageSize: number = 5): Observable<PageResponse<any>> {
    return this.http.get<PageResponse<any>>(`${environment.baseUrl}/vendor/settlements?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getVendorSettlementsByAdmin(vendorId: number, pageNumber: number = 1, pageSize: number = 5): Observable<PageResponse<any>> {
    return this.http.get<PageResponse<any>>(`${environment.baseUrl}/vendor/settlements/vendor/${vendorId}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getVendorSettlementsById(vendorId: number, pageNumber: number = 1, pageSize: number = 5): Observable<PageResponse<any>> {
    return this.http.get<PageResponse<any>>(`${environment.baseUrl}/vendor/settlements/vendor/${vendorId}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
}
