import { HttpClient } from "@angular/common/http";
import { Injectable, signal, effect } from "@angular/core";
import { environment } from "../../environments/environment";
import { Observable } from "rxjs";
import { tap } from "rxjs/operators";
import { AddToWishlistRequest, WishListResponse, WishListItemResponse } from "../models/wishlist.model";

@Injectable({
    providedIn: 'root'
})

export class WishlistService {
    private baseUrl = `${environment.baseUrl}/Wishlist`;
    wishlistCountSignal = signal<number>(this.getInitialWishlistCount());
    wishlistItemsSignal = signal<WishListItemResponse[]>([]);
    
    constructor(private http: HttpClient) {
        window.addEventListener('storage', (event) => {
            if (event.key === 'wishlist_count') {
                const val = event.newValue ? parseInt(event.newValue, 10) : 0;
                this.wishlistCountSignal.set(val);
            }
        });

        effect(() => {
            const count = this.wishlistCountSignal();
            sessionStorage.setItem('wishlist_count', count.toString());
        });
    }

    private getInitialWishlistCount(): number {
        const cached = sessionStorage.getItem('wishlist_count');
        return cached ? parseInt(cached, 10) : 0;
    }

    updateWishlistCount() {
        this.getWishlist().subscribe({
            next: (wishlist) => {
                this.wishlistItemsSignal.set(wishlist.items || []);
                this.wishlistCountSignal.set(wishlist.totalItems);
            },
            error: () => {
                this.wishlistItemsSignal.set([]);
                this.wishlistCountSignal.set(0);
            }
        });
    }

    getWishlist(): Observable<WishListResponse> {
        return this.http.get<WishListResponse>(this.baseUrl);
    }

    addToWishlist(request: AddToWishlistRequest): Observable<{ message: string; data: any }> {
        return this.http.post<{ message: string; data: any }>(this.baseUrl, request).pipe(
            tap(res => {
                if (res && res.data) {
                    const current = this.wishlistItemsSignal();
                    if (!current.some(item => item.id === res.data.id)) {
                        const updated = [...current, res.data];
                        this.wishlistItemsSignal.set(updated);
                        this.wishlistCountSignal.set(updated.length);
                    }
                }
            })
        );
    }

    removeFromWishlist(wishlistItemId: number): Observable<{ message: string; data: boolean }> {
        return this.http.delete<{ message: string; data: boolean }>(`${this.baseUrl}/${wishlistItemId}`).pipe(
            tap(res => {
                if (res && res.data) {
                    const updated = this.wishlistItemsSignal().filter(item => item.id !== wishlistItemId);
                    this.wishlistItemsSignal.set(updated);
                    this.wishlistCountSignal.set(updated.length);
                }
            })
        );
    }

    clearWishlist(): Observable<{ message: string }> {
        return this.http.delete<{ message: string }>(`${this.baseUrl}/clear`).pipe(
            tap(() => {
                this.wishlistItemsSignal.set([]);
                this.wishlistCountSignal.set(0);
            })
        );
    }
}