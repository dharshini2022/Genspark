import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { WishlistService } from './wishlist.service';

describe('WishlistService', () => {
  let service: WishlistService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        WishlistService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created and load initial wishlist count from sessionStorage', () => {
    sessionStorage.setItem('wishlist_count', '15');
    service = TestBed.inject(WishlistService);
    expect(service.wishlistCountSignal()).toBe(15);
  });

  it('should handle getWishlist API request', () => {
    service = TestBed.inject(WishlistService);
    const mockResponse: any = { items: [], totalItems: 2 };
    service.getWishlist().subscribe(res => {
      expect(res).toEqual(mockResponse);
    });

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should update wishlist count successfully', () => {
    service = TestBed.inject(WishlistService);
    const mockResponse: any = { items: [], totalItems: 4 };
    service.updateWishlistCount();

    const req = httpTestingController.expectOne(service['baseUrl']);
    req.flush(mockResponse);

    expect(service.wishlistCountSignal()).toBe(4);
  });

  it('should handle update wishlist count error', () => {
    service = TestBed.inject(WishlistService);
    service.wishlistCountSignal.set(10);
    service.updateWishlistCount();

    const req = httpTestingController.expectOne(service['baseUrl']);
    req.flush('error', { status: 500, statusText: 'Error' });

    expect(service.wishlistCountSignal()).toBe(0);
  });

  it('should add to wishlist', () => {
    service = TestBed.inject(WishlistService);
    const request = { variantId: 5 };
    service.addToWishlist(request).subscribe(res => {
      expect(res.message).toBe('Added');
    });

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({ message: 'Added', data: {} });
  });

  it('should remove from wishlist', () => {
    service = TestBed.inject(WishlistService);
    service.removeFromWishlist(15).subscribe(res => {
      expect(res.data).toBe(true);
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/15`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ message: 'Deleted', data: true });
  });

  it('should clear wishlist', () => {
    service = TestBed.inject(WishlistService);
    service.clearWishlist().subscribe(res => {
      expect(res.message).toBe('Cleared');
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/clear`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ message: 'Cleared' });
  });

  it('should sync changes to sessionStorage via effect', async () => {
    service = TestBed.inject(WishlistService);
    service.wishlistCountSignal.set(9);
    await new Promise(resolve => setTimeout(resolve, 0));
    expect(sessionStorage.getItem('wishlist_count')).toBe('9');
  });

  it('should sync wishlistCountSignal on storage events', () => {
    service = TestBed.inject(WishlistService);
    const storageEvent = new StorageEvent('storage', {
      key: 'wishlist_count',
      newValue: '18'
    });
    window.dispatchEvent(storageEvent);
    expect(service.wishlistCountSignal()).toBe(18);

    const storageEventEmpty = new StorageEvent('storage', {
      key: 'wishlist_count',
      newValue: null
    });
    window.dispatchEvent(storageEventEmpty);
    expect(service.wishlistCountSignal()).toBe(0);
  });
});
