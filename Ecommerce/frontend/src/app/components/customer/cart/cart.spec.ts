import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Cart } from './cart';
import { CartService } from '../../../services/cart.service';
import { ProductService } from '../../../services/product.service';
import { ToastService } from '../../../services/toast.service';
import { Router, ActivatedRoute } from '@angular/router';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';

describe('Cart', () => {
  let component: Cart;
  let fixture: ComponentFixture<Cart>;
  let mockCartService: any;
  let mockProductService: any;
  let mockToastService: any;
  let mockRouter: any;

  const mockCartData = {
    items: [
      {
        id: 1,
        productId: 101,
        productName: 'Product 1',
        categoryName: 'Category 1',
        unitPrice: 100,
        quantity: 2,
        imageUrl: 'http://img.png',
        isInStock: true,
        categoryId: 10,
        vendorId: 5,
        stockQty: 10,
        reservedStockQty: 1
      },
      {
        id: 2,
        productId: 102,
        productName: 'Product 2',
        categoryName: null,
        unitPrice: 50,
        quantity: 1,
        imageUrl: '',
        isInStock: false,
        categoryId: 11,
        vendorId: 5,
        stockQty: 0,
        reservedStockQty: 0
      }
    ],
    totalItems: 3,
    shippingAmount: 10,
    taxAmount: 5,
    discountCode: 'SALE10',
    discountAmount: 20,
    isDiscountExpired: false
  };

  beforeEach(async () => {
    mockCartService = {
      getCart: vi.fn().mockReturnValue(of(mockCartData)),
      cartCountSignal: signal(3),
      updateCartItemQuantity: vi.fn().mockReturnValue(of(null)),
      removeFromCart: vi.fn().mockReturnValue(of(null)),
      removeDiscount: vi.fn().mockReturnValue(of(null)),
      applyDiscount: vi.fn().mockReturnValue(of(null)),
      clearCart: vi.fn().mockReturnValue(of(null))
    };

    mockProductService = {};

    mockToastService = {
      success: vi.fn(),
      warning: vi.fn(),
      info: vi.fn(),
      error: vi.fn()
    };

    mockRouter = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [Cart],
      providers: [
        { provide: ActivatedRoute, useValue: {} },
        { provide: CartService, useValue: mockCartService },
        { provide: ProductService, useValue: mockProductService },
        { provide: ToastService, useValue: mockToastService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Cart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load cart', () => {
    expect(component).toBeTruthy();
    expect(mockCartService.getCart).toHaveBeenCalled();
    expect(component.cartItems().length).toBe(2);
    expect(component.cartItems()[0].name).toBe('Product 1');
    expect(component.cartItems()[0].category).toBe('Category 1');
    expect(component.cartItems()[1].category).toBe('Product'); // null fallback
    expect(component.cartItems()[1].imageUrl).toBe('https://images.unsplash.com/photo-1596436889106-be35e843f974?w=150&q=80'); // empty fallback
    expect(component.couponCode()).toBe('SALE10');
    expect(component.couponApplied()).toBe(true);
    expect(component.couponDiscountAmount()).toBe(20);
  });

  it('should load cart and show success toast if requested', () => {
    mockCartService.getCart.mockReturnValue(of({
      ...mockCartData,
      discountAmount: 15,
      isDiscountExpired: true
    }));
    component.loadCart(true);
    expect(mockToastService.success).toHaveBeenCalledWith('Discount Applied Successfully, You save ₹15.00');
    expect(mockToastService.warning).toHaveBeenCalledWith('Sorry! The applied discount code has expired');
  });

  it('should reset discount fields if discountCode is not present', () => {
    mockCartService.getCart.mockReturnValue(of({
      ...mockCartData,
      discountCode: null,
      discountAmount: 0
    }));
    component.loadCart();
    expect(component.couponCode()).toBe('');
    expect(component.couponApplied()).toBe(false);
    expect(component.couponDiscountAmount()).toBe(0);
  });

  it('should handle error when fetching cart', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockCartService.getCart.mockReturnValue(throwError(() => new Error('API Error')));
    component.loadCart();
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should calculate correct computed fields', () => {
    // Total items count = 2 (quantity 2) + 1 (quantity 1) = 3
    expect(component.totalItemsCount()).toBe(3);

    // subTotal = 100 * 2 + 50 * 1 = 250
    expect(component.subTotal()).toBe(250);

    // grandTotal = Max(0, subTotal + ship + tax + comm - discount)
    // 250 + 10 (ship) + 5 (tax) + 20 (comm) - 20 (discount) = 265
    expect(component.grandTotal()).toBe(265);

    // hasOutOfStockItems should be true because product 2 isInStock is false
    expect(component.hasOutOfStockItems()).toBe(true);
  });

  it('should increment quantity', () => {
    component.incrementQuantity(1);
    expect(mockCartService.updateCartItemQuantity).toHaveBeenCalledWith(1, { newQuantity: 3 });
    expect(mockToastService.info).toHaveBeenCalledWith('Cart quantity updated');
  });

  it('should not increment if item not found', () => {
    component.incrementQuantity(999);
    expect(mockCartService.updateCartItemQuantity).not.toHaveBeenCalled();
  });

  it('should decrement quantity', () => {
    component.decrementQuantity(1);
    expect(mockCartService.updateCartItemQuantity).toHaveBeenCalledWith(1, { newQuantity: 1 });
  });

  it('should remove item when decrementing below 1', () => {
    const removeSpy = vi.spyOn(component, 'removeItem').mockImplementation(() => {});
    component.decrementQuantity(2);
    expect(removeSpy).toHaveBeenCalledWith(2);
  });

  it('should not decrement if item not found', () => {
    component.decrementQuantity(999);
    expect(mockCartService.updateCartItemQuantity).not.toHaveBeenCalled();
  });

  it('should remove item from cart', () => {
    component.removeItem(1);
    expect(mockCartService.removeFromCart).toHaveBeenCalledWith(1);
    expect(mockToastService.success).toHaveBeenCalledWith('Item removed from cart');
  });

  it('should apply coupon when code is not empty', () => {
    component.couponCode.set('SAVE20');
    component.applyCoupon();
    expect(mockCartService.applyDiscount).toHaveBeenCalledWith('SAVE20');
  });

  it('should remove discount if coupon code is empty when applying', () => {
    component.couponCode.set('');
    component.applyCoupon();
    expect(mockCartService.removeDiscount).toHaveBeenCalled();
    expect(mockToastService.info).toHaveBeenCalledWith('Discount removed');
  });

  it('should handle error when applying coupon', () => {
    mockCartService.applyDiscount.mockReturnValue(throwError(() => ({ error: { message: 'Invalid Coupon' } })));
    component.couponCode.set('BADCOUPON');
    component.applyCoupon();
    expect(mockToastService.error).toHaveBeenCalledWith('Invalid Coupon');
    expect(component.couponCode()).toBe('');
    expect(component.couponApplied()).toBe(false);
  });

  it('should handle onDiscountSelected for percentage discount', () => {
    component.onDiscountSelected({ code: 'PERC20', type: 'Percentage', value: 20 });
    expect(mockCartService.applyDiscount).toHaveBeenCalledWith('PERC20');
    expect(component.couponDiscountAmount()).toBe(50); // 20% of 250 = 50
    expect(mockToastService.success).toHaveBeenCalledWith('Discount Applied Successfully, You save ₹50.00');
  });

  it('should handle onDiscountSelected for fixed discount', () => {
    component.onDiscountSelected({ code: 'FIXED30', type: 'Fixed', value: 30 });
    expect(component.couponDiscountAmount()).toBe(30);
  });

  it('should handle error in onDiscountSelected', () => {
    mockCartService.applyDiscount.mockReturnValue(throwError(() => ({ error: { message: 'Err' } })));
    component.onDiscountSelected({ code: 'ERR' });
    expect(mockToastService.error).toHaveBeenCalledWith('Err');
  });

  it('should remove discount on onDiscountSelected(null)', () => {
    component.onDiscountSelected(null);
    expect(mockCartService.removeDiscount).toHaveBeenCalled();
    expect(mockToastService.info).toHaveBeenCalledWith('Discount removed');
    expect(component.couponCode()).toBe('');
  });

  it('should handle error when removing discount via onDiscountSelected(null)', () => {
    mockCartService.removeDiscount.mockReturnValue(throwError(() => new Error('Err')));
    component.onDiscountSelected(null);
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to remove discount.');
  });

  it('should clear cart if not empty', () => {
    component.clearCart();
    expect(mockCartService.clearCart).toHaveBeenCalled();
    expect(mockToastService.success).toHaveBeenCalledWith('Shopping cart cleared');
  });

  it('should not clear cart if already empty', () => {
    component.cartItems.set([]);
    component.clearCart();
    expect(mockCartService.clearCart).not.toHaveBeenCalled();
  });

  it('should handle error when clearing cart', () => {
    mockCartService.clearCart.mockReturnValue(throwError(() => new Error('Err')));
    component.clearCart();
    expect(mockCartService.getCart).toHaveBeenCalled();
  });

  it('should navigate to checkout when proceedToCheckout is valid', () => {
    // Make items in stock
    component.cartItems.set([
      {
        id: 1,
        productId: 101,
        name: 'Product 1',
        category: 'Category 1',
        price: 100,
        quantity: 2,
        imageUrl: 'img',
        isInStock: true,
        categoryId: 10,
        vendorId: 5,
        stockQty: 10,
        reservedStockQty: 1
      }
    ]);
    component.proceedToCheckout();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/checkout']);
  });

  it('should warn and not checkout if there are out-of-stock items', () => {
    component.proceedToCheckout();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please remove out-of-stock items before checkout.');
  });

  it('should warn and not checkout if quantity exceeds stock quantity minus reserved stock', () => {
    component.cartItems.set([
      {
        id: 1,
        productId: 101,
        name: 'Product 1',
        category: 'Category 1',
        price: 100,
        quantity: 5,
        imageUrl: 'img',
        isInStock: true,
        categoryId: 10,
        vendorId: 5,
        stockQty: 5,
        reservedStockQty: 2 // 5 - 2 = 3 available, quantity is 5
      }
    ]);
    component.proceedToCheckout();
    expect(mockToastService.warning).toHaveBeenCalledWith('Product 1 has only 3 qty left');
  });
});
