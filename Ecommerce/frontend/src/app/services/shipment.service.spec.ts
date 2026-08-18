import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ShipmentService } from './shipment.service';

describe('ShipmentService', () => {
  let service: ShipmentService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ShipmentService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ShipmentService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get vendor shipments', () => {
    service.getVendorShipments().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/vendor-shipments`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get vendor shipments for admin', () => {
    service.getVendorShipmentsForAdmin(10).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/all?vendorId=10`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get shipments by order ID', () => {
    service.getShipmentsByOrderId(100).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/order/100`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
