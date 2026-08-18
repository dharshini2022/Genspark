import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RecentOrdersComponent } from './recent-orders';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { RecentOrder, RecentActivity } from '../../../../../models/dashboard.model';
import { By } from '@angular/platform-browser';

describe('RecentOrdersComponent', () => {
  let component: RecentOrdersComponent;
  let fixture: ComponentFixture<RecentOrdersComponent>;
  let mockDashboardService: any;
  let dataSubject: Subject<RecentActivity>;

  const mockActivity = {
    recentNotifications: [],
    recentOrders: [
      { id: 101, customerName: 'John Doe', amount: 150.0, status: 'DELIVERED' as const },
      { id: 102, customerName: 'Jane Smith', amount: 80.0, status: 'SHIPPED' as const }
    ]
  };

  beforeEach(async () => {
    dataSubject = new Subject<RecentActivity>();
    mockDashboardService = {
      getRecentActivity: vi.fn().mockReturnValue(dataSubject)
    };

    await TestBed.configureTestingModule({
      imports: [RecentOrdersComponent],
      providers: [
        { provide: DashboardService, useValue: mockDashboardService },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RecentOrdersComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should render loading skeleton while loading', () => {
    fixture.detectChanges();
    expect(component.loading()).toBe(true);

    const skeleton = fixture.debugElement.query(By.css('.skeleton-row'));
    expect(skeleton).toBeTruthy();
  });

  it('should render orders when loaded successfully', () => {
    fixture.detectChanges();
    dataSubject.next(mockActivity);
    fixture.detectChanges();

    expect(component.loading()).toBe(false);
    expect(component.recentOrders()).toEqual(mockActivity.recentOrders);

    const tableRows = fixture.debugElement.queryAll(By.css('ul.order-list li'));
    expect(tableRows.length).toBe(2);
    expect(tableRows[0].nativeElement.textContent).toContain('John Doe');
  });

  it('should render error and retry on API failure', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    dataSubject.error(new Error('API failure'));
    fixture.detectChanges();

    expect(component.error()).toBe('Failed to load recent orders.');
    const errorContainer = fixture.debugElement.query(By.css('.error-container span'));
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.nativeElement.textContent).toContain('Failed to load recent orders.');

    // Retry
    const newSubject = new Subject<RecentActivity>();
    mockDashboardService.getRecentActivity.mockReturnValue(newSubject);

    const retryBtn = fixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();
    expect(mockDashboardService.getRecentActivity).toHaveBeenCalledTimes(2);

    consoleSpy.mockRestore();
  });

  describe('statusClass', () => {
    it('should return correct badge class for statuses', () => {
      expect(component.statusClass('delivered')).toBe('status-badge status-delivered');
      expect(component.statusClass('shipped')).toBe('status-badge status-shipped');
      expect(component.statusClass('confirmed')).toBe('status-badge status-confirmed');
      expect(component.statusClass('cancelled')).toBe('status-badge status-cancelled');
      expect(component.statusClass('unknown')).toBe('status-badge status-confirmed');
      expect(component.statusClass(null as any)).toBe('status-badge status-confirmed');
    });
  });
});
