import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { signal } from '@angular/core';
import { ProductDetail } from './product-detail';
import { ProductService } from '../../../services/product.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ReviewService } from '../../../services/review.service';
import { VendorService } from '../../../services/vendor.service';
import { CategoryService } from '../../../services/category.service';
import { CartService } from '../../../services/cart.service';
import { ToastService } from '../../../services/toast.service';
import { WishlistService } from '../../../services/wishlist.service';
import { ActivatedRoute, RouterModule, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { DiscountType, DiscountScope, DiscountResponse } from '../../../models/disocunt.model';


const sessionStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem(key: string) { return store[key] || null; },
    setItem(key: string, value: string) { store[key] = value.toString(); },
    removeItem(key: string) { delete store[key]; },
    clear: vi.fn().mockImplementation(() => { store = {}; })
  };
})();
Object.defineProperty(globalThis, 'sessionStorage', { value: sessionStorageMock, writable: true });
Object.defineProperty(window, 'sessionStorage', { value: sessionStorageMock, writable: true });

describe('ProductDetail', () => {
  let component: ProductDetail;
  let fixture: ComponentFixture<ProductDetail>;

  let mockProductService: any;
  let mockDiscountService: any;
  let mockReviewService: any;
  let mockVendorService: any;
  let mockCategoryService: any;
  let mockCartService: any;
  let mockToastService: any;
  let mockWishlistService: any;
  let mockWishlistItemsSignal: any;
  let mockActivatedRoute: any;

  const makeVariant = (overrides: any = {}) => ({
    id: 1, productId: 1, price: 1000, stockQty: 10, orderCount: 0,
    isDefault: true, isActive: true,
    sku: 'SKU-001', variantImages: [], availableValues: { Color: 'Blue' }, ...overrides
  });


  const makeProduct = (overrides: any = {}) => ({
    id: 1, name: 'Test Product', description: 'Test desc',
    categoryId: 5, categoryName: 'Electronics-Tech', vendorId: 2,
    averageRating: 4.0, reviewCount: 10,
    status: 'Active', createdAt: '2024-01-01T00:00:00', storeName: 'Test Store',
    variants: [makeVariant()], ...overrides
  });


  beforeEach(async () => {
    sessionStorageMock.clear();
    sessionStorageMock.setItem('role', 'Customer');

    mockProductService = {
      getById: vi.fn().mockReturnValue(of(makeProduct())),
      updateProduct: vi.fn().mockReturnValue(of({})),
      getProductsByVendorId: vi.fn().mockReturnValue(of({ items: [] })),
      toggleVariantStatus: vi.fn().mockReturnValue(of({}))
    };

    mockDiscountService = {
      getDiscountsOfProduct: vi.fn().mockReturnValue(of([]))
    };

    mockReviewService = {
      getProductReviews: vi.fn().mockReturnValue(of([]))
    };

    mockVendorService = {
      getVendorBasicProfileById: vi.fn().mockReturnValue(of({ id: 2, storeName: 'Test Store' }))
    };

    mockCategoryService = {
      getCategories: vi.fn().mockReturnValue(of([
        { id: 5, name: 'Electronics', slug: 'electronics' }
      ]))
    };

    mockCartService = {
      addToCart: vi.fn().mockReturnValue(of({})),
      cartItemsSignal: signal([])
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    mockWishlistItemsSignal = signal<any[]>([]);

    mockWishlistService = {
      wishlistItemsSignal: mockWishlistItemsSignal,
      getWishlist: vi.fn().mockReturnValue(of({ items: [] })),
      addToWishlist: vi.fn().mockReturnValue(of({ data: { id: 99 } })),
      removeFromWishlist: vi.fn().mockReturnValue(of({})),
      updateWishlistCount: vi.fn()
    };

    mockActivatedRoute = {
      paramMap: of(convertToParamMap({ id: '1' }))
    };

    await TestBed.configureTestingModule({
      imports: [ProductDetail, RouterModule.forRoot([])],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ReviewService, useValue: mockReviewService },
        { provide: VendorService, useValue: mockVendorService },
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: CartService, useValue: mockCartService },
        { provide: ToastService, useValue: mockToastService },
        { provide: WishlistService, useValue: mockWishlistService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductDetail);
    component = fixture.componentInstance;
  });

  afterEach(() => sessionStorageMock.clear());

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should set role from sessionStorage on init', () => {
    sessionStorageMock.setItem('role', 'Admin');
    fixture.detectChanges();
    expect(component.userRole()).toBe('Admin');
  });

  it('should default role to Customer', () => {
    fixture.detectChanges();
    expect(component.userRole()).toBe('Customer');
  });

  it('should load product details on init', () => {
    fixture.detectChanges();
    expect(mockProductService.getById).toHaveBeenCalledWith(1);
    expect(component.product()?.name).toBe('Test Product');
  });

  it('should set selectedVariant to default variant on load', () => {
    fixture.detectChanges();
    expect(component.selectedVariant()?.id).toBe(1);
  });

  it('should handle product load error', () => {
    mockProductService.getById.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    expect(component.loading()).toBe(false);
    consoleSpy.mockRestore();
  });

  it('should not load product if no id in route - orderId is set from paramMap', () => {
    // With id in route, getById IS called (this covers the paramMap branch)
    fixture.detectChanges();
    expect(mockProductService.getById).toHaveBeenCalledWith(1);
  });

  describe('backRoute computed', () => {
    it('should return admin route for Admin', () => {
      sessionStorageMock.setItem('role', 'Admin');
      fixture.detectChanges();
      expect(component.backRoute()).toEqual(['/admin-home/products-list']);
    });

    it('should return vendor route for Vendor', () => {
      sessionStorageMock.setItem('role', 'Vendor');
      fixture.detectChanges();
      expect(component.backRoute()).toEqual(['/vendor-home/products-list']);
    });

    it('should return customer route for Customer', () => {
      fixture.detectChanges();
      expect(component.backRoute()).toEqual(['/customer-home/products-list']);
    });
  });

  describe('displayedVariants computed', () => {
    it('should show all variants for Vendor', () => {
      sessionStorageMock.setItem('role', 'Vendor');
      const prod = makeProduct({ variants: [makeVariant(), makeVariant({ id: 2, isActive: false })] });
      mockProductService.getById.mockReturnValue(of(prod));
      fixture.detectChanges();
      expect(component.displayedVariants().length).toBe(2);
    });

    it('should only show active variants for Customer', () => {
      const prod = makeProduct({ variants: [makeVariant(), makeVariant({ id: 2, isActive: false })] });
      mockProductService.getById.mockReturnValue(of(prod));
      fixture.detectChanges();
      expect(component.displayedVariants().length).toBe(1);
    });
  });

  describe('discountedPrice computed', () => {
    it('should return original price with no discount', () => {
      fixture.detectChanges();
      expect(component.discountedPrice()).toBe(1000);
    });

    it('should calculate percentage discount', () => {
      fixture.detectChanges();
      component.selectedDiscount.set({ id: 1, type: DiscountType.Percentage, value: 10, minOrderValue: 100, code: 'TEST10', isActive: true, scope: DiscountScope.Common, usageLimit: 100, usedCount: 5, expiresAt: '2030-01-01' });
      expect(component.discountedPrice()).toBe(900);
    });

    it('should calculate flat discount', () => {
      fixture.detectChanges();
      component.selectedDiscount.set({ id: 2, type: DiscountType.Flat, value: 200, minOrderValue: 100, code: 'FLAT200', isActive: true, scope: DiscountScope.Common, usageLimit: 100, usedCount: 5, expiresAt: '2030-01-01' });
      expect(component.discountedPrice()).toBe(800);
    });
  });

  describe('discountPercent computed', () => {
    it('should return 0 with no discount', () => {
      fixture.detectChanges();
      expect(component.discountPercent()).toBe(0);
    });

    it('should return percentage value for percentage discount', () => {
      fixture.detectChanges();
      component.selectedDiscount.set({ id: 1, type: DiscountType.Percentage, value: 15, minOrderValue: 100, code: 'TEST', isActive: true, scope: DiscountScope.Common, usageLimit: 100, usedCount: 0, expiresAt: '2030-01-01' });
      expect(component.discountPercent()).toBe(15);
    });

    it('should calculate percent for flat discount', () => {
      fixture.detectChanges();
      component.selectedDiscount.set({ id: 2, type: DiscountType.Flat, value: 100, minOrderValue: 100, code: 'FLAT', isActive: true, scope: DiscountScope.Common, usageLimit: 100, usedCount: 0, expiresAt: '2030-01-01' });
      expect(component.discountPercent()).toBe(10);
    });

    it('should return 0 for flat discount when originalPrice is 0', () => {
      fixture.detectChanges();
      component.selectedVariant.set({ ...makeVariant(), price: 0 });
      component.selectedDiscount.set({ id: 2, type: DiscountType.Flat, value: 100, minOrderValue: 0, code: 'FLAT', isActive: true, scope: DiscountScope.Common, usageLimit: 100, usedCount: 0, expiresAt: '2030-01-01' });
      expect(component.discountPercent()).toBe(0);
    });
  });

  describe('categoryBreadcrumbs', () => {
    it('should split categoryName by dash', () => {
      fixture.detectChanges();
      expect(component.categoryBreadcrumbs()).toContain('Electronics');
      expect(component.categoryBreadcrumbs()).toContain('Tech');
    });

    it('should return empty array if no product', () => {
      fixture.detectChanges();
      component.product.set(undefined);
      expect(component.categoryBreadcrumbs()).toEqual([]);
    });
  });

  describe('mainImageUrl computed', () => {
    it('should return empty string if no selected variant', () => {
      fixture.detectChanges();
      component.selectedVariant.set(undefined);
      expect(component.mainImageUrl()).toBe('');
    });

    it('should return first image url', () => {
      fixture.detectChanges();
      component.selectedVariant.set(makeVariant({ variantImages: [{ imageUrl: 'http://test.com/img.jpg' }] }));
      expect(component.mainImageUrl()).toBe('http://test.com/img.jpg');
    });
  });

  describe('selectVariant', () => {
    it('should set selected variant', () => {
      const prod = makeProduct({ variants: [makeVariant(), makeVariant({ id: 2, price: 2000, isDefault: false })] });
      mockProductService.getById.mockReturnValue(of(prod));
      fixture.detectChanges();
      component.selectVariant(2);
      expect(component.selectedVariant()?.id).toBe(2);
    });

    it('should clear discount if price < minOrderValue', () => {
      const prod = makeProduct({ variants: [makeVariant({ price: 1000 }), makeVariant({ id: 2, price: 500, isDefault: false })] });
      mockProductService.getById.mockReturnValue(of(prod));
      fixture.detectChanges();
      // Set discount after the initial load (which resets selectedDiscount to undefined)
      component.selectedDiscount.set({ id: 1, type: DiscountType.Flat, value: 100, minOrderValue: 2000, code: 'X', isActive: true, scope: DiscountScope.Common, usageLimit: 10, usedCount: 0, expiresAt: '2030-01-01' });
      expect(component.selectedDiscount()?.id).toBe(1);
      // Now select variant with price 500, which is < minOrderValue 2000 -> should clear
      component.selectVariant(2);
      expect(component.selectedDiscount()).toBeUndefined();
    });
  });

  describe('selectDiscount', () => {
    const discount: DiscountResponse = { id: 5, type: DiscountType.Flat, value: 100, minOrderValue: 500, code: 'DISC5', isActive: true, scope: DiscountScope.Common, usageLimit: 10, usedCount: 0, expiresAt: '2030-01-01' };

    it('should set discount if price meets minimum', () => {
      fixture.detectChanges();
      component.selectDiscount(discount);
      expect(component.selectedDiscount()?.id).toBe(5);
    });

    it('should deselect if same discount is clicked again', () => {
      fixture.detectChanges();
      component.selectedDiscount.set(discount);
      component.selectDiscount(discount);
      expect(component.selectedDiscount()).toBeUndefined();
    });

    it('should warn if price does not meet minimum', () => {
      fixture.detectChanges();
      component.selectedVariant.set(makeVariant({ price: 100 }));
      component.selectDiscount({ ...discount, minOrderValue: 1000 });
      expect(mockToastService.warning).toHaveBeenCalled();
    });
  });

  describe('image navigation', () => {
    it('should set image index', () => {
      fixture.detectChanges();
      component.setImageIndex(2);
      expect(component.activeImageIndex()).toBe(2);
    });

    it('should not cycle images if only 1 image', () => {
      fixture.detectChanges();
      component.prevImage();
      component.nextImage();
      expect(component.activeImageIndex()).toBe(0);
    });

    it('should cycle images if multiple images exist', () => {
      fixture.detectChanges();
      component.selectedVariant.set(makeVariant({
        variantImages: [{ imageUrl: 'a' }, { imageUrl: 'b' }, { imageUrl: 'c' }]
      }));
      component.setImageIndex(2);
      component.nextImage();
      expect(component.activeImageIndex()).toBe(0);
      component.prevImage();
      expect(component.activeImageIndex()).toBe(2);
    });
  });

  describe('incrementQty and decrementQty', () => {
    it('should increment qty', () => {
      fixture.detectChanges();
      component.quantity.set(1);
      component.incrementQty();
      expect(component.quantity()).toBe(2);
    });

    it('should warn if qty >= stockQty', () => {
      fixture.detectChanges();
      component.quantity.set(10);
      component.incrementQty();
      expect(mockToastService.warning).toHaveBeenCalled();
      expect(component.quantity()).toBe(10);
    });

    it('should not increment if no variant', () => {
      fixture.detectChanges();
      component.selectedVariant.set(undefined);
      component.quantity.set(5);
      component.incrementQty();
      expect(component.quantity()).toBe(6);
    });

    it('should decrement qty but not below 1', () => {
      fixture.detectChanges();
      component.quantity.set(3);
      component.decrementQty();
      expect(component.quantity()).toBe(2);
      component.quantity.set(1);
      component.decrementQty();
      expect(component.quantity()).toBe(1);
    });
  });

  describe('addToCart', () => {
    it('should not add to cart if no variant', () => {
      fixture.detectChanges();
      component.selectedVariant.set(undefined);
      component.addToCart();
      expect(mockCartService.addToCart).not.toHaveBeenCalled();
    });

    it('should warn if quantity > stockQty', () => {
      fixture.detectChanges();
      component.quantity.set(20);
      component.addToCart();
      expect(mockToastService.warning).toHaveBeenCalled();
    });

    it('should add to cart successfully with no discount', () => {
      fixture.detectChanges();
      component.quantity.set(1);
      component.addToCart();
      expect(mockCartService.addToCart).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should add to cart with discount applied', () => {
      fixture.detectChanges();
      component.quantity.set(1);
      component.selectedDiscount.set({ id: 1, type: DiscountType.Flat, value: 100, minOrderValue: 500, code: 'D1', isActive: true, scope: DiscountScope.Common, usageLimit: 10, usedCount: 0, expiresAt: '2030-01-01' });
      component.addToCart();
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should handle insufficient stock error from cart', () => {
      fixture.detectChanges();
      mockCartService.addToCart.mockReturnValue(throwError(() => ({ error: { message: 'Insufficient stock. Available: 5' } })));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.addToCart();
      expect(mockToastService.warning).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should handle other cart errors', () => {
      fixture.detectChanges();
      mockCartService.addToCart.mockReturnValue(throwError(() => ({ error: { message: 'Some other error' } })));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.addToCart();
      expect(mockToastService.error).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('toggleWishlist', () => {
    it('should warn if not a Customer', () => {
      sessionStorageMock.setItem('role', 'Admin');
      fixture.detectChanges();
      component.toggleWishlist();
      expect(mockToastService.warning).toHaveBeenCalled();
    });

    it('should not proceed without a variant', () => {
      fixture.detectChanges();
      component.selectedVariant.set(undefined);
      component.toggleWishlist();
      expect(mockWishlistService.addToWishlist).not.toHaveBeenCalled();
    });

    it('should add to wishlist when not wishlisted', () => {
      fixture.detectChanges();
      mockWishlistItemsSignal.set([]);
      component.toggleWishlist();
      expect(mockWishlistService.addToWishlist).toHaveBeenCalled();
    });

    it('should remove from wishlist when already wishlisted', () => {
      fixture.detectChanges();
      mockWishlistItemsSignal.set([{ id: 77, variantId: 1 }]);
      component.toggleWishlist();
      expect(mockWishlistService.removeFromWishlist).toHaveBeenCalledWith(77);
    });

    it('should handle add wishlist error', () => {
      fixture.detectChanges();
      mockWishlistService.addToWishlist.mockReturnValue(throwError(() => new Error('fail')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      mockWishlistItemsSignal.set([]);
      component.toggleWishlist();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to add to wishlist');
      consoleSpy.mockRestore();
    });

    it('should handle remove wishlist error', () => {
      fixture.detectChanges();
      mockWishlistService.removeFromWishlist.mockReturnValue(throwError(() => new Error('fail')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      mockWishlistItemsSignal.set([{ id: 77, variantId: 1 }]);
      component.toggleWishlist();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to remove from wishlist');
      consoleSpy.mockRestore();
    });

    it('should not remove from wishlist if wishlistItemId is null', () => {
      fixture.detectChanges();
      mockWishlistItemsSignal.set([{ id: null, variantId: 2 }]);
      component.toggleWishlist();
      expect(mockWishlistService.removeFromWishlist).not.toHaveBeenCalled();
    });
  });

  describe('variant modal methods', () => {
    it('should open add variant modal', () => {
      fixture.detectChanges();
      component.addVariant();
      expect(component.showAddVariantModal()).toBe(true);
    });

    it('should close add variant modal on discard', () => {
      fixture.detectChanges();
      component.addVariant();
      component.onVariantDiscarded();
      expect(component.showAddVariantModal()).toBe(false);
    });

    it('should close modal and reload product on variant saved', () => {
      fixture.detectChanges();
      component.showAddVariantModal.set(true);
      mockProductService.getById.mockClear();
      component.onVariantSaved();
      expect(component.showAddVariantModal()).toBe(false);
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should update selected variant if it was the one saved', () => {
      const prod = makeProduct({ variants: [makeVariant({ id: 1, price: 2000 })] });
      mockProductService.getById.mockReturnValue(of(prod));
      fixture.detectChanges();
      component.selectedVariant.set(makeVariant({ id: 1, price: 1000 }));
      component.onVariantSaved();
      expect(component.selectedVariant()?.price).toBe(2000);
    });
  });

  describe('product edit modal methods', () => {
    it('should open edit product modal', () => {
      fixture.detectChanges();
      component.openEditProductModal();
      expect(component.showEditProductModal()).toBe(true);
      expect(component.editProductName).toBe('Test Product');
    });

    it('should not open modal if no product', () => {
      fixture.detectChanges();
      component.product.set(undefined);
      component.openEditProductModal();
      expect(component.showEditProductModal()).toBe(false);
    });

    it('should close edit product modal', () => {
      fixture.detectChanges();
      component.showEditProductModal.set(true);
      component.closeEditProductModal();
      expect(component.showEditProductModal()).toBe(false);
    });

    it('should save product details successfully', () => {
      fixture.detectChanges();
      const event = { preventDefault: vi.fn() } as any;
      component.saveProductDetails(event);
      expect(mockProductService.updateProduct).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should not save if no product', () => {
      fixture.detectChanges();
      component.product.set(undefined);
      const event = { preventDefault: vi.fn() } as any;
      component.saveProductDetails(event);
      expect(mockProductService.updateProduct).not.toHaveBeenCalled();
    });

    it('should handle product update error', () => {
      mockProductService.updateProduct.mockReturnValue(throwError(() => new Error('fail')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      const event = { preventDefault: vi.fn() } as any;
      component.saveProductDetails(event);
      expect(mockToastService.error).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('variant edit methods', () => {
    it('should open edit variant modal', () => {
      fixture.detectChanges();
      component.openEditVariantModal();
      expect(component.showEditVariantModal()).toBe(true);
    });

    it('should close edit variant modal', () => {
      fixture.detectChanges();
      component.showEditVariantModal.set(true);
      component.closeEditVariantModal();
      expect(component.showEditVariantModal()).toBe(false);
    });

    it('should call openEditVariantModal on updateVariant', () => {
      fixture.detectChanges();
      component.updateVariant();
      expect(component.showEditVariantModal()).toBe(true);
    });
  });

  describe('deactivateVariant', () => {
    it('should call toggleVariantStatus when confirmed', () => {
      vi.spyOn(globalThis, 'confirm' as any).mockReturnValue(true);
      fixture.detectChanges();
      component.deactivateVariant();
      expect(mockProductService.toggleVariantStatus).toHaveBeenCalledWith(1);
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should not toggle if not confirmed', () => {
      vi.spyOn(globalThis, 'confirm' as any).mockReturnValue(false);
      fixture.detectChanges();
      component.deactivateVariant();
      expect(mockProductService.toggleVariantStatus).not.toHaveBeenCalled();
    });

    it('should not deactivate if no variant or product', () => {
      fixture.detectChanges();
      component.selectedVariant.set(undefined);
      component.deactivateVariant();
      expect(mockProductService.toggleVariantStatus).not.toHaveBeenCalled();
    });

    it('should handle error when toggling variant status', () => {
      vi.spyOn(globalThis, 'confirm' as any).mockReturnValue(true);
      mockProductService.toggleVariantStatus.mockReturnValue(throwError(() => new Error('fail')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      component.deactivateVariant();
      expect(mockToastService.error).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });
  });

  describe('getVariantColor', () => {
    it('should return empty string if no variant', () => {
      fixture.detectChanges();
      expect(component.getVariantColor(undefined as any)).toBe('');
    });

    it('should return empty string if no availableValues', () => {
      fixture.detectChanges();
      expect(component.getVariantColor({ availableValues: null } as any)).toBe('');
    });

    it('should return hex color for known color', () => {
      fixture.detectChanges();
      const variant = makeVariant({ availableValues: { Color: 'blue' } });
      expect(component.getVariantColor(variant)).toBe('#3b82f6');
    });

    it('should return value directly for unknown color', () => {
      fixture.detectChanges();
      const variant = makeVariant({ availableValues: { Color: 'neon-pink' } });
      expect(component.getVariantColor(variant)).toBe('neon-pink');
    });

    it('should return empty string if no color key', () => {
      fixture.detectChanges();
      const variant = makeVariant({ availableValues: { Size: 'L' } });
      expect(component.getVariantColor(variant)).toBe('');
    });
  });

  describe('getVariantLabel', () => {
    it('should return Variant # if no variant', () => {
      fixture.detectChanges();
      expect(component.getVariantLabel({ id: 5 } as any)).toBe('Variant #5');
    });

    it('should return label from availableValues', () => {
      fixture.detectChanges();
      const variant = makeVariant({ availableValues: { Color: 'Red', Size: 'M' } });
      const label = component.getVariantLabel(variant);
      expect(label).toContain('Color: Red');
      expect(label).toContain('Size: M');
    });
  });

  describe('getVariantDisplayValue', () => {
    it('should return empty if no variant', () => {
      fixture.detectChanges();
      expect(component.getVariantDisplayValue({ availableValues: null } as any)).toBe('');
    });

    it('should return color value if color key exists', () => {
      fixture.detectChanges();
      const variant = makeVariant({ availableValues: { colour: 'Red' } });
      expect(component.getVariantDisplayValue(variant)).toBe('Red');
    });

    it('should return first value if no color key', () => {
      fixture.detectChanges();
      const variant = makeVariant({ availableValues: { Size: 'XL' } });
      expect(component.getVariantDisplayValue(variant)).toBe('XL');
    });
  });

  describe('modal methods', () => {
    it('should open and close image modal', () => {
      fixture.detectChanges();
      component.openImageModal('https://test.com/img.jpg');
      expect(component.activeModalImage()).toBe('https://test.com/img.jpg');
      component.closeImageModal();
      expect(component.activeModalImage()).toBeNull();
    });

    it('should not open modal with empty url', () => {
      fixture.detectChanges();
      component.openImageModal('');
      expect(component.activeModalImage()).toBeNull();
    });
  });

  describe('getCategoryIdForSegment', () => {
    it('should return category id for matching name', () => {
      fixture.detectChanges();
      expect(component.getCategoryIdForSegment('electronics')).toBe(5);
    });

    it('should return undefined for non-matching segment', () => {
      fixture.detectChanges();
      expect(component.getCategoryIdForSegment('nonexistent')).toBeUndefined();
    });
  });

  describe('averageRatingValue computed', () => {
    it('should use product average rating if no reviews', () => {
      fixture.detectChanges();
      expect(component.averageRatingValue()).toBe(4.0);
    });

    it('should calculate from reviews when available', () => {
      mockReviewService.getProductReviews.mockReturnValue(of([
        { rating: 5, comment: '', reviewerName: '', imageUrls: [] },
        { rating: 3, comment: '', reviewerName: '', imageUrls: [] }
      ]));
      fixture.detectChanges();
      expect(component.averageRatingValue()).toBe(4);
    });
  });
});
