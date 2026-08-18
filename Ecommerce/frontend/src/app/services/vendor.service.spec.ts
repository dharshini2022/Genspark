import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { VendorService } from './vendor.service';

describe('VendorService', () => {
  let service: VendorService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        VendorService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(VendorService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should register vendor', () => {
    const payload = { storeName: 'S' } as any;
    service.registerVendor(payload).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should get basic profile by ID', () => {
    service.getVendorBasicProfileById(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/searchBasicById/42`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get all vendors', () => {
    service.getAllVendors(2, 50).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/listVendors?pageNumber=2&pageSize=50`);
    req.flush({});
  });

  it('should get all vendors defaults', () => {
    service.getAllVendors().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/listVendors?pageNumber=1&pageSize=100`);
    req.flush({});
  });

  it('should get vendors by status', () => {
    service.getVendorsByStatus('Approved').subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/searchByStatus/Approved`);
    req.flush([]);
  });

  it('should approve vendor', () => {
    service.approveVendor(12).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/approve/12`);
    req.flush({});
  });

  it('should cancel vendor', () => {
    service.cancelVendor(12).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/cancel/12`);
    req.flush({});
  });

  it('should get my vendor profile', () => {
    service.getMyVendorProfile().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/profile`);
    req.flush({});
  });

  it('should toggle vendor status with vendorId', () => {
    service.toggleVendorStatus(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}?vendorId=42`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should toggle vendor status without vendorId', () => {
    service.toggleVendorStatus().subscribe();
    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should get profile by ID', () => {
    service.getVendorProfileById(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/searchById/42`);
    req.flush({});
  });

  it('should get admin revenue for vendor', () => {
    service.getAdminRevenueForVendor(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/admin-revenue/42`);
    req.flush({ revenue: 100 });
  });

  it('should get my settlements', () => {
    service.getMySettlements(2, 10).subscribe();
    const rootUrl = service['baseUrl'].replace('/Vendor', '');
    const req = httpTestingController.expectOne(`${rootUrl}/vendor/settlements?pageNumber=2&pageSize=10`);
    req.flush({});
  });

  it('should get my settlements defaults', () => {
    service.getMySettlements().subscribe();
    const rootUrl = service['baseUrl'].replace('/Vendor', '');
    const req = httpTestingController.expectOne(`${rootUrl}/vendor/settlements?pageNumber=1&pageSize=5`);
    req.flush({});
  });

  it('should get vendor settlements by admin', () => {
    service.getVendorSettlementsByAdmin(42, 2, 10).subscribe();
    const rootUrl = service['baseUrl'].replace('/Vendor', '');
    const req = httpTestingController.expectOne(`${rootUrl}/vendor/settlements/vendor/42?pageNumber=2&pageSize=10`);
    req.flush({});
  });

  it('should get vendor settlements by ID', () => {
    service.getVendorSettlementsById(42, 3, 7).subscribe();
    const rootUrl = service['baseUrl'].replace('/Vendor', '');
    const req = httpTestingController.expectOne(`${rootUrl}/vendor/settlements/vendor/42?pageNumber=3&pageSize=7`);
    req.flush({});
  });
});
