import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TopProductsComponent } from './top-products';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { Component, signal } from '@angular/core';
import { PerformanceMetrics } from '../../../../../models/dashboard.model';
import { By } from '@angular/platform-browser';

@Component({
  template: `<app-top-products [month]="month()"></app-top-products>`,
  imports: [TopProductsComponent]
})
class TestHostComponent {
  month = signal<string>('2026-06');
}

describe('TopProductsComponent', () => {
  let hostFixture: ComponentFixture<TestHostComponent>;
  let hostComponent: TestHostComponent;
  let mockDashboardService: any;
  let dataSubject: Subject<PerformanceMetrics>;

  const mockPerformance = {
    topSellingProducts: [
      { rank: 1, name: 'iPhone 15', unitsSold: 50, revenue: 50000.0, category: 'Electronics' },
      { rank: 2, name: 'MacBook Air', unitsSold: 20, revenue: 24000.0, category: 'Electronics' }
    ],
    categoryPerformance: [],
    vendorPerformance: []
  };

  beforeEach(async () => {
    dataSubject = new Subject<PerformanceMetrics>();
    mockDashboardService = {
      getPerformanceMetrics: vi.fn().mockReturnValue(dataSubject)
    };

    await TestBed.configureTestingModule({
      imports: [TestHostComponent, TopProductsComponent],
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

    const childComponent = hostFixture.debugElement.children[0].componentInstance as TopProductsComponent;
    expect(childComponent.loading()).toBe(true);

    const skeleton = hostFixture.debugElement.query(By.css('.skeleton-row'));
    expect(skeleton).toBeTruthy();
  });

  it('should render top products when loaded successfully', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    dataSubject.next(mockPerformance);
    hostFixture.detectChanges();

    const childComponent = hostFixture.debugElement.children[0].componentInstance as TopProductsComponent;
    expect(childComponent.loading()).toBe(false);
    expect(childComponent.topProducts()).toEqual(mockPerformance.topSellingProducts);

    const listItems = hostFixture.debugElement.queryAll(By.css('ul.product-list li'));
    expect(listItems.length).toBe(2);
    expect(listItems[0].nativeElement.textContent).toContain('iPhone 15');
  });

  it('should render error and retry on API failure', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    dataSubject.error(new Error('API failure'));
    hostFixture.detectChanges();

    const childComponent = hostFixture.debugElement.children[0].componentInstance as TopProductsComponent;
    expect(childComponent.error()).toBe('Failed to load top selling products.');

    const errorContainer = hostFixture.debugElement.query(By.css('.error-container span'));
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.nativeElement.textContent).toContain('Failed to load top selling products.');

    // Retry
    const newSubject = new Subject<PerformanceMetrics>();
    mockDashboardService.getPerformanceMetrics.mockReturnValue(newSubject);

    const retryBtn = hostFixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();

    expect(mockDashboardService.getPerformanceMetrics).toHaveBeenCalledWith('2026-06');
    consoleSpy.mockRestore();
  });
});
