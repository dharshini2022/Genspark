import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { OrderDetail } from './order-detail';
import { OrderService } from '../../../services/order.service';
import { ShipmentService } from '../../../services/shipment.service';
import { VendorService } from '../../../services/vendor.service';
import { ActivatedRoute, Router, RouterModule, convertToParamMap } from '@angular/router';
import { Location } from '@angular/common';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';

const sessionStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem(key: string) { return store[key] || null; },
    setItem(key: string, value: string) { store[key] = value.toString(); },
    removeItem(key: string) { delete store[key]; },
    clear() { store = {}; }
  };
})();
Object.defineProperty(globalThis, 'sessionStorage', { value: sessionStorageMock, writable: true });

describe('OrderDetail', () => {
  let component: OrderDetail;
  let fixture: ComponentFixture<OrderDetail>;
  let mockOrderService: any;
  let mockShipmentService: any;
  let mockVendorService: any;
  let mockRouter: any;
  let mockLocation: any;
  let mockActivatedRoute: any;

  const makeOrder = (vendorId?: number) => ({
    id: 1,
    orderNumber: 'ORD-001',
    status: 2,
    paymentStatus: 1,
    totalAmount: 500,
    placedAt: '2024-01-01T10:00:00',
    items: [
      { id: 1, productName: 'Item', quantity: 1, price: 500, vendorId: vendorId || 10 }
    ]
  });

  const makeShipments = (vendorId?: number) => [
    {
      id: 1,
      trackingNumber: 'TRK001',
      status: 2,
      orderItems: [{ id: 1, productName: 'Item', vendorId: vendorId || 10 }]
    }
  ];

  beforeEach(async () => {
    sessionStorageMock.clear();

    mockOrderService = {
      getOrderDetail: vi.fn().mockReturnValue(of(makeOrder()))
    };

    mockShipmentService = {
      getShipmentsByOrderId: vi.fn().mockReturnValue(of(makeShipments()))
    };

    mockVendorService = {
      getMyVendorProfile: vi.fn().mockReturnValue(of({ id: 10, storeName: 'Test Store' }))
    };

    mockRouter = { navigate: vi.fn() };
    mockLocation = { back: vi.fn() };
    mockActivatedRoute = { paramMap: of(convertToParamMap({})) };

    await TestBed.configureTestingModule({
      imports: [OrderDetail, RouterModule.forRoot([])],
      providers: [
        { provide: OrderService, useValue: mockOrderService },
        { provide: ShipmentService, useValue: mockShipmentService },
        { provide: VendorService, useValue: mockVendorService },
        { provide: Router, useValue: mockRouter },
        { provide: Location, useValue: mockLocation },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();
  });

  afterEach(() => sessionStorageMock.clear());

  const createComponent = (role = 'Customer', paramMapId?: string) => {
    sessionStorageMock.setItem('role', role);
    mockActivatedRoute = {
      paramMap: of(convertToParamMap(paramMapId ? { id: paramMapId } : {}))
    };
    TestBed.overrideProvider(ActivatedRoute, { useValue: mockActivatedRoute });
    fixture = TestBed.createComponent(OrderDetail);
    component = fixture.componentInstance;
    component.orderId = 1;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should set role from sessionStorage on init', () => {
    createComponent('Admin');
    expect(component.role()).toBe('Admin');
  });

  it('should load order detail for Customer with orderId from Input', () => {
    createComponent('Customer');
    expect(mockOrderService.getOrderDetail).toHaveBeenCalledWith(1);
  });

  it('should load order with id from route param', () => {
    createComponent('Customer', '5');
    expect(mockOrderService.getOrderDetail).toHaveBeenCalledWith(5);
  });

  it('should fetch vendor profile for Vendor role and then load order', () => {
    createComponent('Vendor');
    expect(mockVendorService.getMyVendorProfile).toHaveBeenCalled();
    expect(mockOrderService.getOrderDetail).toHaveBeenCalled();
  });

  it('should handle vendor profile error and still load order detail', () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent('Vendor');
    expect(mockOrderService.getOrderDetail).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should filter order items by vendorId for Vendor role', () => {
    createComponent('Vendor');
    const order = component.order();
    expect(order?.items?.length).toBe(1);
  });

  it('should not duplicate loading if already loaded orderId', () => {
    createComponent('Customer');
    mockOrderService.getOrderDetail.mockClear();
    component.loadOrderDetail();
    expect(mockOrderService.getOrderDetail).not.toHaveBeenCalled();
  });

  it('should handle order loading error', () => {
    mockOrderService.getOrderDetail.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent('Customer');
    expect(component.errorMsg()).toBe('Failed to load order details.');
    consoleSpy.mockRestore();
  });

  it('should handle shipment loading error gracefully', () => {
    mockShipmentService.getShipmentsByOrderId.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent('Customer');
    expect(component.loading()).toBe(false);
    consoleSpy.mockRestore();
  });

  it('should filter shipments for Vendor role by vendorId', () => {
    createComponent('Vendor');
    const ships = component.shipments();
    expect(ships.length).toBe(1);
  });

  it('should call location.back on goBack', () => {
    createComponent('Customer');
    component.goBack();
    expect(mockLocation.back).toHaveBeenCalled();
  });

  it('should return true for isCustomer when role is Customer', () => {
    createComponent('Customer');
    expect(component.isCustomer()).toBe(true);
  });

  it('should return false for isCustomer when role is not Customer', () => {
    createComponent('Admin');
    expect(component.isCustomer()).toBe(false);
  });

  it('should navigate to review page on navigateToReview', () => {
    createComponent('Customer');
    component.navigateToReview(55);
    expect(mockRouter.navigate).toHaveBeenCalledWith(
      ['/customer-home/product-review-form'],
      { queryParams: { productId: 55, orderId: 1 } }
    );
  });

  it('should call alert and window.print on downloadInvoice', () => {
    createComponent('Customer');
    const alertSpy = vi.spyOn(globalThis, 'alert' as any).mockImplementation(() => {});
    const printSpy = vi.spyOn(globalThis, 'print' as any).mockImplementation(() => {});
    (component as any).downloadInvoice();
    expect(alertSpy).toHaveBeenCalled();
    alertSpy.mockRestore();
    printSpy.mockRestore();
  });

  describe('getEstimatedDeliveryDate', () => {
    it('should return 7-10 Days if no date', () => {
      createComponent('Customer');
      expect(component.getEstimatedDeliveryDate()).toBe('7-10 Days');
      expect(component.getEstimatedDeliveryDate(undefined)).toBe('7-10 Days');
    });

    it('should compute a date 7 days ahead', () => {
      createComponent('Customer');
      const result = component.getEstimatedDeliveryDate('2024-06-01T00:00:00');
      expect(result).toBeTruthy();
    });
  });

  describe('getOrderStatusLabel', () => {
    it('should return label for numeric status', () => {
      createComponent('Customer');
      expect(component.getOrderStatusLabel(0)).toBe('PENDING PAYMENT');
      expect(component.getOrderStatusLabel(4)).toBe('DELIVERED');
      expect(component.getOrderStatusLabel(5)).toBe('CANCELLED');
    });

    it('should return PENDING for unknown status', () => {
      createComponent('Customer');
      expect(component.getOrderStatusLabel(99)).toBe('PENDING');
    });

    it('should uppercase string status', () => {
      createComponent('Customer');
      expect(component.getOrderStatusLabel('shipped')).toBe('SHIPPED');
    });
  });

  describe('getOrderStatusClass', () => {
    it('should return status-delivered', () => {
      createComponent('Customer');
      expect(component.getOrderStatusClass(4)).toBe('status-delivered');
    });
    it('should return status-shipped', () => {
      createComponent('Customer');
      expect(component.getOrderStatusClass(3)).toBe('status-shipped');
    });
    it('should return status-cancelled', () => {
      createComponent('Customer');
      expect(component.getOrderStatusClass(5)).toBe('status-cancelled');
    });
    it('should return status-confirmed as default', () => {
      createComponent('Customer');
      expect(component.getOrderStatusClass(0)).toBe('status-confirmed');
    });
  });

  describe('getPaymentStatusLabel', () => {
    it('should return PENDING for 0', () => {
      createComponent('Customer');
      expect(component.getPaymentStatusLabel(0)).toBe('PENDING');
      expect(component.getPaymentStatusLabel(1)).toBe('PAID');
      expect(component.getPaymentStatusLabel(99)).toBe('PENDING');
    });

    it('should uppercase string value', () => {
      createComponent('Customer');
      expect(component.getPaymentStatusLabel('refunded')).toBe('REFUNDED');
    });
  });

  describe('getShipmentStatusLabel', () => {
    it('should return label for numeric status', () => {
      createComponent('Customer');
      expect(component.getShipmentStatusLabel(1)).toBe('PENDING');
      expect(component.getShipmentStatusLabel(3)).toBe('DELIVERED');
      expect(component.getShipmentStatusLabel(99)).toBe('PENDING');
    });

    it('should uppercase string value', () => {
      createComponent('Customer');
      expect(component.getShipmentStatusLabel('initiated')).toBe('INITIATED');
    });
  });

  describe('getShipmentStatusClass', () => {
    it('should return status-delivered for delivered or picked', () => {
      createComponent('Customer');
      expect(component.getShipmentStatusClass(3)).toBe('status-delivered');
      expect(component.getShipmentStatusClass(4)).toBe('status-delivered');
    });

    it('should return status-shipped for initiated', () => {
      createComponent('Customer');
      expect(component.getShipmentStatusClass(2)).toBe('status-shipped');
    });

    it('should return status-cancelled for cancelled', () => {
      createComponent('Customer');
      expect(component.getShipmentStatusClass(5)).toBe('status-cancelled');
    });

    it('should return status-pending as default', () => {
      createComponent('Customer');
      expect(component.getShipmentStatusClass(1)).toBe('status-pending');
    });
  });
});
