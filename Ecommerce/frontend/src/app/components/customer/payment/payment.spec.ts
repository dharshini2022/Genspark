import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Payment } from './payment';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService } from '../../../services/order.service';
import { ToastService } from '../../../services/toast.service';
import { of, throwError } from 'rxjs';

describe('Payment', () => {
  let component: Payment;
  let fixture: ComponentFixture<Payment>;
  let mockActivatedRoute: any;
  let mockRouter: any;
  let mockOrderService: any;
  let mockToastService: any;
  let originalLocation: any;

  beforeEach(async () => {
    mockActivatedRoute = {
      snapshot: {
        queryParams: { orderId: '123', amount: '2500' }
      }
    };

    mockRouter = {
      navigate: vi.fn()
    };

    mockOrderService = {
      createCheckoutSession: vi.fn().mockReturnValue(of({ url: 'http://stripe.checkout.url' }))
    };

    mockToastService = {
      error: vi.fn()
    };

    // Save original location and mock window.location
    originalLocation = window.location;
    // We use Object.defineProperty because window.location is read-only
    const mockLocation = { href: '' };
    Object.defineProperty(window, 'location', {
      value: mockLocation,
      writable: true,
      configurable: true
    });

    await TestBed.configureTestingModule({
      imports: [Payment],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: OrderService, useValue: mockOrderService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    // Restore original window.location
    Object.defineProperty(window, 'location', {
      value: originalLocation,
      configurable: true
    });
  });

  it('should create and automatically redirect to stripe', () => {
    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component).toBeTruthy();
    expect(component.orderId()).toBe(123);
    expect(component.amount()).toBe(2500);
    expect(mockOrderService.createCheckoutSession).toHaveBeenCalledWith(123);
    expect(window.location.href).toBe('http://stripe.checkout.url');
  });

  it('should warn and redirect to cart if orderId is missing', () => {
    mockActivatedRoute.snapshot.queryParams = { amount: '2500' };
    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(mockToastService.error).toHaveBeenCalledWith('Invalid payment session. Redirecting back…');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/cart']);
  });

  it('should warn and redirect to cart if orderId is NaN', () => {
    mockActivatedRoute.snapshot.queryParams = { orderId: 'not-a-number' };
    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(mockToastService.error).toHaveBeenCalledWith('Invalid payment session. Redirecting back…');
  });

  it('should handle stripe checkout session creation error', () => {
    mockOrderService.createCheckoutSession.mockReturnValue(throwError(() => ({ error: { message: 'Failed to create session' } })));
    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.isLoading()).toBe(false);
    expect(component.errorMessage()).toBe('Failed to create session');
  });

  it('should handle stripe checkout session creation error with fallback message', () => {
    mockOrderService.createCheckoutSession.mockReturnValue(throwError(() => new Error('Unknown')));
    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.errorMessage()).toBe('Could not start payment. Please try again.');
  });

  it('should go back when goBack is called', () => {
    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();
    component.goBack();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/order-checkout']);
  });

  it('should format currency correctly', () => {
    fixture = TestBed.createComponent(Payment);
    component = fixture.componentInstance;
    fixture.detectChanges();
    
    expect(component.formatCurrency(null)).toBe('—');
    
    const formatted = component.formatCurrency(123.45);
    // INR formatting contains currency symbol and non-breaking space / space/ digits.
    expect(formatted).toContain('123.45');
  });

  describe('HTML Template rendering and actions', () => {
    it('should render error view and trigger action buttons on click', () => {
      mockOrderService.createCheckoutSession.mockReturnValue(throwError(() => new Error('Err')));
      fixture = TestBed.createComponent(Payment);
      component = fixture.componentInstance;
      fixture.detectChanges();

      const retryBtn = fixture.debugElement.nativeElement.querySelector('#payment-retry-btn');
      expect(retryBtn).toBeTruthy();
      
      const spyRedirect = vi.spyOn(component, 'redirectToStripe');
      retryBtn.click();
      expect(spyRedirect).toHaveBeenCalledWith(123);

      const backBtn = fixture.debugElement.nativeElement.querySelector('#payment-back-btn');
      expect(backBtn).toBeTruthy();

      const spyGoBack = vi.spyOn(component, 'goBack');
      backBtn.click();
      expect(spyGoBack).toHaveBeenCalled();
    });
  });
});
