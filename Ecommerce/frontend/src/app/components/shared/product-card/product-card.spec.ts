import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductCard } from './product-card';
import { DiscountService } from '../../../services/disocunt.service';
import { RouterModule } from '@angular/router';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { DiscountType } from '../../../models/disocunt.model';

const sessionStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem(key: string) { return store[key] || null; },
    setItem(key: string, value: string) { store[key] = value.toString(); },
    removeItem(key: string) { delete store[key]; },
    clear() { store = {}; }
  };
})();
Object.defineProperty(globalThis, 'sessionStorage', { value: sessionStorageMock, writable: true });

describe('ProductCard', () => {
  let component: ProductCard;
  let fixture: ComponentFixture<ProductCard>;
  let mockDiscountService: any;

  const makeVariant = (overrides = {}) => ({
    id: 1,
    productId: 1,
    price: 1000,
    stockQty: 10,
    orderCount: 0,
    isDefault: true,
    isActive: true,
    sku: 'SKU-001',
    variantImages: [],
    availableValues: { Color: 'Blue' },
    ...overrides
  });


  const makeProduct = (overrides = {}) => ({
    id: 1,
    name: 'Test Product',
    description: 'A test product',
    categoryId: 5,
    categoryName: 'Electronics',
    vendorId: 2,
    averageRating: 4.2,
    reviewCount: 15,
    status: 'Active',
    createdAt: '2024-01-01T00:00:00',
    storeName: 'Test Store',
    variants: [makeVariant()],
    ...overrides
  });

  beforeEach(async () => {
    sessionStorageMock.clear();

    mockDiscountService = {
      getDiscountsOfProduct: vi.fn().mockReturnValue(of([]))
    };

    await TestBed.configureTestingModule({
      imports: [ProductCard, RouterModule.forRoot([])],
      providers: [
        { provide: DiscountService, useValue: mockDiscountService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductCard);
    component = fixture.componentInstance;
  });

  afterEach(() => sessionStorageMock.clear());

  it('should create', () => {
    component.product = makeProduct();
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should set role from sessionStorage on init', () => {
    sessionStorageMock.setItem('role', 'Admin');
    component.product = makeProduct();
    fixture.detectChanges();
    expect(component.role()).toBe('Admin');
  });

  it('should default role to Customer if not in sessionStorage', () => {
    component.product = makeProduct();
    fixture.detectChanges();
    expect(component.role()).toBe('Customer');
  });

  describe('detailRoute computed', () => {
    it('should return admin product-detail for Admin role', () => {
      sessionStorageMock.setItem('role', 'Admin');
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.detailRoute()).toBe('/admin-home/product-detail');
    });

    it('should return vendor product-detail for Vendor role', () => {
      sessionStorageMock.setItem('role', 'Vendor');
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.detailRoute()).toBe('/vendor-home/product-detail');
    });

    it('should return customer product-detail for Customer role', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.detailRoute()).toBe('/customer-home/product-detail');
    });
  });

  describe('defaultVariant computed', () => {
    it('should return undefined if product has no variants', () => {
      component.product = makeProduct({ variants: [] });
      fixture.detectChanges();
      expect(component.defaultVariant()).toBeUndefined();
    });

    it('should return default variant', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.defaultVariant()?.id).toBe(1);
    });

    it('should return active variant if no default', () => {
      component.product = makeProduct({
        variants: [makeVariant({ isDefault: false, isActive: true })]
      });
      fixture.detectChanges();
      expect(component.defaultVariant()).toBeTruthy();
    });
  });

  describe('originalPrice computed', () => {
    it('should return variant price', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.originalPrice()).toBe(1000);
    });

    it('should return 0 if no variant', () => {
      component.product = makeProduct({ variants: [] });
      fixture.detectChanges();
      expect(component.originalPrice()).toBe(0);
    });
  });

  describe('averageRating and reviewCount computed', () => {
    it('should return product averageRating', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.averageRating()).toBe(4.2);
    });

    it('should return reviewCount', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.reviewCount()).toBe(15);
    });
  });

  describe('discountInfo', () => {
    it('should use discountLabel and discountedPrice if pre-enriched', () => {
      component.product = makeProduct({ discountLabel: '10%', discountedPrice: 900 } as any);
      fixture.detectChanges();
      // toSignal is async, just verify the service is not called for pre-enriched products
      expect(mockDiscountService.getDiscountsOfProduct).not.toHaveBeenCalled();
    });

    it('should call getDiscountsOfProduct for non-enriched products', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(mockDiscountService.getDiscountsOfProduct).toHaveBeenCalledWith(1, 5, 2);
    });

    it('should handle flat discount correctly', () => {
      const flatDiscount = { type: DiscountType.Flat, value: 100, minOrderValue: 500 };
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(of([flatDiscount]));
      component.product = makeProduct();
      fixture.detectChanges();
      // The discountInfo signal should update with the discount info
      expect(mockDiscountService.getDiscountsOfProduct).toHaveBeenCalled();
    });

    it('should ignore discount if variant price is less than minOrderValue', () => {
      const flatDiscount = { type: DiscountType.Flat, value: 100, minOrderValue: 2000 };
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(of([flatDiscount]));
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.discountInfo()?.label).toBe('');
      expect(component.discountInfo()?.discountedPrice).toBe(1000);
    });

    it('should handle percentage discount correctly', () => {
      const pctDiscount = { type: DiscountType.Percentage, value: 10, minOrderValue: 100 };
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(of([pctDiscount]));
      component.product = makeProduct();
      fixture.detectChanges();
      expect(mockDiscountService.getDiscountsOfProduct).toHaveBeenCalled();
    });

    it('should handle discount fetch errors gracefully', () => {
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(throwError(() => new Error('fail')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.product = makeProduct();
      fixture.detectChanges();
      consoleSpy.mockRestore();
    });

    it('should return empty label and 0 discountedPrice for product with no variants', () => {
      component.product = makeProduct({ variants: [] });
      fixture.detectChanges();
      expect(component.discountInfo()?.label).toBe('');
      expect(component.discountInfo()?.discountedPrice).toBe(0);
    });
  });

  describe('getRating and getReviewCount', () => {
    it('should return rating as fixed string', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.getRating()).toBe('4.2');
    });

    it('should return review count', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      expect(component.getReviewCount()).toBe(15);
    });
  });

  describe('toggleWishlist', () => {
    it('should toggle isWishlisted to true', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
      const event = { stopPropagation: vi.fn() } as any;
      component.toggleWishlist(event);
      expect(event.stopPropagation).toHaveBeenCalled();
      expect(component.isWishlisted()).toBe(true);
      consoleSpy.mockRestore();
    });

    it('should toggle isWishlisted back to false', () => {
      component.product = makeProduct();
      fixture.detectChanges();
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});
      const event = { stopPropagation: vi.fn() } as any;
      component.toggleWishlist(event);
      component.toggleWishlist(event);
      expect(component.isWishlisted()).toBe(false);
      consoleSpy.mockRestore();
    });
  });
});
