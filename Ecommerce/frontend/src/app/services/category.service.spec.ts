import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CategoryService } from './category.service';

describe('CategoryService', () => {
  let service: CategoryService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CategoryService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(CategoryService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get categories list', () => {
    const mockCategories = [{ id: 1, name: 'Electronics', parentId: null }];
    service.getCategories().subscribe(res => {
      expect(res).toEqual(mockCategories);
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/list`);
    expect(req.request.method).toBe('GET');
    req.flush(mockCategories);
  });

  it('should get category tree', () => {
    const mockTree = [{ id: 1, name: 'Electronics', parentId: null, children: [] }];
    service.getCategoryTree().subscribe(res => {
      expect(res).toEqual(mockTree);
    });

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('GET');
    req.flush(mockTree);
  });

  it('should create a category', () => {
    const requestPayload = { name: 'Books', slug: 'books' };
    service.createCategory(requestPayload).subscribe(res => {
      expect(res).toEqual({ id: 5 });
    });

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(requestPayload);
    req.flush({ id: 5 });
  });

  it('should update a category', () => {
    const requestPayload = { name: 'Books Updated' };
    service.updateCategory(10, requestPayload).subscribe(res => {
      expect(res).toEqual({ success: true });
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/10`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(requestPayload);
    req.flush({ success: true });
  });

  it('should delete a category', () => {
    service.deleteCategory(10).subscribe(res => {
      expect(res).toEqual({ success: true });
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/10`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true });
  });
});
