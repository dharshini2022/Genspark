import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaymentStatus } from './payment-status';
import { ActivatedRoute, Router } from '@angular/router';
import { CartService } from '../../../services/cart.service';
import { OrderService } from '../../../services/order.service';
import { ToastService } from '../../../services/toast.service';
import { of, throwError, Observable } from 'rxjs';
import { signal } from '@angular/core';

describe('PaymentStatus', () => {
  let component: PaymentStatus;
  let fixture: ComponentFixture<PaymentStatus>;
  let mockActivatedRoute: any;
  let mockRouter: any;
  let mockCartService: any;
  let mockOrderService: any;
  let mockToastService: any;

  beforeEach(async () => {
    mockActivatedRoute = {
      snapshot: {
        queryParams: { redirect_status: 'succeeded', orderId: '100' }
      }
    };

    mockRouter = {
      navigate: vi.fn()
    };

    mockCartService = {
      cartCountSignal: {
        set: vi.fn()
      }
    };

    mockOrderService = {
      makePayment: vi.fn().mockReturnValue(of({}))
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [PaymentStatus],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: CartService, useValue: mockCartService },
        { provide: OrderService, useValue: mockOrderService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should process payment on succeed status', () => {
    fixture = TestBed.createComponent(PaymentStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isSuccess()).toBe(true);
    expect(mockOrderService.makePayment).toHaveBeenCalledWith(100, 'stripe_checkout');
    expect(mockCartService.cartCountSignal.set).toHaveBeenCalledWith(0);
    expect(mockToastService.success).toHaveBeenCalledWith('Payment confirmed! Your order is placed.');
    expect(component.isLoading()).toBe(false);

    // Should navigate after 3000ms
    vi.advanceTimersByTime(3000);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/order-list']);
  });

  it('should process payment on status success', () => {
    mockActivatedRoute.snapshot.queryParams = { status: 'success', orderId: '200' };
    fixture = TestBed.createComponent(PaymentStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isSuccess()).toBe(true);
    expect(mockOrderService.makePayment).toHaveBeenCalledWith(200, 'stripe_checkout');
    vi.advanceTimersByTime(3000);
  });

  it('should handle order payment api error', () => {
    mockOrderService.makePayment.mockReturnValue(throwError(() => ({ error: { message: 'Api Error' } })));
    fixture = TestBed.createComponent(PaymentStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isSuccess()).toBe(false);
    expect(component.errorMsg()).toBe('Api Error');
    expect(component.isLoading()).toBe(false);
  });

  it('should handle order payment api error with default fallback', () => {
    mockOrderService.makePayment.mockReturnValue(throwError(() => new Error('Unknown')));
    fixture = TestBed.createComponent(PaymentStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.errorMsg()).toBe('Failed to update order status.');
  });

  it('should handle non-success status', () => {
    mockActivatedRoute.snapshot.queryParams = { redirect_status: 'failed', orderId: '100' };
    fixture = TestBed.createComponent(PaymentStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isSuccess()).toBe(false);
    expect(component.errorMsg()).toBe('Payment was not completed. Status: failed');
    expect(component.isLoading()).toBe(false);
  });

  it('should handle missing orderId', () => {
    mockActivatedRoute.snapshot.queryParams = { redirect_status: 'succeeded' };
    fixture = TestBed.createComponent(PaymentStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isSuccess()).toBe(false);
  });

  it('should navigate to cart on calling goBack', () => {
    fixture = TestBed.createComponent(PaymentStatus);
    component = fixture.componentInstance;
    fixture.detectChanges();
    component.goBack();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/cart']);
  });

  describe('HTML Template rendering and actions', () => {
    it('should render loading spinner when isLoading is true', () => {
      // Mock the service call to never complete initially
      mockOrderService.makePayment.mockReturnValue(new Observable(() => {}));
      mockActivatedRoute.snapshot.queryParams = { redirect_status: 'succeeded', orderId: '100' };
      fixture = TestBed.createComponent(PaymentStatus);
      component = fixture.componentInstance;
      fixture.detectChanges();

      const spinner = fixture.debugElement.nativeElement.querySelector('.spinner-loader');
      expect(spinner).toBeTruthy();
    });

    it('should render success view and trigger goToOrders button click', () => {
      mockActivatedRoute.snapshot.queryParams = { redirect_status: 'succeeded', orderId: '100' };
      fixture = TestBed.createComponent(PaymentStatus);
      component = fixture.componentInstance;
      fixture.detectChanges();

      const ordersBtn = fixture.debugElement.nativeElement.querySelector('#payment-status-orders-btn');
      expect(ordersBtn).toBeTruthy();

      const spy = vi.spyOn(component, 'goToOrders');
      ordersBtn.click();
      expect(spy).toHaveBeenCalled();
    });

    it('should render failed view and trigger goBack button click', () => {
      mockActivatedRoute.snapshot.queryParams = { redirect_status: 'failed', orderId: '100' };
      fixture = TestBed.createComponent(PaymentStatus);
      component = fixture.componentInstance;
      fixture.detectChanges();

      const retryBtn = fixture.debugElement.nativeElement.querySelector('#payment-status-retry-btn');
      expect(retryBtn).toBeTruthy();

      const spy = vi.spyOn(component, 'goBack');
      retryBtn.click();
      expect(spy).toHaveBeenCalled();
    });
  });
});
