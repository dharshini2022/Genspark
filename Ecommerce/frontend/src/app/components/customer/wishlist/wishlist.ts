import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { WishlistService } from '../../../services/wishlist.service';
import { ProductService } from '../../../services/product.service';
import { CartService } from '../../../services/cart.service';
import { ToastService } from '../../../services/toast.service';

interface WishlistItem {
  id: number;
  variantId: number;
  productId: number;
  name: string;
  category: string;
  price: number;
  imageUrl: string;
  isInStock: boolean;
}

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ResolveImagePipe],
  templateUrl: './wishlist.html',
  styleUrl: './wishlist.css',
})
export class Wishlist implements OnInit {
  wishlistItems = signal<WishlistItem[]>([]);
  isLoading = signal<boolean>(true);

  inStockItemsCount = computed(() => {
    return this.wishlistItems().filter(item => item.isInStock).length;
  });

  constructor(
    private wishlistService: WishlistService,
    private productService: ProductService,
    private cartService: CartService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadWishlist();
  }

  loadWishlist() {
    this.wishlistService.getWishlist().subscribe({
      next: (wishlist) => {
        const items = wishlist.items.map(i => {
          return {
            id: i.id,
            variantId: i.variantId,
            productId: i.productId,
            name: i.productName,
            category: i.categoryName || 'Product',
            price: i.unitPrice,
            imageUrl: i.imageUrl || 'https://images.unsplash.com/photo-1596436889106-be35e843f974?w=150&q=80',
            isInStock: i.isInStock
          };
        });
        this.wishlistItems.set(items);
        this.wishlistService.wishlistItemsSignal.set(wishlist.items || []);
        this.wishlistService.wishlistCountSignal.set(wishlist.totalItems);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching wishlist:', err);
        this.isLoading.set(false);
      }
    });
  }

  removeItem(itemId: number) {
    this.wishlistService.removeFromWishlist(itemId).subscribe({
      next: () => {
        this.toastService.success('Item removed from wishlist');
        this.loadWishlist();
      },
      error: (err) => {
        console.error('Error removing item from wishlist:', err);
        this.toastService.error('Failed to remove item from wishlist');
      }
    });
  }

  addToCart(item: WishlistItem) {
    if (!item.isInStock) {
      this.toastService.warning('Item is currently out of stock');
      return;
    }
    this.cartService.addToCart({
      variantId: item.variantId,
      quantity: 1
    }).subscribe({
      next: () => {
        this.cartService.updateCartCount();
        
        // Remove from wishlist on successful add-to-cart
        this.wishlistService.removeFromWishlist(item.id).subscribe({
          next: () => {
            this.toastService.success(`Added ${item.name} to cart & removed from wishlist`);
            this.loadWishlist();
          },
          error: (removeErr) => {
            console.error('Error removing from wishlist after adding to cart:', removeErr);
            this.toastService.success(`Added ${item.name} to cart`);
            this.loadWishlist();
          }
        });
      },
      error: (err) => {
        console.error('Error adding to cart:', err);
        this.toastService.error('Failed to add item to cart');
      }
    });
  }

  clearWishlist() {
    if (this.wishlistItems().length === 0) return;
    this.wishlistService.clearWishlist().subscribe({
      next: () => {
        this.toastService.success('Wishlist cleared');
        this.loadWishlist();
      },
      error: (err) => {
        console.error('Error clearing wishlist:', err);
        this.toastService.error('Failed to clear wishlist');
      }
    });
  }

  addAllToCart() {
    const inStock = this.wishlistItems().filter(item => item.isInStock);
    if (inStock.length === 0) return;

    let addedCount = 0;
    const addNext = (index: number) => {
      if (index >= inStock.length) {
        this.toastService.success(`Added ${addedCount} item(s) to cart & updated wishlist`);
        this.cartService.updateCartCount();
        this.loadWishlist();
        return;
      }

      const item = inStock[index];
      this.cartService.addToCart({ variantId: item.variantId, quantity: 1 }).subscribe({
        next: () => {
          addedCount++;
          // Remove item from wishlist
          this.wishlistService.removeFromWishlist(item.id).subscribe({
            next: () => addNext(index + 1),
            error: () => addNext(index + 1)
          });
        },
        error: (err) => {
          console.error(`Error adding variant ${item.variantId} to cart:`, err);
          addNext(index + 1);
        }
      });
    };

    addNext(0);
  }
}
