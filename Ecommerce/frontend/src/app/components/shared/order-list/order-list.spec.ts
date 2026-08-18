import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderList } from './order-list';
import { OrderService } from '../../../services/order.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
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

describe('OrderList', () => {
  let component: OrderList;
  let fixture: ComponentFixture<OrderList>;
  let mockOrderService: any;
  let mockRouter: any;

  const makePagedResponse = (items: any[] = [], totalCount = 0, totalPages = 1) => ({
    items,
    totalCount,
    pageNumber: 1,
    pageSize: 10,
    totalPages,
    hasNext: false,
    hasPrevious: false
  });

  const makeOrder = (overrides = {}) => ({
    id: 1,
    orderNumber: 'ORD-001',
    status: 0,
    paymentStatus: 1,
    totalAmount: 500,
    placedAt: new Date().toISOString(),
    items: [{ id: 1, productName: 'Item', quantity: 1, price: 500 }],
    ...overrides
  });

  beforeEach(async () => {
    sessionStorageMock.clear();

    mockOrderService = {
      getAllOrders: vi.fn().mockReturnValue(of(makePagedResponse([makeOrder()], 1, 1))),
      getVendorOrders: vi.fn().mockReturnValue(of(makePagedResponse([makeOrder({ id: 2 })], 1, 1))),
      getMyOrders: vi.fn().mockReturnValue(of(makePagedResponse([makeOrder({ id: 3 })], 1, 1)))
    };

    mockRouter = { navigate: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [OrderList, RouterModule.forRoot([])],
      providers: [
        { provide: OrderService, useValue: mockOrderService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: { data: of({}) } }
      ]
    }).compileComponents();
  });

  afterEach(() => sessionStorageMock.clear());

  const createComponent = () => {
    fixture = TestBed.createComponent(OrderList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should use role from sessionStorage if route data has no role', () => {
    sessionStorageMock.setItem('role', 'Admin');
    createComponent();
    expect(component.role).toBe('Admin');
    expect(mockOrderService.getAllOrders).toHaveBeenCalled();
  });

  it('should use Vendor role from sessionStorage', () => {
    sessionStorageMock.setItem('role', 'Vendor');
    createComponent();
    expect(component.role).toBe('Vendor');
    expect(mockOrderService.getVendorOrders).toHaveBeenCalled();
  });

  it('should use Customer role from sessionStorage', () => {
    sessionStorageMock.setItem('role', 'Customer');
    createComponent();
    expect(component.role).toBe('Customer');
    expect(mockOrderService.getMyOrders).toHaveBeenCalled();
  });

  it('should use role from route data when available', async () => {
    await TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [OrderList, RouterModule.forRoot([])],
      providers: [
        { provide: OrderService, useValue: mockOrderService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: { data: of({ role: 'Admin' }) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrderList);
    component = fixture.componentInstance;
    fixture.detectChanges();
    expect(component.role).toBe('Admin');
  });

  it('should load orders and set signals', () => {
    sessionStorageMock.setItem('role', 'Customer');
    createComponent();
    expect(component.orders().length).toBe(1);
    expect(component.totalCount()).toBe(1);
    expect(component.loading()).toBe(false);
  });

  it('should handle order load error and set errorMsg', () => {
    sessionStorageMock.setItem('role', 'Customer');
    mockOrderService.getMyOrders.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent();
    expect(component.errorMsg()).toBe('Failed to load orders.');
    expect(component.orders()).toEqual([]);
    consoleSpy.mockRestore();
  });

  describe('ngOnChanges', () => {
    it('should reload orders on role change', () => {
      sessionStorageMock.setItem('role', 'Customer');
      createComponent();
      component.ngOnChanges({ role: { currentValue: 'Customer', previousValue: 'Vendor', firstChange: false, isFirstChange: () => false } });
      expect(component.page()).toBe(1);
    });
  });

  describe('setPage', () => {
    it('should set page and reload orders for valid page number', () => {
      sessionStorageMock.setItem('role', 'Customer');
      createComponent();
      component.totalPages.set(3);
      mockOrderService.getMyOrders.mockClear();
      component.setPage(2);
      expect(component.page()).toBe(2);
      expect(mockOrderService.getMyOrders).toHaveBeenCalled();
    });

    it('should not change page for page 0', () => {
      sessionStorageMock.setItem('role', 'Customer');
      createComponent();
      component.totalPages.set(3);
      mockOrderService.getMyOrders.mockClear();
      component.setPage(0);
      expect(mockOrderService.getMyOrders).not.toHaveBeenCalled();
    });

    it('should not change page for page exceeding total', () => {
      sessionStorageMock.setItem('role', 'Customer');
      createComponent();
      component.totalPages.set(3);
      mockOrderService.getMyOrders.mockClear();
      component.setPage(99);
      expect(mockOrderService.getMyOrders).not.toHaveBeenCalled();
    });
  });

  describe('viewOrderDetail navigation', () => {
    it('should navigate to admin order-detail', () => {
      sessionStorageMock.setItem('role', 'Admin');
      createComponent();
      component.viewOrderDetail(10);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/admin-home/order-detail', 10]);
    });

    it('should navigate to vendor order-detail', () => {
      sessionStorageMock.setItem('role', 'Vendor');
      createComponent();
      component.viewOrderDetail(10);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/vendor-home/order-detail', 10]);
    });

    it('should navigate to customer order-detail', () => {
      sessionStorageMock.setItem('role', 'Customer');
      createComponent();
      component.viewOrderDetail(10);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/order-detail', 10]);
    });
  });

  describe('getOrderStatusLabel', () => {
    it('should return label for numeric status', () => {
      createComponent();
      expect(component.getOrderStatusLabel(0)).toBe('PENDING PAYMENT');
      expect(component.getOrderStatusLabel(2)).toBe('CONFIRMED');
      expect(component.getOrderStatusLabel(4)).toBe('DELIVERED');
      expect(component.getOrderStatusLabel(5)).toBe('CANCELLED');
    });

    it('should return PENDING for unknown numeric status', () => {
      createComponent();
      expect(component.getOrderStatusLabel(99)).toBe('PENDING');
    });

    it('should uppercase string status', () => {
      createComponent();
      expect(component.getOrderStatusLabel('confirmed')).toBe('CONFIRMED');
    });
  });

  describe('getOrderStatusClass', () => {
    it('should return status-delivered for delivered status', () => {
      createComponent();
      expect(component.getOrderStatusClass(4)).toBe('status-delivered');
    });

    it('should return status-shipped for shipped status', () => {
      createComponent();
      expect(component.getOrderStatusClass(3)).toBe('status-shipped');
    });

    it('should return status-cancelled for cancelled status', () => {
      createComponent();
      expect(component.getOrderStatusClass(5)).toBe('status-cancelled');
    });

    it('should return status-confirmed as default', () => {
      createComponent();
      expect(component.getOrderStatusClass(0)).toBe('status-confirmed');
    });
  });

  describe('getPaymentStatusLabel', () => {
    it('should return label for numeric payment status', () => {
      createComponent();
      expect(component.getPaymentStatusLabel(0)).toBe('PENDING');
      expect(component.getPaymentStatusLabel(1)).toBe('PAID');
      expect(component.getPaymentStatusLabel(2)).toBe('FAILED');
      expect(component.getPaymentStatusLabel(3)).toBe('REFUNDED');
    });

    it('should return PENDING for unknown numeric payment status', () => {
      createComponent();
      expect(component.getPaymentStatusLabel(99)).toBe('PENDING');
    });

    it('should uppercase string payment status', () => {
      createComponent();
      expect(component.getPaymentStatusLabel('paid')).toBe('PAID');
    });
  });

  describe('getPaymentStatusClass', () => {
    it('should return badge-active for paid status', () => {
      createComponent();
      expect(component.getPaymentStatusClass(1)).toBe('badge-active');
    });

    it('should return badge-draft for refunded status', () => {
      createComponent();
      expect(component.getPaymentStatusClass(3)).toBe('badge-draft');
    });

    it('should return status-cancelled for failed status', () => {
      createComponent();
      expect(component.getPaymentStatusClass(2)).toBe('status-cancelled');
    });

    it('should return status-confirmed as default', () => {
      createComponent();
      expect(component.getPaymentStatusClass(0)).toBe('status-confirmed');
    });
  });
});
