import { Component, Input, OnChanges, SimpleChanges, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DiscountService } from '../../../../services/disocunt.service';
import { ToastService } from '../../../../services/toast.service';
import { DiscountResponse, CartEvaluationRequest, CartItemEvaluationResponse } from '../../../../models/disocunt.model';

@Component({
  selector: 'app-cart-discount',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cart-discount.html',
  styleUrl: './cart-discount.css'
})
export class CartDiscount implements OnChanges {
  @Input() subTotal: number = 0;
  @Input() cartItems: any[] = [];
  @Input() appliedDiscountCode: string = '';

  @Output() selectDiscount = new EventEmitter<any>();

  applicableLockedDiscounts = signal<DiscountResponse[]>([]);

  private lastSubTotal: number = -1;
  private lastItemsKey: string = '';

  constructor(
    private discountService: DiscountService,
    private toastService: ToastService
  ) {}

  copyToClipboard(code: string, event: MouseEvent): void {
    event.stopPropagation(); // Prevent card click event
    navigator.clipboard.writeText(code).then(() => {
      this.toastService.success('Discount code copied to clipboard!');
    }).catch(err => {
      console.error('Could not copy text: ', err);
      this.toastService.error('Failed to copy discount code.');
    });
  }

  applyDiscountCode(discount: DiscountResponse): void {
    if (discount.code === this.appliedDiscountCode) {
      // Toggle off (remove coupon)
      this.selectDiscount.emit(null);
      return;
    }
    if (this.getProgressPercent(discount) < 100) {
      this.toastService.warning('This discount is still locked. Add more items to unlock it.');
      return;
    }
    this.selectDiscount.emit(discount);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['subTotal'] || changes['cartItems']) {
      this.loadApplicableLockedDiscounts();
    }
  }

  loadApplicableLockedDiscounts(): void {
    const inStockItems = this.cartItems.filter(item => item.isInStock);
    if (inStockItems.length === 0) {
      this.applicableLockedDiscounts.set([]);
      this.lastSubTotal = -1;
      this.lastItemsKey = '';
      return;
    }

    const itemsKey = inStockItems.map(item => `${item.productId}:${item.quantity}`).join(',');
    
    // Skip redundant network requests if the cart contents and subtotal have not changed.
    if (this.subTotal === this.lastSubTotal && itemsKey === this.lastItemsKey) {
      return;
    }

    this.lastSubTotal = this.subTotal;
    this.lastItemsKey = itemsKey;

    const itemsPayload: CartItemEvaluationResponse[] = inStockItems.map(item => ({
      id: item.id,
      productId: item.productId,
      categoryId: item.categoryId,
      vendorId: item.vendorId,
      subTotal: item.price * item.quantity
    }));

    const request: CartEvaluationRequest = {
      subTotal: this.subTotal,
      items: itemsPayload
    };

    this.discountService.getApplicableLockedDiscounts(request).subscribe({
      next: (discounts) => {
        this.applicableLockedDiscounts.set(discounts);
      },
      error: (err) => {
        console.error('Error fetching locked discounts:', err);
      }
    });
  }

  getProgressPercent(discount: DiscountResponse): number {
    if (!discount.minOrderValue || discount.minOrderValue <= 0) return 100;
    const pct = (this.subTotal / discount.minOrderValue) * 100;
    return pct >= 100 ? 100 : Math.max(0, pct);
  }

  getRemainingAmount(discount: DiscountResponse): number {
    return Math.max(0, discount.minOrderValue - this.subTotal);
  }
}
