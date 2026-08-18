import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DashboardService } from './admin-dashboard.service';

describe('DashboardService', () => {
  let service: DashboardService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DashboardService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get KPIs', () => {
    service.getKpis('2026-07').subscribe();

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/kpis` &&
      req.params.get('month') === '2026-07'
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get revenue breakdown', () => {
    service.getRevenueBreakdown().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/revenue-breakdown`);
    expect(req.request.method).toBe('GET');
    req.flush({ monthly: [] });
  });

  it('should get order status', () => {
    service.getOrderStatus().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/order-status`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get performance metrics', () => {
    service.getPerformanceMetrics('2026-07').subscribe();
    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/performance` &&
      req.params.get('month') === '2026-07'
    );
    req.flush({});
  });

  it('should get recent activity', () => {
    service.getRecentActivity().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/activity`);
    req.flush({});
  });

  it('should get discount distribution', () => {
    service.getDiscountDistribution().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/discount-distribution`);
    req.flush({});
  });

  it('should export report as blob', () => {
    const mockBlob = new Blob(['report data'], { type: 'application/pdf' });
    service.exportReport('2026-07').subscribe(res => {
      expect(res).toEqual(mockBlob);
    });

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/export` &&
      req.params.get('month') === '2026-07'
    );
    expect(req.request.responseType).toBe('blob');
    req.flush(mockBlob);
  });

  it('should mark all notifications as read', () => {
    service.markAllNotificationsRead().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/notifications/mark-all-read`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });
});
