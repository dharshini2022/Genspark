import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ReviewService } from './review.service';

describe('ReviewService', () => {
  let service: ReviewService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ReviewService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ReviewService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get product reviews', () => {
    service.getProductReviews(42).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/product/42`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should get my reviews', () => {
    service.getMyReviews().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/my`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should post review', () => {
    const payload = { rating: 5 } as any;
    service.postReview(payload).subscribe();
    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should put review', () => {
    const payload = { rating: 4 } as any;
    service.putReview(10, payload).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/10`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should delete review', () => {
    service.deleteReview(10).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/10`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true, message: 'Deleted' });
  });

  it('should get overall reviews', () => {
    service.getOverallReviews().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/overall`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
