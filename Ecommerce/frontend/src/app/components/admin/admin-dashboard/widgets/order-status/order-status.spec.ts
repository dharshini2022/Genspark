import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OrderStatusComponent } from './order-status';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { OrderStatus } from '../../../../../models/dashboard.model';
import { By } from '@angular/platform-browser';

describe('OrderStatusComponent', () => {
  let component: OrderStatusComponent;
  let fixture: ComponentFixture<OrderStatusComponent>;
  let mockDashboardService: any;
  let dataSubject: Subject<OrderStatus>;

  const mockOrderStatus: OrderStatus = {
    totalOrders: 100,
    slices: [
      { label: 'Delivered', percentage: 70, color: '#10B981' },
      { label: 'Shipped', percentage: 20, color: '#3B82F6' },
      { label: 'Cancelled', percentage: 10, color: '#EF4444' }
    ]
  };

  beforeEach(async () => {
    dataSubject = new Subject<OrderStatus>();
    mockDashboardService = {
      getOrderStatus: vi.fn().mockReturnValue(dataSubject)
    };

    await TestBed.configureTestingModule({
      imports: [OrderStatusComponent],
      providers: [
        { provide: DashboardService, useValue: mockDashboardService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrderStatusComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should render loading skeleton while loading', () => {
    fixture.detectChanges();
    expect(component.loading()).toBe(true);

    const skeleton = fixture.debugElement.query(By.css('.skeleton-donut'));
    expect(skeleton).toBeTruthy();
  });

  it('should render data and chart when loaded successfully', () => {
    fixture.detectChanges();
    dataSubject.next(mockOrderStatus);
    fixture.detectChanges();

    expect(component.loading()).toBe(false);
    expect(component.orderStatus()).toEqual(mockOrderStatus);

    const totalCount = fixture.debugElement.query(By.css('.donut-center strong'));
    expect(totalCount.nativeElement.textContent).toContain('0.1k'); // 100 / 1000 = 0.1k

    const sliceLabels = fixture.debugElement.queryAll(By.css('ul.legend li'));
    expect(sliceLabels[0].nativeElement.textContent).toContain('Delivered');
    expect(sliceLabels[1].nativeElement.textContent).toContain('Shipped');
  });

  it('should render error and retry on API failure', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    dataSubject.error(new Error('API failure'));
    fixture.detectChanges();

    expect(component.error()).toBe('Failed to load order status data.');
    const errorText = fixture.debugElement.query(By.css('.error-container span'));
    expect(errorText.nativeElement.textContent).toContain('Failed to load order status data.');

    // Retry
    const newSubject = new Subject<OrderStatus>();
    mockDashboardService.getOrderStatus.mockReturnValue(newSubject);
    
    const retryBtn = fixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();
    expect(mockDashboardService.getOrderStatus).toHaveBeenCalledTimes(2);

    consoleSpy.mockRestore();
  });

  describe('donutGradient', () => {
    it('should return fallback gray color if slices are empty or null', () => {
      expect(component.donutGradient([])).toBe('#E5E7EB');
      expect(component.donutGradient(null as any)).toBe('#E5E7EB');
    });

    it('should generate conic-gradient string based on slices', () => {
      const gradient = component.donutGradient(mockOrderStatus.slices);
      expect(gradient).toBe('conic-gradient(#10B981 0% 70%, #3B82F6 70% 90%, #EF4444 90% 100%)');
    });
  });
});
