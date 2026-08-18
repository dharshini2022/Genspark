import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CartService } from './cart.service';

describe('CartService', () => {
  let service: CartService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        CartService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created and load initial cart count from sessionStorage', () => {
    sessionStorage.setItem('cart_count', '8');
    service = TestBed.inject(CartService);
    expect(service.cartCountSignal()).toBe(8);
  });

  it('should handle getCart API request', () => {
    service = TestBed.inject(CartService);
    const mockCartResponse: any = { items: [], totalItems: 3 };
    service.getCart().subscribe(res => {
      expect(res).toEqual(mockCartResponse);
    });

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('GET');
    req.flush(mockCartResponse);
  });

  it('should update cart count successfully', () => {
    service = TestBed.inject(CartService);
    const mockCartResponse: any = { items: [], totalItems: 5 };
    service.updateCartCount();

    const req = httpTestingController.expectOne(service['baseUrl']);
    req.flush(mockCartResponse);

    expect(service.cartCountSignal()).toBe(5);
  });

  it('should handle update cart count error', () => {
    service = TestBed.inject(CartService);
    service.cartCountSignal.set(10);
    service.updateCartCount();

    const req = httpTestingController.expectOne(service['baseUrl']);
    req.flush('error', { status: 500, statusText: 'Server Error' });

    expect(service.cartCountSignal()).toBe(0);
  });

  it('should add to cart', () => {
    service = TestBed.inject(CartService);
    const request = { variantId: 2, quantity: 1 };
    service.addToCart(request).subscribe(res => {
      expect(res.message).toBe('Added');
    });

    const req = httpTestingController.expectOne(service['baseUrl']);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({ message: 'Added', data: {} });
  });

  it('should remove from cart', () => {
    service = TestBed.inject(CartService);
    service.removeFromCart(12).subscribe(res => {
      expect(res.data).toBe(true);
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/12`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ message: 'Deleted', data: true });
  });

  it('should update cart item quantity', () => {
    service = TestBed.inject(CartService);
    const request = { newQuantity: 5 };
    service.updateCartItemQuantity(12, request).subscribe(res => {
      expect(res.message).toBe('Updated');
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/12`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual(request);
    req.flush({ message: 'Updated' });
  });

  it('should clear cart', () => {
    service = TestBed.inject(CartService);
    service.clearCart().subscribe(res => {
      expect(res.message).toBe('Cleared');
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/clear`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ message: 'Cleared' });
  });

  it('should apply discount', () => {
    service = TestBed.inject(CartService);
    service.applyDiscount('CODE').subscribe(res => {
      expect(res).toEqual({ success: true });
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/apply-discount`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ discountCode: 'CODE' });
    req.flush({ success: true });
  });

  it('should remove discount', () => {
    service = TestBed.inject(CartService);
    service.removeDiscount().subscribe(res => {
      expect(res).toEqual({ success: true });
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/remove-discount`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });
  });

  it('should sync changes to sessionStorage via effect', async () => {
    service = TestBed.inject(CartService);
    service.cartCountSignal.set(12);
    // Wait for the effect to execute
    await new Promise(resolve => setTimeout(resolve, 0));
    expect(sessionStorage.getItem('cart_count')).toBe('12');
  });

  it('should sync cartCountSignal on storage events', () => {
    service = TestBed.inject(CartService);
    const storageEvent = new StorageEvent('storage', {
      key: 'cart_count',
      newValue: '22'
    });
    window.dispatchEvent(storageEvent);
    expect(service.cartCountSignal()).toBe(22);

    const storageEventEmpty = new StorageEvent('storage', {
      key: 'cart_count',
      newValue: null
    });
    window.dispatchEvent(storageEventEmpty);
    expect(service.cartCountSignal()).toBe(0);
  });
});
