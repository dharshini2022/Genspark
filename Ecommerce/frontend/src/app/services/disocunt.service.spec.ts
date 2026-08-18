import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DiscountService } from './disocunt.service';

describe('DiscountService', () => {
  let service: DiscountService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DiscountService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DiscountService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get active discounts with query params', () => {
    const mockRes: any = { items: [], totalCount: 0 };
    service.getActiveDiscounts({ pageNumber: 2, pageSize: 10, searchTerm: 'SALE' }).subscribe(res => {
      expect(res).toEqual(mockRes);
    });

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/active` &&
      req.params.get('pageNumber') === '2' &&
      req.params.get('pageSize') === '10' &&
      req.params.get('searchTerm') === 'SALE'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockRes);
  });

  it('should get active discounts without query params', () => {
    service.getActiveDiscounts().subscribe();
    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/active` &&
      !req.params.has('pageNumber')
    );
    req.flush({});
  });

  it('should get my vendor discounts', () => {
    service.getMyVendorDiscounts(2, 8).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/vendor?pageNumber=2&pageSize=8`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get my vendor discounts default params', () => {
    service.getMyVendorDiscounts().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/vendor?pageNumber=1&pageSize=5`);
    req.flush({});
  });

  it('should get discount history and construct PageResponse (when items count < pageSize)', () => {
    const mockList: any[] = [
      { id: 1, code: 'C1' },
      { id: 2, code: 'C2' }
    ];
    service.getDiscountHisotry({ pageNumber: 2, pageSize: 5 }).subscribe(res => {
      expect(res.items).toEqual(mockList);
      expect(res.pageNumber).toBe(2);
      expect(res.pageSize).toBe(5);
      expect(res.totalCount).toBe(7); // (2-1)*5 + 2 = 7
      expect(res.totalPages).toBe(2);
      expect(res.hasNext).toBe(false);
      expect(res.hasPrevious).toBe(true);
    });

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/all` &&
      req.params.get('pageNumber') === '2' &&
      req.params.get('pageSize') === '5'
    );
    req.flush(mockList);
  });

  it('should get discount history and construct PageResponse (when items count >= pageSize)', () => {
    const mockList: any[] = [
      { id: 1 }, { id: 2 }, { id: 3 }, { id: 4 }, { id: 5 }
    ];
    service.getDiscountHisotry({ pageNumber: 1, pageSize: 5, searchTerm: 'SALE' }).subscribe(res => {
      expect(res.totalCount).toBe(6); // 1*5 + 1 = 6
      expect(res.hasNext).toBe(true);
      expect(res.hasPrevious).toBe(false);
    });

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/all` &&
      req.params.get('searchTerm') === 'SALE'
    );
    req.flush(mockList);
  });

  it('should get discount history with default request parameters', () => {
    service.getDiscountHisotry().subscribe(res => {
      expect(res.pageNumber).toBe(1);
      expect(res.pageSize).toBe(5);
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/all`);
    req.flush([]);
  });

  it('should get vendor discounts by admin', () => {
    service.getVendorDiscountsByAdmin(101, 3, 6).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/vendor/101?pageNumber=3&pageSize=6`);
    req.flush({});
  });

  it('should create discount', () => {
    const payload = { code: 'SALE10' } as any;
    service.createDiscount(payload).subscribe(res => {
      expect(res.message).toBe('Created');
    });

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ message: 'Created' });
  });

  it('should deactivate discount', () => {
    service.deactivateDiscount('SALE10').subscribe(res => {
      expect(res.message).toBe('Deactivated');
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/deactivate/SALE10`);
    expect(req.request.method).toBe('PATCH');
    req.flush({ message: 'Deactivated' });
  });

  it('should evaluate cart discounts', () => {
    const reqPayload: any = { subTotal: 100, items: [] };
    service.evaluateCartDiscounts(reqPayload).subscribe();

    const req = httpTestingController.expectOne(`${service['baseUrl']}/evaluate`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(reqPayload);
    req.flush([]);
  });

  it('should get applicable locked discounts', () => {
    const reqPayload: any = { subTotal: 100, items: [] };
    service.getApplicableLockedDiscounts(reqPayload).subscribe();

    const req = httpTestingController.expectOne(`${service['baseUrl']}/applicable-locked`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(reqPayload);
    req.flush([]);
  });

  it('should get discounts of product', () => {
    service.getDiscountsOfProduct(1, 2, 3).subscribe();

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/product` &&
      req.params.get('productId') === '1' &&
      req.params.get('categoryId') === '2' &&
      req.params.get('vendorId') === '3'
    );
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
