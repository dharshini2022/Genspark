import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DiscountDistributionComponent } from './discount-distribution';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { DiscountDistribution } from '../../../../../models/dashboard.model';
import { By } from '@angular/platform-browser';

describe('DiscountDistributionComponent', () => {
  let component: DiscountDistributionComponent;
  let fixture: ComponentFixture<DiscountDistributionComponent>;
  let mockDashboardService: any;
  let dataSubject: Subject<DiscountDistribution>;

  const mockDiscountData: DiscountDistribution = {
    totalTypes: 15,
    slices: [
      { label: '10% Off', percentage: 33.3, color: '#FF0000' },
      { label: '20% Off', percentage: 66.7, color: '#00FF00' }
    ]
  };

  beforeEach(async () => {
    dataSubject = new Subject<DiscountDistribution>();
    mockDashboardService = {
      getDiscountDistribution: vi.fn().mockReturnValue(dataSubject)
    };

    await TestBed.configureTestingModule({
      imports: [DiscountDistributionComponent],
      providers: [
        { provide: DashboardService, useValue: mockDashboardService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DiscountDistributionComponent);
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
    dataSubject.next(mockDiscountData);
    fixture.detectChanges();

    expect(component.loading()).toBe(false);
    expect(component.discountDistribution()).toEqual(mockDiscountData);

    const totalCount = fixture.debugElement.query(By.css('.donut-center strong'));
    expect(totalCount.nativeElement.textContent).toContain('15');

    const sliceLabels = fixture.debugElement.queryAll(By.css('ul.legend li'));
    expect(sliceLabels[0].nativeElement.textContent).toContain('10% Off');
    expect(sliceLabels[1].nativeElement.textContent).toContain('20% Off');
  });

  it('should render error and retry on API failure', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    dataSubject.error(new Error('API failure'));
    fixture.detectChanges();

    expect(component.error()).toBe('Failed to load discount data.');
    const errorText = fixture.debugElement.query(By.css('.error-container span'));
    expect(errorText.nativeElement.textContent).toContain('Failed to load discount data.');

    // Retry
    const newSubject = new Subject<DiscountDistribution>();
    mockDashboardService.getDiscountDistribution.mockReturnValue(newSubject);
    
    const retryBtn = fixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();
    expect(mockDashboardService.getDiscountDistribution).toHaveBeenCalledTimes(2);

    consoleSpy.mockRestore();
  });

  describe('donutGradient', () => {
    it('should return fallback gray color if slices are empty or null', () => {
      expect(component.donutGradient([])).toBe('#E5E7EB');
      expect(component.donutGradient(null as any)).toBe('#E5E7EB');
    });

    it('should generate conic-gradient string based on slices', () => {
      const gradient = component.donutGradient(mockDiscountData.slices);
      expect(gradient).toBe('conic-gradient(#FF0000 0% 33.3%, #00FF00 33.3% 100%)');
    });
  });
});
