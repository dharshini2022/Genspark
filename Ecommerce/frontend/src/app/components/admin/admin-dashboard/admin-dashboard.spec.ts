import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminDashboard } from './admin-dashboard';
import { DashboardService } from '../../../services/admin-dashboard.service';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { DashboardStats, OrderStatus, DiscountDistribution, RecentActivity, PerformanceMetrics } from '../../../models/dashboard.model';

describe('AdminDashboard', () => {
  let component: AdminDashboard;
  let fixture: ComponentFixture<AdminDashboard>;
  let mockDashboardService: any;

  const mockKpis: DashboardStats = {
    totalRevenue: { value: 5000, changePercent: 5.5, changeDirection: 'up' },
    totalOrders: { value: 100, changePercent: 2.1, changeDirection: 'down' },
    activeProducts: { value: 80, changePercent: 1.2, changeDirection: 'up' },
    activeVendors: { value: 10, changePercent: 0, changeDirection: 'up' }
  };

  const mockOrderStatus: OrderStatus = {
    totalOrders: 100,
    slices: [{ label: 'Delivered', percentage: 100, color: '#000' }]
  };

  const mockDiscountData: DiscountDistribution = {
    totalTypes: 5,
    slices: [{ label: '10% Off', percentage: 100, color: '#000' }]
  };

  const mockActivity: RecentActivity = {
    recentNotifications: [],
    recentOrders: []
  };

  const mockPerformance: PerformanceMetrics = {
    topSellingProducts: [],
    categoryPerformance: [],
    vendorPerformance: []
  };

  beforeEach(async () => {
    mockDashboardService = {
      exportReport: vi.fn().mockReturnValue(of(new Blob(['test-data'], { type: 'text/csv' }))),
      getKpis: vi.fn().mockReturnValue(of(mockKpis)),
      getRevenueBreakdown: vi.fn().mockReturnValue(of({ monthly: [] })),
      getOrderStatus: vi.fn().mockReturnValue(of(mockOrderStatus)),
      getDiscountDistribution: vi.fn().mockReturnValue(of(mockDiscountData)),
      getRecentActivity: vi.fn().mockReturnValue(of(mockActivity)),
      getPerformanceMetrics: vi.fn().mockReturnValue(of(mockPerformance))
    };

    await TestBed.configureTestingModule({
      imports: [AdminDashboard],
      providers: [
        { provide: DashboardService, useValue: mockDashboardService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should initialize selectedMonth to the current year and month', () => {
    fixture.detectChanges();
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = String(now.getMonth() + 1).padStart(2, '0');
    expect(component.selectedMonth()).toBe(`${currentYear}-${currentMonth}`);
  });

  it('should update selectedMonth when onMonthChange is called', () => {
    fixture.detectChanges();
    const dummyEvent = {
      target: {
        value: '2026-08'
      }
    } as unknown as Event;

    component.onMonthChange(dummyEvent);
    expect(component.selectedMonth()).toBe('2026-08');
  });

  it('should not update selectedMonth when onMonthChange event has no target or value', () => {
    fixture.detectChanges();
    component.selectedMonth.set('2026-07');
    
    component.onMonthChange({ target: null } as unknown as Event);
    expect(component.selectedMonth()).toBe('2026-07');

    component.onMonthChange({ target: {} } as unknown as Event);
    expect(component.selectedMonth()).toBe('2026-07');
  });

  it('should download report successfully on exportReport', () => {
    fixture.detectChanges();
    const createObjectURLSpy = vi.spyOn(URL, 'createObjectURL').mockReturnValue('mock-url');
    const revokeObjectURLSpy = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {});
    
    const realCreateElement = document.createElement.bind(document);
    const dummyAnchor = realCreateElement('a');
    const clickSpy = vi.spyOn(dummyAnchor, 'click').mockImplementation(() => {});
    
    vi.spyOn(document, 'createElement').mockImplementation((tagName: string) => {
      if (tagName === 'a') return dummyAnchor;
      return realCreateElement(tagName);
    });

    const appendChildSpy = vi.spyOn(document.body, 'appendChild');
    const removeChildSpy = vi.spyOn(document.body, 'removeChild');

    component.selectedMonth.set('2026-07');
    component.exportReport();

    expect(mockDashboardService.exportReport).toHaveBeenCalledWith('2026-07');
    expect(createObjectURLSpy).toHaveBeenCalled();
    expect(appendChildSpy).toHaveBeenCalledWith(dummyAnchor);
    expect(clickSpy).toHaveBeenCalled();
    expect(removeChildSpy).toHaveBeenCalledWith(dummyAnchor);
    expect(revokeObjectURLSpy).toHaveBeenCalledWith('mock-url');
  });

  it('should handle exportReport failure', () => {
    fixture.detectChanges();
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    const alertSpy = vi.spyOn(window, 'alert').mockImplementation(() => {});
    mockDashboardService.exportReport.mockReturnValue(throwError(() => new Error('API failure')));

    component.selectedMonth.set('2026-07');
    component.exportReport();

    expect(consoleSpy).toHaveBeenCalled();
    expect(alertSpy).toHaveBeenCalledWith('Failed to export report.');
  });
});
