import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../../services/cart.service';
import { ProductService } from '../../../services/product.service';
import { ToastService } from '../../../services/toast.service';
import { CartDiscount } from './cart-discount/cart-discount';

interface CartItem {
  id: number;
  name: string;
  category: string;
  price: number;
  quantity: number;
  imageUrl: string;
  isInStock: boolean;
  productId: number;
  categoryId: number;
  vendorId: number;
  stockQty: number;
  reservedStockQty: number;
}

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CartDiscount, ResolveImagePipe],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart implements OnInit {
  cartItems = signal<CartItem[]>([]);
  isLoading = signal<boolean>(true);
  couponCode = signal<string>('');
  couponApplied = signal<boolean>(false);
  couponDiscountAmount = signal<number>(0);
  platformCommission = signal<number>(20);
  shippingCost = signal<number>(0);
  taxes = signal<number>(0);

  totalItemsCount = computed(() => {
    return this.cartItems().reduce((acc, item) => acc + item.quantity, 0);
  });

  subTotal = computed(() => {
    return this.cartItems().reduce((acc, item) => acc + (item.price * item.quantity), 0);
  });

  grandTotal = computed(() => {
    const sub = this.subTotal();
    const discount = this.couponDiscountAmount();
    const comm = this.platformCommission();
    const ship = this.shippingCost();
    const tax = this.taxes();
    return Math.max(0, sub + ship + tax + comm - discount);
  });

  hasOutOfStockItems = computed(() => {
    return this.cartItems().some(item => !item.isInStock);
  });

  constructor(
    private cartService: CartService,
    private productService: ProductService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadCart();
  }

  loadCart(showSuccessToast = false) {
    this.cartService.getCart().subscribe({
      next: (cart) => {
        const items = cart.items.map(i => {
          return {
            id: i.id,
            name: i.productName,
            category: i.categoryName || 'Product',
            price: i.unitPrice,
            quantity: i.quantity,
            imageUrl: i.imageUrl || 'https://images.unsplash.com/photo-1596436889106-be35e843f974?w=150&q=80',
            isInStock: i.isInStock,
            productId: i.productId,
            categoryId: i.categoryId,
            vendorId: i.vendorId,
            stockQty: i.stockQty,
            reservedStockQty: i.reservedStockQty
          };
        });
        this.cartItems.set(items);
        this.cartService.cartCountSignal.set(cart.totalItems);
        this.shippingCost.set(cart.shippingAmount);
        this.taxes.set(cart.taxAmount);

        if (cart.discountCode) {
          this.couponCode.set(cart.discountCode);
          this.couponApplied.set(true);
          this.couponDiscountAmount.set(cart.discountAmount || 0);
          
          if (showSuccessToast) {
            this.toastService.success(`Discount Applied Successfully, You save ₹${(cart.discountAmount || 0).toFixed(2)}`);
          }
        } else {
          this.couponCode.set('');
          this.couponApplied.set(false);
          this.couponDiscountAmount.set(0);
        }

        if (cart.isDiscountExpired) {
          this.toastService.warning('Sorry! The applied discount code has expired');
        }
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Error fetching cart:', err);
        this.isLoading.set(false);
      }
    });
  }

  incrementQuantity(itemId: number) {
    const item = this.cartItems().find(c => c.id === itemId);
    if (!item) return;
    this.cartService.updateCartItemQuantity(itemId, { newQuantity: item.quantity + 1 }).subscribe({
      next: () => {
        this.toastService.info('Cart quantity updated');
        this.loadCart();
      }
    });
  }

  decrementQuantity(itemId: number) {
    const item = this.cartItems().find(c => c.id === itemId);
    if (!item) return;
    const newQty = item.quantity - 1;
    if (newQty <= 0) {
      this.removeItem(itemId);
    } else {
      this.cartService.updateCartItemQuantity(itemId, { newQuantity: newQty }).subscribe({
        next: () => {
          this.toastService.info('Cart quantity updated');
          this.loadCart();
        }
      });
    }
  }

  removeItem(itemId: number) {
    this.cartService.removeFromCart(itemId).subscribe({
      next: () => {
        this.toastService.success('Item removed from cart');
        this.loadCart();
      }
    });
  }

  applyCoupon() {
    const code = this.couponCode().trim().toUpperCase();
    if (!code) {
      this.cartService.removeDiscount().subscribe({
        next: () => {
          this.toastService.info('Discount removed');
          this.loadCart();
        }
      });
      return;
    }

    this.cartService.applyDiscount(code).subscribe({
      next: () => {
        this.loadCart(true);
      },
      error: (err: any) => {
        console.error('Error applying discount:', err);
        this.toastService.error(err.error?.message || 'Failed to apply discount.');
        this.couponCode.set('');
        this.couponApplied.set(false);
        this.couponDiscountAmount.set(0);
      }
    });
  }

  onDiscountSelected(discount: any) {
    if (!discount) {
      this.cartService.removeDiscount().subscribe({
        next: () => {
          this.toastService.info('Discount removed');
          this.couponCode.set('');
          this.couponApplied.set(false);
          this.couponDiscountAmount.set(0);
        },
        error: (err: any) => {
          console.error('Error removing discount:', err);
          this.toastService.error('Failed to remove discount.');
        }
      });
      return;
    }

    this.cartService.applyDiscount(discount.code).subscribe({
      next: () => {
        this.couponCode.set(discount.code);
        this.couponApplied.set(true);

        let amount = 0;
        if (discount.type === 'Percentage') {
          amount = (discount.value / 100) * this.subTotal();
        } else {
          amount = discount.value;
        }
        this.couponDiscountAmount.set(amount);
        this.toastService.success(`Discount Applied Successfully, You save ₹${amount.toFixed(2)}`);
      },
      error: (err: any) => {
        console.error('Error applying discount:', err);
        this.toastService.error(err.error?.message || 'Failed to apply discount.');
      }
    });
  }

  clearCart() {
    if (this.cartItems().length === 0) return;
    this.cartService.clearCart().subscribe({
      next: () => {
        this.toastService.success('Shopping cart cleared');
        this.loadCart();
      },
      error: (err: any) => {
        console.error('Error clearing cart:', err);
        this.loadCart();
      }
    });
  }

  proceedToCheckout() {
    if (this.hasOutOfStockItems()) {
      this.toastService.warning('Please remove out-of-stock items before checkout.');
      return;
    }

    for (const item of this.cartItems()) {
      const available = item.stockQty - item.reservedStockQty;
      if (item.quantity > available) {
        this.toastService.warning(`${item.name} has only ${available > 0 ? available : 0} qty left`);
        return;
      }
    }

    this.router.navigate(['/customer-home/checkout']);
  }
}
