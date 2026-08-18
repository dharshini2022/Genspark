import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReviewResponse, CreateReviewRequest, UpdateReviewRequest } from '../models/review.model';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private baseUrl = `${environment.baseUrl}/Review`;

  constructor(private http: HttpClient) {}

  getProductReviews(productId: number): Observable<ReviewResponse[]> {
    return this.http.get<ReviewResponse[]>(`${this.baseUrl}/product/${productId}`);
  }

  getReviewByUserAndProduct(userId: number, productId: number): Observable<ReviewResponse> {
    return this.http.get<ReviewResponse>(`${this.baseUrl}/user/${userId}/product/${productId}`);
  }

  getMyReviews(): Observable<ReviewResponse[]> {
    return this.http.get<ReviewResponse[]>(`${this.baseUrl}/my`);
  }

  postReview(request: CreateReviewRequest): Observable<ReviewResponse> {
    return this.http.post<ReviewResponse>(this.baseUrl, request);
  }

  putReview(reviewId: number, request: UpdateReviewRequest): Observable<ReviewResponse> {
    return this.http.put<ReviewResponse>(`${this.baseUrl}/${reviewId}`, request);
  }

  deleteReview(reviewId: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.baseUrl}/${reviewId}`);
  }

  getOverallReviews(): Observable<ReviewResponse[]> {
    return this.http.get<ReviewResponse[]>(`${this.baseUrl}/overall`);
  }
}
