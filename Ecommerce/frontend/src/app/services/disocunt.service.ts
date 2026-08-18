import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../environments/environment";
import { Observable, map } from "rxjs";
import { PageResponse } from "../models/product.model";
import {
  DiscountResponse,
  PageRequest,
  CreateDiscountRequest,
  ToggleDiscountStatusResponse,
  CartEvaluationRequest,
  DiscountCartResponse
} from "../models/disocunt.model";

@Injectable({
  providedIn: 'root'
})
export class DiscountService {
  private baseUrl = `${environment.baseUrl}/Discount`;

  constructor(private http: HttpClient) {}

  getActiveDiscounts(request?: PageRequest): Observable<PageResponse<DiscountResponse>> {
    let params = new HttpParams();
    if (request) {
      if (request.pageNumber !== undefined) params = params.set('pageNumber', request.pageNumber.toString());
      if (request.pageSize !== undefined) params = params.set('pageSize', request.pageSize.toString());
      if (request.searchTerm !== undefined) params = params.set('searchTerm', request.searchTerm);
    }
    return this.http.get<PageResponse<DiscountResponse>>(`${this.baseUrl}/active`, { params });
  }

  getMyVendorDiscounts(pageNumber: number = 1, pageSize: number = 5): Observable<PageResponse<DiscountResponse>> {
    return this.http.get<PageResponse<DiscountResponse>>(`${this.baseUrl}/vendor?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getDiscountHisotry(request?: PageRequest): Observable<PageResponse<DiscountResponse>> {
    let params = new HttpParams();
    let pageNumber = 1;
    let pageSize = 5;
    if (request) {
      if (request.pageNumber !== undefined) {
        pageNumber = request.pageNumber;
        params = params.set('pageNumber', pageNumber.toString());
      }
      if (request.pageSize !== undefined) {
        pageSize = request.pageSize;
        params = params.set('pageSize', pageSize.toString());
      }
      if (request.searchTerm !== undefined) params = params.set('searchTerm', request.searchTerm);
    }
    return this.http.get<DiscountResponse[]>(`${this.baseUrl}/all`, { params }).pipe(
      map(items => {
        const list = items || [];
        const totalCount = list.length < pageSize
          ? (pageNumber - 1) * pageSize + list.length
          : pageNumber * pageSize + 1;
        const totalPages = Math.ceil(totalCount / pageSize);
        return {
          items: list,
          pageNumber: pageNumber,
          pageSize: pageSize,
          totalCount: totalCount,
          totalPages: totalPages,
          hasNext: pageNumber < totalPages,
          hasPrevious: pageNumber > 1
        };
      })
    );
  }

  getVendorDiscountsByAdmin(vendorId: number, pageNumber: number = 1, pageSize: number = 5): Observable<PageResponse<DiscountResponse>> {
    return this.http.get<PageResponse<DiscountResponse>>(`${this.baseUrl}/vendor/${vendorId}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  createDiscount(request: CreateDiscountRequest): Observable<{ message: string; data: DiscountResponse }> {
    return this.http.post<{ message: string; data: DiscountResponse }>(this.baseUrl, request);
  }

  deactivateDiscount(discountCode: string): Observable<{ message: string; data: ToggleDiscountStatusResponse }> {
    return this.http.patch<{ message: string; data: ToggleDiscountStatusResponse }>(`${this.baseUrl}/deactivate/${discountCode}`, {});
  }

  evaluateCartDiscounts(request: CartEvaluationRequest): Observable<DiscountCartResponse[]> {
    return this.http.post<DiscountCartResponse[]>(`${this.baseUrl}/evaluate`, request);
  }

  getApplicableLockedDiscounts(request: CartEvaluationRequest): Observable<DiscountResponse[]> {
    return this.http.post<DiscountResponse[]>(`${this.baseUrl}/applicable-locked`, request);
  }

  getDiscountsOfProduct(productId: number, categoryId: number, vendorId: number): Observable<DiscountResponse[]> {
    let params = new HttpParams()
      .set('productId', productId.toString())
      .set('categoryId', categoryId.toString())
      .set('vendorId', vendorId.toString());
    return this.http.get<DiscountResponse[]>(`${this.baseUrl}/product`, { params });
  }
}

    