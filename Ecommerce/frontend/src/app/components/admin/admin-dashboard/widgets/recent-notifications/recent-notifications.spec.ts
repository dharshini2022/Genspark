import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RecentNotificationsComponent } from './recent-notifications';
import { DashboardService } from '../../../../../services/admin-dashboard.service';
import { of, throwError, Subject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { RecentNotification, RecentActivity } from '../../../../../models/dashboard.model';
import { By } from '@angular/platform-browser';

describe('RecentNotificationsComponent', () => {
  let component: RecentNotificationsComponent;
  let fixture: ComponentFixture<RecentNotificationsComponent>;
  let mockDashboardService: any;
  let dataSubject: Subject<RecentActivity>;

  const mockActivity = {
    recentNotifications: [
      { id: 1, message: 'New vendor registered', type: 'info' as const, notifiedAt: '2026-06-01', timeAgo: '2h', read: false },
      { id: 2, message: 'Low stock warning', type: 'warning' as const, notifiedAt: '2026-06-02', timeAgo: '1h', read: false }
    ],
    recentOrders: []
  };

  beforeEach(async () => {
    dataSubject = new Subject<RecentActivity>();
    mockDashboardService = {
      getRecentActivity: vi.fn().mockReturnValue(dataSubject),
      markAllNotificationsRead: vi.fn().mockReturnValue(of(undefined))
    };

    await TestBed.configureTestingModule({
      imports: [RecentNotificationsComponent],
      providers: [
        { provide: DashboardService, useValue: mockDashboardService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RecentNotificationsComponent);
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

  it('should render notifications when loaded successfully', () => {
    fixture.detectChanges();
    dataSubject.next(mockActivity);
    fixture.detectChanges();

    expect(component.loading()).toBe(false);
    expect(component.recentNotifications()).toEqual(mockActivity.recentNotifications);

    const items = fixture.debugElement.queryAll(By.css('ul.notification-list li'));
    expect(items.length).toBe(2);
    expect(items[0].nativeElement.textContent).toContain('New vendor registered');
  });

  it('should render error and retry on API failure', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    dataSubject.error(new Error('API failure'));
    fixture.detectChanges();

    expect(component.error()).toBe('Failed to load notifications.');
    const errorContainer = fixture.debugElement.query(By.css('.error-container span'));
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.nativeElement.textContent).toContain('Failed to load notifications.');

    // Retry
    const newSubject = new Subject<RecentActivity>();
    mockDashboardService.getRecentActivity.mockReturnValue(newSubject);

    const retryBtn = fixture.debugElement.query(By.css('.retry-btn'));
    retryBtn.nativeElement.click();
    expect(mockDashboardService.getRecentActivity).toHaveBeenCalledTimes(2);

    consoleSpy.mockRestore();
  });

  describe('notificationDotClass', () => {
    it('should return correct class for different notification types', () => {
      expect(component.notificationDotClass('info')).toBe('dot dot-info');
      expect(component.notificationDotClass('warning')).toBe('dot dot-warning');
      expect(component.notificationDotClass('success')).toBe('dot dot-success');
      expect(component.notificationDotClass('error')).toBe('dot dot-error');
      expect(component.notificationDotClass('unknown')).toBe('dot dot-info');
      expect(component.notificationDotClass(null as any)).toBe('dot dot-info');
    });
  });

  describe('markAllNotificationsRead', () => {
    it('should call service and refetch notifications on success', () => {
      fixture.detectChanges();
      mockDashboardService.getRecentActivity.mockClear();

      component.markAllNotificationsRead();

      expect(mockDashboardService.markAllNotificationsRead).toHaveBeenCalled();
      expect(mockDashboardService.getRecentActivity).toHaveBeenCalled();
    });

    it('should log error when markAllNotificationsRead fails', () => {
      fixture.detectChanges();
      mockDashboardService.markAllNotificationsRead.mockReturnValue(throwError(() => new Error('Mark read error')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.markAllNotificationsRead();

      expect(consoleSpy).toHaveBeenCalled();
    });
  });
});
