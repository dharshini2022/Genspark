import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { CartService } from '../../../services/cart.service';
import { OrderService } from '../../../services/order.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ToastService } from '../../../services/toast.service';
import { UserAddress } from '../../../models/address.model';
import { AddressForm } from '../address-form/address-form';

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';

@Component({
  selector: 'app-order-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, AddressForm, ResolveImagePipe],
  templateUrl: './order-checkout.html',
  styleUrl: './order-checkout.css',
})
export class OrderCheckout implements OnInit {
  addresses = signal<UserAddress[]>([]);
  selectedAddressId = signal<number | null>(null);
  showAddressDropdown = signal<boolean>(false);

  idempotencyKey: string = '';
  isSubmitting = signal<boolean>(false);
  
  cartItems = signal<any[]>([]);
  subTotal = signal<number>(0);
  shippingCost = signal<number>(0);
  taxes = signal<number>(0);
  platformCommission = signal<number>(20);
  
  discountCode = signal<string>('');
  discountAmount = signal<number>(0);
  couponApplied = signal<boolean>(false);

  showAddressForm = signal<boolean>(false);
  selectedAddressForEdit = signal<UserAddress | null>(null);

  totalItemsCount = computed(() => {
    return this.cartItems().reduce((acc: number, item: any) => acc + item.quantity, 0);
  });

  grandTotal = computed(() => {
    const sub = this.subTotal();
    const discount = this.discountAmount();
    const comm = this.platformCommission();
    const ship = this.shippingCost();
    const tax = this.taxes();
    return Math.max(0, sub + ship + tax + comm - discount);
  });

  constructor(
    private userService: UserService,
    private cartService: CartService,
    private orderService: OrderService,
    private discountService: DiscountService,
    private toastService: ToastService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.idempotencyKey = this.generateUUID();
    this.loadCartAndEvaluateDiscount();
    this.loadAddresses();
  }

  private generateUUID(): string {
    if (typeof crypto !== 'undefined' && crypto.randomUUID) {
      return crypto.randomUUID();
    }
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  loadAddresses(): void {
    this.userService.getMyAddresses().subscribe({
      next: (data: UserAddress[]) => {
        this.addresses.set(data);
        if (data.length > 0 && !this.selectedAddressId()) {
          this.selectedAddressId.set(data[0].id || null);
        }
      },
      error: (err: any) => {
        console.error('Error fetching addresses:', err);
        this.toastService.error('Failed to load delivery addresses.');
      }
    });
  }

  loadCartAndEvaluateDiscount(): void {
    this.cartService.getCart().subscribe({
      next: (cart: any) => {
        const items = cart.items.map((i: any) => ({
          id: i.id,
          name: i.productName,
          category: i.categoryName || 'Product',
          price: i.unitPrice,
          quantity: i.quantity,
          imageUrl: i.imageUrl || 'https://images.unsplash.com/photo-1596436889106-be35e843f974?w=150&q=80',
          isInStock: i.isInStock,
          productId: i.productId,
          categoryId: i.categoryId,
          vendorId: i.vendorId
        }));
        
        this.cartItems.set(items);
        this.subTotal.set(items.reduce((acc: number, item: any) => acc + (item.price * item.quantity), 0));
        this.shippingCost.set(cart.shippingAmount);
        this.taxes.set(cart.taxAmount);

        if (cart.discountCode) {
          this.discountCode.set(cart.discountCode);
          this.couponApplied.set(true);
          this.discountAmount.set(cart.discountAmount || 0);
        } else {
          this.discountCode.set('');
          this.couponApplied.set(false);
          this.discountAmount.set(0);
        }

        if (cart.isDiscountExpired) {
          this.toastService.warning('Sorry! The applied discount code has expired');
        }
      },
      error: (err: any) => {
        console.error('Error loading cart:', err);
      }
    });
  }

  getSelectedAddress(): UserAddress | undefined {
    return this.addresses().find(a => a.id === this.selectedAddressId());
  }

  toggleAddressDropdown(): void {
    this.showAddressDropdown.set(!this.showAddressDropdown());
  }

  selectAddress(addressId: number): void {
    this.selectedAddressId.set(addressId);
    this.showAddressDropdown.set(false);
  }

  openAddAddressForm(): void {
    this.selectedAddressForEdit.set(null);
    this.showAddressForm.set(true);
    this.showAddressDropdown.set(false);
  }

  openEditAddressForm(address: UserAddress, event: MouseEvent): void {
    event.stopPropagation(); 
    this.selectedAddressForEdit.set(address);
    this.showAddressForm.set(true);
    this.showAddressDropdown.set(false);
  }

  closeAddressForm(): void {
    this.showAddressForm.set(false);
    this.selectedAddressForEdit.set(null);
  }

  saveAddress(payload: UserAddress): void {
    const addressId = payload.id;

    if (addressId !== undefined && addressId !== null) {
      this.userService.updateUserAddress(addressId, payload).subscribe({
        next: (updated: any) => {
          this.toastService.success('Address updated successfully!');
          this.showAddressForm.set(false);
          this.loadAddresses();
        },
        error: (err: any) => {
          console.error('Error updating address:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to update address.');
        }
      });
    } else {
      this.userService.addUserAddress(payload).subscribe({
        next: (created: any) => {
          this.toastService.success('Address added successfully!');
          this.showAddressForm.set(false);
          this.loadAddresses();
        },
        error: (err: any) => {
          console.error('Error adding address:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to add address.');
        }
      });
    }
  }

  proceedToPayment(): void {
    if (this.isSubmitting()) return;

    const addressId = this.selectedAddressId();
    if (!addressId) {
      this.toastService.warning('Please select or add a delivery address.');
      return;
    }

    this.isSubmitting.set(true);

    const payload = {
      userAddressId: addressId,
      discountCode: this.couponApplied() ? this.discountCode() : undefined
    };

    this.orderService.placeOrder(payload, this.idempotencyKey).subscribe({
      next: (orderResponse: any) => {
        const orderId = orderResponse.data?.orderId;
        const amount  = orderResponse.data?.total;

        if (!orderId) {
          this.toastService.error('Invalid order placement response.');
          this.isSubmitting.set(false);
          return;
        }

        this.router.navigate(['/customer-home/payment'], {
          queryParams: { orderId, amount }
        });
      },
      error: (err: any) => {
        console.error('Checkout error:', err);
        this.toastService.error(err.error?.message || 'Failed to place checkout order.');
        this.isSubmitting.set(false);
        this.idempotencyKey = this.generateUUID();
      }
    });
  }

  changeDiscountCode(): void {
    this.cartService.removeDiscount().subscribe({
      next: () => {
        this.router.navigate(['/customer-home/cart']);
      },
      error: () => {
        this.router.navigate(['/customer-home/cart']);
      }
    });
  }
}
