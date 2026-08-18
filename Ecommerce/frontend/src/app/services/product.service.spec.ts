import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProductService } from './product.service';

describe('ProductService', () => {
  let service: ProductService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ProductService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get catalog with filter properties', () => {
    const filter = {
      pageNumber: 2,
      pageSize: 10,
      categoryId: 5,
      sortBy: 'price',
      sortOrder: 'asc',
      searchQuery: 'shoes',
      minPrice: 50,
      maxPrice: 200
    };

    service.getCatalog(filter).subscribe();

    const req = httpTestingController.expectOne(req => 
      req.url === service['baseUrl'] &&
      req.params.get('pageNumber') === '2' &&
      req.params.get('pageSize') === '10' &&
      req.params.get('categoryId') === '5' &&
      req.params.get('sortBy') === 'price' &&
      req.params.get('sortOrder') === 'asc' &&
      req.params.get('searchQuery') === 'shoes' &&
      req.params.get('minPrice') === '50' &&
      req.params.get('maxPrice') === '200'
    );
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get catalog with default parameters when filter is empty', () => {
    service.getCatalog({}).subscribe();

    const req = httpTestingController.expectOne(req => 
      req.url === service['baseUrl'] &&
      req.params.get('sortBy') === 'newest' &&
      req.params.get('sortOrder') === 'desc' &&
      !req.params.has('pageNumber')
    );
    req.flush({});
  });

  it('should get catalog when no filter parameter is passed', () => {
    service.getCatalog().subscribe();

    const req = httpTestingController.expectOne(req => 
      req.url === service['baseUrl'] &&
      !req.params.has('sortBy')
    );
    req.flush({});
  });

  it('should search products', () => {
    service.search('test').subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/search?q=test`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get by ID', () => {
    service.getById(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/42`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should get vendor products (default params)', () => {
    service.getVendorProducts().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/vendor?pageNumber=1&pageSize=5`);
    req.flush({});
  });

  it('should get products by vendor ID', () => {
    service.getProductsByVendorId(99, 2, 10).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/vendor/99?pageNumber=2&pageSize=10`);
    req.flush({});
  });

  it('should create product', () => {
    const payload = { name: 'P' } as any;
    service.createProduct(payload).subscribe();
    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should update product', () => {
    const payload = { name: 'P' } as any;
    service.updateProduct(42, payload).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/42`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should publish product', () => {
    service.publishProduct(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/publish/42`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should toggle product status', () => {
    service.toggleProductStatus(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/toggle/42`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should add variant', () => {
    const payload = { price: 10 } as any;
    service.addVariant(10, payload).subscribe();
    const rootUrl = service['baseUrl'].replace('/Product', '');
    const req = httpTestingController.expectOne(`${rootUrl}/ProductVariant/10`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should update variant', () => {
    const payload = { price: 10 } as any;
    service.updateVariant(20, payload).subscribe();
    const rootUrl = service['baseUrl'].replace('/Product', '');
    const req = httpTestingController.expectOne(`${rootUrl}/ProductVariant/20`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should toggle variant status', () => {
    service.toggleVariantStatus(20).subscribe();
    const rootUrl = service['baseUrl'].replace('/Product', '');
    const req = httpTestingController.expectOne(`${rootUrl}/ProductVariant/20`);
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should add variant image', () => {
    const payload = { imageUrl: 'img' } as any;
    service.addVariantImage(20, payload).subscribe();
    const rootUrl = service['baseUrl'].replace('/Product', '');
    const req = httpTestingController.expectOne(`${rootUrl}/ProductVariant/image/20`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should delete variant image', () => {
    service.deleteVariantImage(99).subscribe();
    const rootUrl = service['baseUrl'].replace('/Product', '');
    const req = httpTestingController.expectOne(`${rootUrl}/ProductVariant/image/99`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
