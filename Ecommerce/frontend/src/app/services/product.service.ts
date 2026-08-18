import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../environments/environment";
import { Observable } from "rxjs";
import { ProductResponse, PageResponse, ProductFilterRequest, CreateProductRequest, UpdateProductRequest, AddProductVariantRequest, ProductVariantResponse, CreateProductImageRequest, ProductSearchResult, UpdateProductVariantRequest } from "../models/product.model";

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private baseUrl = `${environment.baseUrl}/Product`;

  constructor(private http: HttpClient) {}

  getCatalog(filter?: ProductFilterRequest): Observable<PageResponse<ProductResponse>> {
    let params = new HttpParams();
    if (filter) {
      if (filter.pageNumber !== undefined) params = params.set('pageNumber', filter.pageNumber.toString());
      if (filter.pageSize !== undefined) params = params.set('pageSize', filter.pageSize.toString());
      if (filter.categoryId !== undefined) params = params.set('categoryId', filter.categoryId.toString());
      params = filter.sortBy ? params.set('sortBy', filter.sortBy): params.set('sortBy', 'newest');
      params = filter.sortOrder ? params.set('sortOrder', filter.sortOrder) : params.set('sortOrder', 'desc');
      if (filter.searchQuery != undefined ) params = params.set('searchQuery', filter.searchQuery);
      if (filter.minPrice !== undefined) params = params.set('minPrice', filter.minPrice.toString());
      if (filter.maxPrice !== undefined) params = params.set('maxPrice', filter.maxPrice.toString());
    }
    return this.http.get<PageResponse<ProductResponse>>(this.baseUrl, { params });
  }

  search(q: string): Observable<ProductSearchResult[]> {
    return this.http.get<ProductSearchResult[]>(`${this.baseUrl}/search?q=${q}`);
  }

  getById(id: number): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${this.baseUrl}/${id}`);
  }

  getVendorProducts(pageNumber: number = 1, pageSize: number = 5): Observable<PageResponse<ProductResponse>> {
    return this.http.get<PageResponse<ProductResponse>>(`${this.baseUrl}/vendor?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  getProductsByVendorId(vendorId: number, pageNumber: number = 1, pageSize: number = 5): Observable<PageResponse<ProductResponse>> {
    return this.http.get<PageResponse<ProductResponse>>(`${this.baseUrl}/vendor/${vendorId}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  createProduct(request: CreateProductRequest): Observable<{ message: string; data: ProductResponse }> {
    return this.http.post<{ message: string; data: ProductResponse }>(this.baseUrl, request);
  }

  updateProduct(id: number, request: UpdateProductRequest): Observable<{ message: string; data: ProductResponse }> {
    return this.http.patch<{ message: string; data: ProductResponse }>(`${this.baseUrl}/${id}`, request);
  }

  publishProduct(productId: number): Observable<{ message: string; data: ProductResponse }> {
    return this.http.patch<{ message: string; data: ProductResponse }>(`${this.baseUrl}/publish/${productId}`, {});
  }

  toggleProductStatus(id: number): Observable<{ message: string; data: ProductResponse }> {
    return this.http.patch<{ message: string; data: ProductResponse }>(`${this.baseUrl}/toggle/${id}`, {});
  }

  addVariant(productId: number, request: AddProductVariantRequest): Observable<{ message: string; data: ProductVariantResponse }> {
    return this.http.post<{ message: string; data: ProductVariantResponse }>(`${environment.baseUrl}/ProductVariant/${productId}`, request);
  }

  updateVariant(variantId: number, request: UpdateProductVariantRequest): Observable<{ message: string; data: ProductVariantResponse }> {
    return this.http.put<{ message: string; data: ProductVariantResponse }>(`${environment.baseUrl}/ProductVariant/${variantId}`, request);
  }

  toggleVariantStatus(variantId: number): Observable<{ message: string; data: any }> {
    return this.http.patch<{ message: string; data: any }>(`${environment.baseUrl}/ProductVariant/${variantId}`,{});
  }

  addVariantImage(variantId: number, request: CreateProductImageRequest): Observable<{ message: string; data: any }> {
    return this.http.post<{ message: string; data: any }>(`${environment.baseUrl}/ProductVariant/image/${variantId}`, request);
  }

  deleteVariantImage(imageId: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${environment.baseUrl}/ProductVariant/image/${imageId}`);
  }

  uploadVariantImage(file: File, productName: string, variantNo: number, imageNo: number, variantId: number): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('productName', productName);
    formData.append('variantNo', variantNo.toString());
    formData.append('imageNo', imageNo.toString());
    formData.append('variantId', variantId.toString());
    return this.http.post<{ imageUrl: string }>(`${environment.baseUrl}/Upload/variant-image`, formData);
  }
}