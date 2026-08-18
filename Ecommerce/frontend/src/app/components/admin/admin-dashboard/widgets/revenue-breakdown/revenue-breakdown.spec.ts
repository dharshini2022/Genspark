import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RevenueBreakdownComponent } from './revenue-breakdown';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { By } from '@angular/platform-browser';

describe('RevenueBreakdownComponent', () => {
  let component: RevenueBreakdownComponent;
  let fixture: ComponentFixture<RevenueBreakdownComponent>;
  let mockDashboardService: any;
  let dataSubject: Subject<{ monthly: any[] }>;

  const mockRevenueData = {
    monthly: [
      { label: 'Jan', revenue: 1000 },
      { label: 'Feb', revenue: 2500 },
      { label: 'Mar', revenue: 500 }
    ]
  };

  beforeEach(async () => {
    dataSubject = new Subject<{ monthly: any[] }>();
    mockDashboardService = {
      getRevenueBreakdown: vi.fn().mockReturnValue(dataSubject)
    };

    await TestBed.configureTestingModule({
      imports: [RevenueBreakdownComponent],
      providers: [
        { provide: DashboardService, useValue: mockDashboardService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RevenueBreakdownComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should render loading skeleton while loading', () => {
    fixture.detectChanges();
    expect(component.loading()).toBe(true);

    const skeleton = fixture.debugElement.query(By.css('.skeleton-chart'));
    expect(skeleton).toBeTruthy();
  });

  it('should render chart when loaded successfully', () => {
    fixture.detectChanges();
    dataSubject.next(mockRevenueData);
    fixture.detectChanges();

    expect(component.loading()).toBe(false);
    expect(component.revenueBreakdown()).toEqual(mockRevenueData);

    const chartBars = fixture.debugElement.queryAll(By.css('.bar-col'));
    expect(chartBars.length).toBe(3);
  });

  it('should render error and retry on API failure', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    dataSubject.error(new Error('API failure'));
    fixture.detectChanges();

    expect(component.error()).toBe('Failed to load revenue data.');
    const errorContainer = fixture.debugElement.query(By.css('.error-container span'));
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.nativeElement.textContent).toContain('Failed to load revenue data.');

    // Retry
    const newSubject = new Subject<{ monthly: any[] }>();
    mockDashboardService.getRevenueBreakdown.mockReturnValue(newSubject);

    const retryBtn = fixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();
    expect(mockDashboardService.getRevenueBreakdown).toHaveBeenCalledTimes(2);

    consoleSpy.mockRestore();
  });

  it('should correctly calculate computed revenueChartData signal', () => {
    fixture.detectChanges();
    dataSubject.next(mockRevenueData);
    fixture.detectChanges();

    const chartData = component.revenueChartData();
    expect(chartData).toEqual([
      { label: 'Jan', value: 1000 },
      { label: 'Feb', value: 2500 },
      { label: 'Mar', value: 500 }
    ]);
  });

  it('should return empty list for revenueChartData when data is null or empty', () => {
    component.revenueBreakdown.set(null);
    expect(component.revenueChartData()).toEqual([]);

    component.revenueBreakdown.set({ monthly: null as any });
    expect(component.revenueChartData()).toEqual([]);
  });

  describe('barHeightPercent', () => {
    it('should return 0 when chart data is empty', () => {
      component.revenueBreakdown.set(null);
      expect(component.barHeightPercent(500)).toBe(0);
    });

    it('should return 0 when max value in chart data is 0', () => {
      component.revenueBreakdown.set({
        monthly: [
          { label: 'Jan', revenue: 0 },
          { label: 'Feb', revenue: 0 }
        ]
      });
      expect(component.barHeightPercent(0)).toBe(0);
    });

    it('should calculate relative height ratio correctly', () => {
      fixture.detectChanges();
      dataSubject.next(mockRevenueData);
      fixture.detectChanges(); // Jan: 1000, Feb: 2500, Mar: 500 (max: 2500)
      
      // 2500 / 2500 * 100 = 100
      expect(component.barHeightPercent(2500)).toBe(100);
      
      // 1000 / 2500 * 100 = 40
      expect(component.barHeightPercent(1000)).toBe(40);
      
      // 5 / 2500 * 100 = 0.2% -> should clamp to min 5%
      expect(component.barHeightPercent(5)).toBe(5);

      // 0 should return 0
      expect(component.barHeightPercent(0)).toBe(0);
    });
  });
});
