import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { NotificationService } from './notification.service';
import * as signalR from '@microsoft/signalr';

const mockStart = vi.fn().mockImplementation(() => Promise.resolve());
const mockStop = vi.fn().mockImplementation(() => Promise.resolve());
const mockOn = vi.fn();

const mockConnection = {
  start: mockStart,
  stop: mockStop,
  on: mockOn,
  state: 'Disconnected'
};

const mockBuilderInstance = {
  withUrl: vi.fn().mockReturnThis(),
  withAutomaticReconnect: vi.fn().mockReturnThis(),
  build: vi.fn().mockReturnValue(mockConnection)
};


describe('NotificationService', () => {
  let service: NotificationService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    mockStart.mockClear().mockImplementation(() => Promise.resolve());
    mockStop.mockClear().mockImplementation(() => Promise.resolve());
    mockOn.mockClear();
    mockConnection.state = 'Disconnected';

    vi.spyOn(signalR.HubConnectionBuilder.prototype, 'withUrl').mockReturnThis();
    vi.spyOn(signalR.HubConnectionBuilder.prototype, 'withAutomaticReconnect').mockReturnThis();
    vi.spyOn(signalR.HubConnectionBuilder.prototype, 'build').mockReturnValue(mockConnection as any);


    TestBed.configureTestingModule({
      providers: [
        NotificationService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(NotificationService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load notifications', () => {
    const mockNotifications = [
      { id: 1, userId: 1, title: 'N1', message: 'Msg1', isRead: false, createdAt: '2026-07-09', type: '1', level: '1' },
      { id: 2, userId: 1, title: 'N2', message: 'Msg2', isRead: true, createdAt: '2026-07-09', type: '2', level: '2' }
    ];

    service.loadNotifications();

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('GET');
    req.flush(mockNotifications);

    expect(service.notifications()).toEqual(mockNotifications);
    expect(service.unreadCount()).toBe(1);
  });

  it('should handle loadNotifications error gracefully', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    service.loadNotifications();

    const req = httpTestingController.expectOne(service['baseUrl']);
    req.flush('error', { status: 500, statusText: 'Err' });

    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should mark all notifications as read', () => {
    service.markAllAsRead().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/mark-all-read`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should mark a specific notification as read', () => {
    service.markAsRead(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/42/read`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  describe('SignalR connection handling', () => {
    it('should not start connection if user is missing in sessionStorage', () => {
      service.startConnection();
      expect(mockStart).not.toHaveBeenCalled();
    });

    it('should not start connection if already connected', () => {
      sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
      mockConnection.state = 'Connected';
      // Injects existing connection
      service['hubConnection'] = mockConnection as any;

      service.startConnection();
      expect(mockStart).not.toHaveBeenCalled();
    });

    it('should start connection successfully and register ReceiveNotification event', async () => {
      sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});

      service.startConnection();
      expect(mockStart).toHaveBeenCalled();

      // Flush start Promise resolution
      await new Promise(resolve => setTimeout(resolve, 0));
      expect(consoleSpy).toHaveBeenCalledWith('SignalR connected to NotificationHub');
      expect(mockOn).toHaveBeenCalledWith('ReceiveNotification', expect.any(Function));

      // Verify withCredentials is sent in connection builder
      const withUrlSpy = vi.spyOn(signalR.HubConnectionBuilder.prototype, 'withUrl');
      const urlArgs = withUrlSpy.mock.calls[0];
      const options = urlArgs[1];
      expect(options?.withCredentials).toBe(true);

      // Trigger the registered ReceiveNotification callback
      const callback = mockOn.mock.calls[0][1];
      const newNotification = { id: 3, userId: 1, title: 'N3', message: 'Msg3', isRead: false, createdAt: '2026', type: '3', level: '3' };
      service.notifications.set([]);
      service.unreadCount.set(0);

      callback(newNotification);

      expect(service.notifications()).toEqual([newNotification]);
      expect(service.unreadCount()).toBe(1);

      consoleSpy.mockRestore();
    });

    it('should log error when start fails', async () => {
      sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
      mockStart.mockImplementation(() => Promise.reject('Connection Fail'));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      service.startConnection();
      await new Promise(resolve => setTimeout(resolve, 0));
      expect(consoleSpy).toHaveBeenCalledWith('SignalR connection error: ', 'Connection Fail');
      consoleSpy.mockRestore();
    });

    it('should stop connection and clear hubConnection reference', async () => {
      service['hubConnection'] = mockConnection as any;
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});

      service.stopConnection();
      expect(mockStop).toHaveBeenCalled();
      await new Promise(resolve => setTimeout(resolve, 0));
      expect(consoleSpy).toHaveBeenCalledWith('SignalR connection stopped');
      expect(service['hubConnection']).toBeUndefined();
      consoleSpy.mockRestore();
    });

    it('should handle stop connection error gracefully', async () => {
      service['hubConnection'] = mockConnection as any;
      mockStop.mockImplementation(() => Promise.reject('Stop Fail'));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      service.stopConnection();
      await new Promise(resolve => setTimeout(resolve, 0));
      expect(consoleSpy).toHaveBeenCalledWith('SignalR stop error: ', 'Stop Fail');
      consoleSpy.mockRestore();
    });

    it('should do nothing on stopConnection if hubConnection is not present', () => {
      service['hubConnection'] = undefined;
      service.stopConnection();
      expect(mockStop).not.toHaveBeenCalled();
    });
  });
});
