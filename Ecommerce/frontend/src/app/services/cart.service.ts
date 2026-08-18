import { Injectable, signal, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { CartResponse, AddToCartRequest, UpdateCartItemRequest } from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private baseUrl = `${environment.baseUrl}/Cart`;
  cartCountSignal = signal<number>(this.getInitialCartCount());

  constructor(private http: HttpClient) {
    window.addEventListener('storage', (event) => {
      if (event.key === 'cart_count') {
        const val = event.newValue ? parseInt(event.newValue, 10) : 0;
        this.cartCountSignal.set(val);
      }
    });

    effect(() => {
      const count = this.cartCountSignal();
      sessionStorage.setItem('cart_count', count.toString());
    });
  }

  private getInitialCartCount(): number {
    const cached = sessionStorage.getItem('cart_count');
    return cached ? parseInt(cached, 10) : 0;
  }

  updateCartCount() {
    this.getCart().subscribe({
      next: (cart) => this.cartCountSignal.set(cart.totalItems),
      error: () => this.cartCountSignal.set(0)
    });
  }

  getCart(): Observable<CartResponse> {
    return this.http.get<CartResponse>(this.baseUrl);
  }

  addToCart(request: AddToCartRequest): Observable<{ message: string; data: any }> {
    return this.http.post<{ message: string; data: any }>(this.baseUrl, request);
  }

  removeFromCart(cartItemId: number): Observable<{ message: string; data: boolean }> {
    return this.http.delete<{ message: string; data: boolean }>(`${this.baseUrl}/${cartItemId}`);
  }

  updateCartItemQuantity(cartItemId: number, request: UpdateCartItemRequest): Observable<{ message: string; data?: any }> {
    return this.http.patch<{ message: string; data?: any }>(`${this.baseUrl}/${cartItemId}`, request);
  }

  clearCart(): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/clear`);
  }

  applyDiscount(discountCode: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/apply-discount`, { discountCode });
  }

  removeDiscount(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/remove-discount`, {});
  }
}
