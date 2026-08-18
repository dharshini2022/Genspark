import { ComponentFixture, TestBed } from '@angular/core/testing';
import { KpiGridComponent } from './kpi-grid';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { Component, signal } from '@angular/core';
import { DashboardStats } from '../../../../../models/dashboard.model';
import { By } from '@angular/platform-browser';

@Component({
  template: `<app-kpi-grid [month]="month()"></app-kpi-grid>`,
  imports: [KpiGridComponent]
})
class TestHostComponent {
  month = signal<string>('2026-06');
}

describe('KpiGridComponent', () => {
  let hostFixture: ComponentFixture<TestHostComponent>;
  let hostComponent: TestHostComponent;
  let mockDashboardService: any;
  let kpiSubject: Subject<DashboardStats>;

  const mockKpis: DashboardStats = {
    totalRevenue: { value: 5000, changePercent: 5.5, changeDirection: 'up' },
    totalOrders: { value: 100, changePercent: 2.1, changeDirection: 'down' },
    activeProducts: { value: 80, changePercent: 1.2, changeDirection: 'up' },
    activeVendors: { value: 10, changePercent: 0, changeDirection: 'up' }
  };

  beforeEach(async () => {
    kpiSubject = new Subject<DashboardStats>();
    mockDashboardService = {
      getKpis: vi.fn().mockReturnValue(kpiSubject)
    };

    await TestBed.configureTestingModule({
      imports: [TestHostComponent, KpiGridComponent],
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

    const childComponent = hostFixture.debugElement.children[0].componentInstance as KpiGridComponent;
    expect(childComponent.loading()).toBe(true);

    const skeletonCards = hostFixture.debugElement.queryAll(By.css('.stat-card.skeleton'));
    expect(skeletonCards.length).toBe(4);
  });

  it('should render stats-grid when data load is successful', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    kpiSubject.next(mockKpis);
    hostFixture.detectChanges();

    const childComponent = hostFixture.debugElement.children[0].componentInstance as KpiGridComponent;
    expect(childComponent.loading()).toBe(false);
    expect(childComponent.kpis()).toEqual(mockKpis);

    const values = hostFixture.debugElement.queryAll(By.css('.stat-value'));
    expect(values[0].nativeElement.textContent).toContain('Rs. 5,000.00');
    expect(values[1].nativeElement.textContent).toContain('100');
  });

  it('should render error message and support retry on API failure', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    kpiSubject.error({ error: { message: 'Network Failure' } });
    hostFixture.detectChanges();

    const childComponent = hostFixture.debugElement.children[0].componentInstance as KpiGridComponent;
    expect(childComponent.error()).toBe('Network Failure');

    const errorContainer = hostFixture.debugElement.query(By.css('.error-container'));
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.nativeElement.textContent).toContain('Network Failure');

    // Trigger retry
    mockDashboardService.getKpis.mockClear();
    const newKpiSubject = new Subject<DashboardStats>();
    mockDashboardService.getKpis.mockReturnValue(newKpiSubject);

    const retryBtn = hostFixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();
    hostFixture.detectChanges();

    expect(mockDashboardService.getKpis).toHaveBeenCalledWith('2026-06');
    consoleSpy.mockRestore();
  });
});
