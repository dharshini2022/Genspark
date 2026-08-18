import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PerformanceMetricsComponent } from './performance-metrics';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { Component, signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { PerformanceMetrics } from '../../../../../models/dashboard.model';

@Component({
  template: `<app-performance-metrics [month]="month()"></app-performance-metrics>`,
  imports: [PerformanceMetricsComponent]
})
class TestHostComponent {
  month = signal<string>('2026-06');
}

describe('PerformanceMetricsComponent', () => {
  let hostFixture: ComponentFixture<TestHostComponent>;
  let hostComponent: TestHostComponent;
  let mockDashboardService: any;
  let dataSubject: Subject<PerformanceMetrics>;

  const mockPerformance = {
    topSellingProducts: [],
    categoryPerformance: [
      { category: 'Electronics', orders: 80, percentage: 40 }
    ],
    vendorPerformance: [
      { rank: 1, name: 'Apple Store', revenue: 3000, percentage: 60 }
    ]
  };

  beforeEach(async () => {
    dataSubject = new Subject<PerformanceMetrics>();
    mockDashboardService = {
      getPerformanceMetrics: vi.fn().mockReturnValue(dataSubject)
    };

    await TestBed.configureTestingModule({
      imports: [TestHostComponent, PerformanceMetricsComponent],
      providers: [
        { provide: DashboardService, useValue: mockDashboardService }
      ]
    }).compileComponents();

    hostFixture = TestBed.createComponent(TestHostComponent);
    hostComponent = hostFixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should render loading skeleton while loading', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    const childComponent = hostFixture.debugElement.children[0].componentInstance as PerformanceMetricsComponent;
    expect(childComponent.loading()).toBe(true);

    const skeleton = hostFixture.debugElement.query(By.css('.skeleton-row-cat'));
    expect(skeleton).toBeTruthy();
  });

  it('should render performance data when loaded successfully', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    dataSubject.next(mockPerformance);
    hostFixture.detectChanges();

    const childComponent = hostFixture.debugElement.children[0].componentInstance as PerformanceMetricsComponent;
    expect(childComponent.loading()).toBe(false);
    expect(childComponent.categoryPerformance()).toEqual(mockPerformance.categoryPerformance);
    expect(childComponent.vendorPerformance()).toEqual(mockPerformance.vendorPerformance);

    const catItems = hostFixture.debugElement.queryAll(By.css('ul.category-list li'));
    expect(catItems.length).toBe(1);
    expect(catItems[0].nativeElement.textContent).toContain('Electronics');

    const vendorItems = hostFixture.debugElement.queryAll(By.css('ul.vendor-list li'));
    expect(vendorItems.length).toBe(1);
    expect(vendorItems[0].nativeElement.textContent).toContain('Apple Store');
  });

  it('should render error and retry on API failure', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    dataSubject.error(new Error('API failure'));
    hostFixture.detectChanges();

    const childComponent = hostFixture.debugElement.children[0].componentInstance as PerformanceMetricsComponent;
    expect(childComponent.error()).toBe('Failed to load performance metrics.');

    const errorContainer = hostFixture.debugElement.query(By.css('.error-container span'));
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.nativeElement.textContent).toContain('Failed to load performance metrics.');

    // Retry
    const newSubject = new Subject<PerformanceMetrics>();
    mockDashboardService.getPerformanceMetrics.mockReturnValue(newSubject);

    const retryBtn = hostFixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();

    expect(mockDashboardService.getPerformanceMetrics).toHaveBeenCalledWith('2026-06');
    consoleSpy.mockRestore();
  });
});
