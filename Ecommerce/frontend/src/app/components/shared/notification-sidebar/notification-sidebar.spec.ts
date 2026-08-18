import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotificationSidebar } from './notification-sidebar';
import { NotificationService } from '../../../services/notification.service';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

const sessionStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem(key: string) { return store[key] || null; },
    setItem(key: string, value: string) { store[key] = value.toString(); },
    removeItem(key: string) { delete store[key]; },
    clear() { store = {}; }
  };
})();
Object.defineProperty(globalThis, 'sessionStorage', { value: sessionStorageMock, writable: true });

describe('NotificationSidebar', () => {
  let component: NotificationSidebar;
  let fixture: ComponentFixture<NotificationSidebar>;
  let mockNotificationService: any;

  const makeNotification = (overrides = {}) => ({
    id: 1,
    userId: 100,
    type: 'OrderPlaced',
    level: 'Info',
    title: 'Test',
    message: 'Test notification',
    isRead: false,
    createdAt: new Date().toISOString(),
    ...overrides
  });

  beforeEach(async () => {
    const notificationsSignal = signal<any[]>([]);
    const unreadCountSignal = signal<number>(0);

    mockNotificationService = {
      notifications: notificationsSignal,
      unreadCount: unreadCountSignal,
      markAsRead: vi.fn().mockReturnValue(of(null)),
      markAllAsRead: vi.fn().mockReturnValue(of(null))
    };

    await TestBed.configureTestingModule({
      imports: [NotificationSidebar],
      providers: [
        { provide: NotificationService, useValue: mockNotificationService }
      ]
    }).compileComponents();
  });

  const createComponent = (isOpen = false) => {
    fixture = TestBed.createComponent(NotificationSidebar);
    component = fixture.componentInstance;
    component.isOpen = isOpen;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should compute unread notifications from service', () => {
    createComponent();
    mockNotificationService.notifications.set([
      makeNotification({ id: 1, isRead: false }),
      makeNotification({ id: 2, isRead: true }),
    ]);
    fixture.detectChanges();
    expect(component.notifications().length).toBe(1);
    expect(component.notifications()[0].id).toBe(1);
  });

  it('should emit closeSidebar when onClose is called', () => {
    createComponent();
    const spy = vi.spyOn(component.closeSidebar, 'emit');
    component.onClose();
    expect(spy).toHaveBeenCalled();
  });

  it('should mark notification as read and update signals', () => {
    createComponent();
    const notification = makeNotification({ id: 10, isRead: false });
    mockNotificationService.notifications.set([notification]);

    component.onMarkAsRead(notification);

    expect(mockNotificationService.markAsRead).toHaveBeenCalledWith(10);
    fixture.detectChanges();

    const updated = mockNotificationService.notifications().find((n: any) => n.id === 10);
    expect(updated?.isRead).toBe(true);
  });

  it('should NOT call markAsRead if notification is already read', () => {
    createComponent();
    const notification = makeNotification({ id: 5, isRead: true });
    component.onMarkAsRead(notification);
    expect(mockNotificationService.markAsRead).not.toHaveBeenCalled();
  });

  it('should log error if markAsRead fails', () => {
    createComponent();
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockNotificationService.markAsRead.mockReturnValue(throwError(() => new Error('fail')));
    const notification = makeNotification({ id: 7, isRead: false });
    component.onMarkAsRead(notification);
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should mark all notifications as read on onMarkAllAsRead', () => {
    createComponent();
    mockNotificationService.notifications.set([
      makeNotification({ id: 1, isRead: false }),
      makeNotification({ id: 2, isRead: false }),
    ]);

    component.onMarkAllAsRead();

    expect(mockNotificationService.markAllAsRead).toHaveBeenCalled();
    const all = mockNotificationService.notifications();
    expect(all.every((n: any) => n.isRead)).toBe(true);
    expect(mockNotificationService.unreadCount()).toBe(0);
  });

  it('should log error if markAllAsRead fails', () => {
    createComponent();
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockNotificationService.markAllAsRead.mockReturnValue(throwError(() => new Error('fail')));
    component.onMarkAllAsRead();
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  describe('formatTime', () => {
    it('should return empty string for empty dateString', () => {
      createComponent();
      expect(component.formatTime('')).toBe('');
    });

    it('should return time string for today\'s date', () => {
      createComponent();
      const todayStr = new Date().toISOString();
      const result = component.formatTime(todayStr);
      expect(result).toBeTruthy();
      expect(typeof result).toBe('string');
    });

    it('should return non-empty string for a past date', () => {
      createComponent();
      const pastDate = new Date('2020-01-15T10:30:00').toISOString();
      const result = component.formatTime(pastDate);
      expect(result).toBeTruthy();
      expect(typeof result).toBe('string');
    });
  });

  it('should set isOpen input and reflect it on component', () => {
    createComponent(true);
    expect(component.isOpen).toBe(true);
  });
});
