import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderCheckout } from './order-checkout';
import { UserService } from '../../../services/user.service';
import { CartService } from '../../../services/cart.service';
import { OrderService } from '../../../services/order.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ToastService } from '../../../services/toast.service';
import { Router, ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('OrderCheckout', () => {
  let component: OrderCheckout;
  let fixture: ComponentFixture<OrderCheckout>;

  let mockUserService: any;
  let mockCartService: any;
  let mockOrderService: any;
  let mockDiscountService: any;
  let mockToastService: any;
  let mockRouter: any;
  let mockActivatedRoute: any;

  const mockAddresses = [
    {
      id: 1,
      recipientName: 'Alice',
      phone: '1234567890',
      line1: '123 Main St',
      city: 'CityA',
      state: 'StateA',
      postalCode: '123456',
      country: 'India',
      label: 'Home'
    },
    {
      id: 2,
      recipientName: 'Bob',
      phone: '0987654321',
      line1: '456 Side St',
      city: 'CityB',
      state: 'StateB',
      postalCode: '654321',
      country: 'India',
      label: 'Work'
    }
  ];

  const mockCartResponse = {
    items: [
      {
        id: 101,
        productName: 'Product 1',
        categoryName: 'Tech',
        unitPrice: 100,
        quantity: 2,
        imageUrl: 'url1',
        isInStock: true,
        productId: 10,
        categoryId: 5,
        vendorId: 2
      },
      {
        id: 102,
        productName: 'Product 2',
        categoryName: 'Fashion',
        unitPrice: 50,
        quantity: 1,
        imageUrl: '',
        isInStock: true,
        productId: 11,
        categoryId: 6,
        vendorId: 2
      }
    ],
    shippingAmount: 15,
    taxAmount: 5,
    discountCode: 'TEST20',
    discountAmount: 10,
    isDiscountExpired: false
  };

  beforeEach(async () => {
    mockUserService = {
      getMyAddresses: vi.fn().mockReturnValue(of(mockAddresses)),
      addUserAddress: vi.fn().mockReturnValue(of({ id: 3 })),
      updateUserAddress: vi.fn().mockReturnValue(of({ id: 1 }))
    };

    mockCartService = {
      getCart: vi.fn().mockReturnValue(of(mockCartResponse)),
      removeDiscount: vi.fn().mockReturnValue(of({}))
    };

    mockOrderService = {
      placeOrder: vi.fn().mockReturnValue(of({ data: { orderId: 999, total: 260 } }))
    };

    mockDiscountService = {};

    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    mockRouter = {
      navigate: vi.fn().mockResolvedValue(true)
    };

    mockActivatedRoute = {};

    await TestBed.configureTestingModule({
      imports: [OrderCheckout],
      providers: [
        { provide: UserService, useValue: mockUserService },
        { provide: CartService, useValue: mockCartService },
        { provide: OrderService, useValue: mockOrderService },
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ToastService, useValue: mockToastService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();
  });

  const createComponent = () => {
    fixture = TestBed.createComponent(OrderCheckout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  describe('Initialization (ngOnInit)', () => {
    it('should load addresses and auto-select first address if none is selected', () => {
      createComponent();
      expect(mockUserService.getMyAddresses).toHaveBeenCalled();
      expect(component.addresses()).toEqual(mockAddresses);
      expect(component.selectedAddressId()).toBe(1);
    });

    it('should handle address loading error', () => {
      mockUserService.getMyAddresses.mockReturnValue(throwError(() => new Error('Db error')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      createComponent();

      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load delivery addresses.');
      consoleSpy.mockRestore();
    });

    it('should map cart items and calculate totals on init', () => {
      createComponent();
      expect(mockCartService.getCart).toHaveBeenCalled();
      expect(component.cartItems().length).toBe(2);
      expect(component.cartItems()[0].name).toBe('Product 1');
      expect(component.cartItems()[1].imageUrl).toBe('https://images.unsplash.com/photo-1596436889106-be35e843f974?w=150&q=80'); // Fallback URL
      expect(component.subTotal()).toBe(250); // (100 * 2) + (50 * 1)
      expect(component.shippingCost()).toBe(15);
      expect(component.taxes()).toBe(5);
      expect(component.discountCode()).toBe('TEST20');
      expect(component.couponApplied()).toBe(true);
      expect(component.discountAmount()).toBe(10);
    });

    it('should clear discount details if no discount code is applied', () => {
      const cartNoDiscount = { ...mockCartResponse, discountCode: null, discountAmount: null };
      mockCartService.getCart.mockReturnValue(of(cartNoDiscount));

      createComponent();

      expect(component.discountCode()).toBe('');
      expect(component.couponApplied()).toBe(false);
      expect(component.discountAmount()).toBe(0);
    });

    it('should show toast warning if discount code has expired', () => {
      const cartExpiredDiscount = { ...mockCartResponse, isDiscountExpired: true };
      mockCartService.getCart.mockReturnValue(of(cartExpiredDiscount));

      createComponent();

      expect(mockToastService.warning).toHaveBeenCalledWith('Sorry! The applied discount code has expired');
    });

    it('should log error when cart fails to load', () => {
      mockCartService.getCart.mockReturnValue(throwError(() => new Error('Cart failed')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      createComponent();

      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('Address Dropdown Actions', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should toggle address dropdown visibility', () => {
      expect(component.showAddressDropdown()).toBe(false);
      component.toggleAddressDropdown();
      expect(component.showAddressDropdown()).toBe(true);
    });

    it('should select an address and close dropdown', () => {
      component.showAddressDropdown.set(true);
      component.selectAddress(2);
      expect(component.selectedAddressId()).toBe(2);
      expect(component.showAddressDropdown()).toBe(false);
    });

    it('should return current selected address object', () => {
      component.selectedAddressId.set(2);
      const selected = component.getSelectedAddress();
      expect(selected).toEqual(mockAddresses[1]);
    });
  });

  describe('Address Modal Management', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should open new address form', () => {
      component.showAddressForm.set(false);
      component.showAddressDropdown.set(true);
      component.openAddAddressForm();

      expect(component.selectedAddressForEdit()).toBeNull();
      expect(component.showAddressForm()).toBe(true);
      expect(component.showAddressDropdown()).toBe(false);
    });

    it('should open edit address form with stopPropagation on mouse event', () => {
      const mockEvent = { stopPropagation: vi.fn() } as unknown as MouseEvent;
      component.showAddressDropdown.set(true);
      component.openEditAddressForm(mockAddresses[1], mockEvent);

      expect(mockEvent.stopPropagation).toHaveBeenCalled();
      expect(component.selectedAddressForEdit()).toEqual(mockAddresses[1]);
      expect(component.showAddressForm()).toBe(true);
      expect(component.showAddressDropdown()).toBe(false);
    });

    it('should close address form', () => {
      component.showAddressForm.set(true);
      component.selectedAddressForEdit.set(mockAddresses[0]);
      component.closeAddressForm();

      expect(component.showAddressForm()).toBe(false);
      expect(component.selectedAddressForEdit()).toBeNull();
    });
  });

  describe('Save Address Actions', () => {
    beforeEach(() => {
      createComponent();
      mockUserService.getMyAddresses.mockClear();
    });

    it('should update existing address if ID is present', () => {
      component.saveAddress(mockAddresses[0]);
      expect(mockUserService.updateUserAddress).toHaveBeenCalledWith(1, mockAddresses[0]);
      expect(mockToastService.success).toHaveBeenCalledWith('Address updated successfully!');
      expect(component.showAddressForm()).toBe(false);
      expect(mockUserService.getMyAddresses).toHaveBeenCalled();
    });

    it('should handle address update error', () => {
      mockUserService.updateUserAddress.mockReturnValue(throwError(() => new Error('Error')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveAddress(mockAddresses[0]);

      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to update address.');
      consoleSpy.mockRestore();
    });

    it('should add new address if ID is absent', () => {
      const newAddr = { ...mockAddresses[0], id: undefined };
      component.saveAddress(newAddr);
      expect(mockUserService.addUserAddress).toHaveBeenCalledWith(newAddr);
      expect(mockToastService.success).toHaveBeenCalledWith('Address added successfully!');
      expect(component.showAddressForm()).toBe(false);
      expect(mockUserService.getMyAddresses).toHaveBeenCalled();
    });

    it('should handle address addition error', () => {
      const newAddr = { ...mockAddresses[0], id: undefined };
      mockUserService.addUserAddress.mockReturnValue(throwError(() => new Error('Error')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveAddress(newAddr);

      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to add address.');
      consoleSpy.mockRestore();
    });
  });

  describe('Proceed to Payment (Place Order)', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should show warning toast if no address is selected', () => {
      component.selectedAddressId.set(null);
      component.proceedToPayment();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please select or add a delivery address.');
      expect(mockOrderService.placeOrder).not.toHaveBeenCalled();
    });

    it('should place order and navigate on success', () => {
      component.selectedAddressId.set(1);
      component.couponApplied.set(true);
      component.discountCode.set('TEST20');

      component.proceedToPayment();

      expect(mockOrderService.placeOrder).toHaveBeenCalledWith({
        userAddressId: 1,
        discountCode: 'TEST20'
      });
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/payment'], {
        queryParams: { orderId: 999, amount: 260 }
      });
    });

    it('should place order without discount if coupon is not applied', () => {
      component.selectedAddressId.set(2);
      component.couponApplied.set(false);

      component.proceedToPayment();

      expect(mockOrderService.placeOrder).toHaveBeenCalledWith({
        userAddressId: 2,
        discountCode: undefined
      });
    });

    it('should show toast error if order response lacks orderId', () => {
      mockOrderService.placeOrder.mockReturnValue(of({ data: {} }));
      component.selectedAddressId.set(1);

      component.proceedToPayment();

      expect(mockToastService.error).toHaveBeenCalledWith('Invalid order placement response.');
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should handle order placement error', () => {
      mockOrderService.placeOrder.mockReturnValue(throwError(() => ({
        error: { message: 'Out of stock' }
      })));
      component.selectedAddressId.set(1);
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.proceedToPayment();

      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Out of stock');
      consoleSpy.mockRestore();
    });
  });

  describe('Change / Remove Discount Code', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should remove discount and navigate to cart', () => {
      component.changeDiscountCode();
      expect(mockCartService.removeDiscount).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/cart']);
    });

    it('should navigate to cart even if removeDiscount fails', () => {
      mockCartService.removeDiscount.mockReturnValue(throwError(() => new Error('Error')));
      component.changeDiscountCode();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/cart']);
    });
  });

  describe('Computed Signals', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should compute totalItemsCount correctly', () => {
      expect(component.totalItemsCount()).toBe(3); // 2 of Product 1 + 1 of Product 2
    });

    it('should compute grandTotal correctly', () => {
      // subTotal = 250, shipping = 15, taxes = 5, platform = 20, discount = 10
      // 250 + 15 + 5 + 20 - 10 = 280
      expect(component.grandTotal()).toBe(280);
    });

    it('should compute grandTotal as 0 if discount exceeds total cost', () => {
      component.discountAmount.set(500);
      expect(component.grandTotal()).toBe(0);
    });
  });

  describe('HTML Template rendering and actions', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should render address dropdown and select address / add address options when showAddressDropdown is true', () => {
      component.showAddressDropdown.set(true);
      fixture.detectChanges();

      const dropdownList = fixture.debugElement.nativeElement.querySelector('.address-dropdown-list');
      expect(dropdownList).toBeTruthy();

      const items = dropdownList.querySelectorAll('.address-dropdown-item');
      expect(items.length).toBe(2);

      // Click to select address
      items[1].click();
      expect(component.selectedAddressId()).toBe(2);

      // Click Add New Address
      const addBtn = fixture.debugElement.nativeElement.querySelector('.add-address-dotted-btn');
      expect(addBtn).toBeTruthy();
      addBtn.click();
      expect(component.showAddressForm()).toBe(true);
    });

    it('should render no-address state with buttons when no address is selected', () => {
      component.selectedAddressId.set(null);
      fixture.detectChanges();

      const noAddrState = fixture.debugElement.nativeElement.querySelector('.no-address-state');
      expect(noAddrState).toBeTruthy();

      const actionBtn = noAddrState.querySelector('.add-first-address-btn');
      expect(actionBtn).toBeTruthy();
      actionBtn.click();
      expect(component.showAddressDropdown()).toBe(true);
    });

    it('should render address form modal when showAddressForm is true', () => {
      component.showAddressForm.set(true);
      fixture.detectChanges();

      const overlay = fixture.debugElement.nativeElement.querySelector('.modal-overlay');
      expect(overlay).toBeTruthy();
      overlay.click();
      expect(component.showAddressForm()).toBe(false);
    });
  });
});
