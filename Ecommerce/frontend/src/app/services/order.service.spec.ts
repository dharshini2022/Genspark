import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { OrderService } from './order.service';

describe('OrderService', () => {
  let service: OrderService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        OrderService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(OrderService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get vendor orders', () => {
    service.getVendorOrders(1, 10, 'query').subscribe();

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/vendor-orders` &&
      req.params.get('pageNumber') === '1' &&
      req.params.get('pageSize') === '10' &&
      req.params.get('searchTerm') === 'query'
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get vendor orders by ID', () => {
    service.getVendorOrdersById(101).subscribe();

    const req = httpTestingController.expectOne(`${service['baseUrl']}/vendor-orders/101`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get my orders', () => {
    service.getMyOrders().subscribe();

    const req = httpTestingController.expectOne(`${service['baseUrl']}/my-orders`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get all orders', () => {
    service.getAllOrders(3, 5).subscribe();

    const req = httpTestingController.expectOne(req => 
      req.url === `${service['baseUrl']}/all` &&
      req.params.get('pageNumber') === '3' &&
      req.params.get('pageSize') === '5'
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get order detail', () => {
    service.getOrderDetail(123).subscribe();

    const req = httpTestingController.expectOne(`${service['baseUrl']}/123`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should place order', () => {
    const payload = { userAddressId: 22, discountCode: 'SALE10' };
    service.placeOrder(payload).subscribe();

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should make payment', () => {
    service.makePayment(123, 'pm_card_visa').subscribe();

    const req = httpTestingController.expectOne(`${TestBed.inject(OrderService)['baseUrl'].replace('/Order', '')}/Payment/pay/123`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ paymentMethodId: 'pm_card_visa' });
    req.flush({});
  });

  it('should create checkout session', () => {
    service.createCheckoutSession(123).subscribe();

    const req = httpTestingController.expectOne(`${TestBed.inject(OrderService)['baseUrl'].replace('/Order', '')}/Payment/checkout-session/123`);
    expect(req.request.method).toBe('POST');
    req.flush({ url: 'http://url' });
  });
});
