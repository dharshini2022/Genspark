import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DiscountForm } from './discount-form';
import { DiscountService } from '../../../services/disocunt.service';
import { ToastService } from '../../../services/toast.service';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach, beforeAll } from 'vitest';

const sessionStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem(key: string) { return store[key] || null; },
    setItem(key: string, value: string) { store[key] = value.toString(); },
    removeItem(key: string) { delete store[key]; },
    clear() { store = {}; }
  };
})();

Object.defineProperty(globalThis, 'sessionStorage', {
  value: sessionStorageMock,
  writable: true
});
describe('DiscountForm', () => {
  let component: DiscountForm;
  let fixture: ComponentFixture<DiscountForm>;

  let mockDiscountService: any;
  let mockToastService: any;
  let mockProductService: any;
  let mockCategoryService: any;

  beforeEach(async () => {
    mockDiscountService = {
      createDiscount: vi.fn().mockReturnValue(of({ message: 'Success' }))
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    mockProductService = {
      search: vi.fn().mockReturnValue(of([{ id: 1, name: 'Search Product' }]))
    };

    mockCategoryService = {
      getCategories: vi.fn().mockReturnValue(of([{ id: 2, name: 'Category 1', slug: 'category-1' }]))
    };

    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [DiscountForm],
      providers: [
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ToastService, useValue: mockToastService },
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  const createComponent = () => {
    fixture = TestBed.createComponent(DiscountForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  describe('Role-based initialization', () => {
    it('should initialize with Common scope if role is Admin', () => {
      sessionStorage.setItem('role', 'Admin');
      createComponent();
      expect(component.isAdmin).toBe(true);
      expect(component.selectedScope()).toBe('Common');
    });

    it('should initialize with Vendor scope if role is not Admin', () => {
      sessionStorage.setItem('role', 'Vendor');
      createComponent();
      expect(component.isAdmin).toBe(false);
      expect(component.selectedScope()).toBe('Vendor');
    });
  });

  describe('Scope Selection & Loading Categories', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should set scope and load categories if scope is Category and categories are empty', () => {
      expect(component.categories().length).toBe(0);
      component.onScopeChange('Category');
      expect(component.selectedScope()).toBe('Category');
      expect(mockCategoryService.getCategories).toHaveBeenCalled();
      expect(component.categories().length).toBe(1);
      expect(component.categories()[0].name).toBe('Category 1');
    });

    it('should not load categories if they are already loaded', () => {
      component.categories.set([{ id: 9, name: 'Already loaded', slug: 'already-loaded' }]);
      mockCategoryService.getCategories.mockClear();
      component.onScopeChange('Category');
      expect(mockCategoryService.getCategories).not.toHaveBeenCalled();
    });

    it('should set scope without loading categories if scope is not Category', () => {
      component.onScopeChange('Product');
      expect(component.selectedScope()).toBe('Product');
      expect(mockCategoryService.getCategories).not.toHaveBeenCalled();
    });

    it('should log error when getCategories fails', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      mockCategoryService.getCategories.mockReturnValue(throwError(() => new Error('Category fetch failed')));
      component.onScopeChange('Category');
      expect(consoleSpy).toHaveBeenCalled();
      expect(component.categories().length).toBe(0);
      consoleSpy.mockRestore();
    });
  });

  describe('Product Search', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should clear searchedProducts if query is empty or whitespace', () => {
      component.searchedProducts.set([{ id: 1, name: 'Dummy' }]);
      component.onProductSearchChange('');
      expect(component.searchedProducts().length).toBe(0);
      expect(mockProductService.search).not.toHaveBeenCalled();

      component.searchedProducts.set([{ id: 1, name: 'Dummy' }]);
      component.onProductSearchChange('   ');
      expect(component.searchedProducts().length).toBe(0);
      expect(mockProductService.search).not.toHaveBeenCalled();
    });

    it('should query productService when search term is provided', () => {
      component.onProductSearchChange('test');
      expect(component.productSearchQuery()).toBe('test');
      expect(mockProductService.search).toHaveBeenCalledWith('test');
      expect(component.searchedProducts().length).toBe(1);
      expect(component.searchedProducts()[0].name).toBe('Search Product');
    });

    it('should log error if product search fails', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      mockProductService.search.mockReturnValue(throwError(() => new Error('Search failed')));
      component.onProductSearchChange('test');
      expect(consoleSpy).toHaveBeenCalled();
      expect(component.searchedProducts().length).toBe(0);
      consoleSpy.mockRestore();
    });

    it('should select product, update signals, and clear searchedProducts', () => {
      const targetProduct = { id: 101, name: 'Target Item' };
      component.searchedProducts.set([targetProduct]);
      component.selectProduct(targetProduct);

      expect(component.selectedProduct()).toBe(targetProduct);
      expect(component.productSearchQuery()).toBe('Target Item');
      expect(component.searchedProducts().length).toBe(0);
    });
  });

  describe('Cancel Form Action', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should emit cancel event when cancelAddDiscount is called', () => {
      const cancelSpy = vi.spyOn(component.cancel, 'emit');
      component.cancelAddDiscount();
      expect(cancelSpy).toHaveBeenCalled();
    });
  });

  describe('Form Validation and Submission', () => {
    let mockEvent: any;

    beforeEach(() => {
      createComponent();
      mockEvent = {
        preventDefault: vi.fn()
      };
    });

    it('should block submission and show toast if discount value is <= 0 or null', () => {
      component.newDiscountValue.set(null);
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Discount value must be greater than 0.');

      mockToastService.error.mockClear();
      component.newDiscountValue.set(-5);
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Discount value must be greater than 0.');
    });

    it('should block submission and show toast if type is Percentage and value > 100', () => {
      component.newDiscountValue.set(105);
      component.newDiscountType.set('Percentage');
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Percentage discount cannot exceed 100%.');
    });

    it('should block submission and show toast if minOrder is null or negative', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(null);
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Minimum order value cannot be negative.');

      mockToastService.error.mockClear();
      component.newDiscountMinOrder.set(-1);
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Minimum order value cannot be negative.');
    });

    it('should block submission and show toast if Flat discount is greater than minOrder', () => {
      component.newDiscountValue.set(50);
      component.newDiscountMinOrder.set(40);
      component.newDiscountType.set('Flat');
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Flat discount value cannot exceed minimum order value.');
    });

    it('should block submission and show toast if usageLimit is null or < 10', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(0);
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Usage limit must be at least 10.');
    });

    it('should block submission and show toast if expiry is missing', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(10);
      component.newDiscountExpiry.set('');
      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Expiry date is required.');
    });

    it('should block submission and show toast if scope is Product and no product is selected', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(10);
      component.newDiscountExpiry.set('2030-12-31');
      component.selectedScope.set('Product');
      component.selectedProduct.set(null);

      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Please select a product for the Product-scoped discount.');
    });

    it('should block submission and show toast if scope is Category and no category is selected', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(10);
      component.newDiscountExpiry.set('2030-12-31');
      component.selectedScope.set('Category');
      component.selectedCategoryId.set(null);

      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Please select a category for the Category-scoped discount.');
    });

    it('should block submission and show toast if expiry is in the past', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(10);
      component.newDiscountExpiry.set('2000-01-01');
      component.selectedScope.set('Common');

      component.submitDiscount(mockEvent);
      expect(mockToastService.error).toHaveBeenCalledWith('Expiry date must be in the future.');
    });

    it('should submit successfully and emit success when data is valid', () => {
      component.newDiscountValue.set(15);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(20);
      component.newDiscountExpiry.set('2035-12-31');
      component.selectedScope.set('Product');
      component.selectedProduct.set({ id: 99, name: 'Target Prod' });

      const successSpy = vi.spyOn(component.success, 'emit');

      component.submitDiscount(mockEvent);

      expect(component.submittingDiscount()).toBe(false);
      expect(mockToastService.success).toHaveBeenCalledWith('Discount coupon created successfully!');
      expect(successSpy).toHaveBeenCalled();
      expect(mockDiscountService.createDiscount).toHaveBeenCalledWith({
        scope: 'Product',
        type: 'Flat',
        value: 15,
        minOrderValue: 100,
        usageLimit: 20,
        expiresAt: new Date('2035-12-31').toISOString(),
        productId: 99,
        categoryId: undefined
      });
    });

    it('should submit successfully with Category scope when data is valid', () => {
      component.newDiscountValue.set(20);
      component.newDiscountMinOrder.set(150);
      component.newDiscountUsageLimit.set(50);
      component.newDiscountExpiry.set('2035-12-31');
      component.selectedScope.set('Category');
      component.selectedCategoryId.set(45);

      component.submitDiscount(mockEvent);

      expect(mockDiscountService.createDiscount).toHaveBeenCalledWith({
        scope: 'Category',
        type: 'Flat',
        value: 20,
        minOrderValue: 150,
        usageLimit: 50,
        expiresAt: new Date('2035-12-31').toISOString(),
        productId: undefined,
        categoryId: 45
      });
    });

    it('should handle submission errors from discountService', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(10);
      component.newDiscountExpiry.set('2035-12-31');
      component.selectedScope.set('Common');

      mockDiscountService.createDiscount.mockReturnValue(throwError(() => ({
        error: { message: 'Coupon code already exists.' }
      })));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.submitDiscount(mockEvent);

      expect(component.submittingDiscount()).toBe(false);
      expect(mockToastService.error).toHaveBeenCalledWith('Coupon code already exists.');
      consoleSpy.mockRestore();
    });

    it('should fall back to standard error message on service failure if message field is absent', () => {
      component.newDiscountValue.set(10);
      component.newDiscountMinOrder.set(100);
      component.newDiscountUsageLimit.set(10);
      component.newDiscountExpiry.set('2035-12-31');
      component.selectedScope.set('Common');

      mockDiscountService.createDiscount.mockReturnValue(throwError(() => ({
        error: null
      })));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.submitDiscount(mockEvent);

      expect(component.submittingDiscount()).toBe(false);
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to create discount.');
      consoleSpy.mockRestore();
    });
  });

  describe('Template and Edge Cases', () => {
    beforeEach(() => {
      createComponent();
    });

    it('should set categories to empty array if service returns null', () => {
      mockCategoryService.getCategories.mockReturnValue(of(null));
      component.loadCategories();
      expect(component.categories()).toEqual([]);
    });

    it('should set searchedProducts to empty array if search returns null', () => {
      mockProductService.search.mockReturnValue(of(null));
      component.onProductSearchChange('test');
      expect(component.searchedProducts()).toEqual([]);
    });

    it('should trigger onScopeChange when select value changes in template', async () => {
      component.isAdmin = true;
      fixture.detectChanges();
      
      const selectElement = fixture.nativeElement.querySelector('#discountScope');
      expect(selectElement).toBeTruthy();
      
      selectElement.value = 'Category';
      selectElement.dispatchEvent(new Event('change'));
      component.onScopeChange('Category');
      fixture.detectChanges();
      await fixture.whenStable();
      
      expect(component.selectedScope()).toBe('Category');
      expect(mockCategoryService.getCategories).toHaveBeenCalled();
    });

    it('should trigger onProductSearchChange and select product from template', async () => {
      component.selectedScope.set('Product');
      fixture.detectChanges();
      
      const inputElement = fixture.nativeElement.querySelector('#productSearch');
      expect(inputElement).toBeTruthy();
      
      inputElement.value = 'test product';
      inputElement.dispatchEvent(new Event('input'));
      component.onProductSearchChange('test product');
      fixture.detectChanges();
      await fixture.whenStable();
      
      expect(component.productSearchQuery()).toBe('test product');
      
      component.searchedProducts.set([{ id: 9, name: 'Search Product' }]);
      fixture.detectChanges();
      
      const listItems = fixture.nativeElement.querySelectorAll('.autocomplete-item');
      expect(listItems.length).toBeGreaterThan(0);
      
      listItems[0].click();
      component.selectProduct({ id: 9, name: 'Search Product' });
      fixture.detectChanges();
      await fixture.whenStable();
      
      expect(component.selectedProduct()?.name).toBe('Search Product');
    });

    it('should trigger category selection from template', async () => {
      component.selectedScope.set('Category');
      component.categories.set([{ id: 10, name: 'Cat 10', slug: 'cat-10' }]);
      fixture.detectChanges();
      
      const selectElement = fixture.nativeElement.querySelector('#discountCategory');
      expect(selectElement).toBeTruthy();
      
      selectElement.value = '10';
      selectElement.dispatchEvent(new Event('change'));
      component.selectedCategoryId.set(10);
      fixture.detectChanges();
      await fixture.whenStable();
      
      expect(component.selectedCategoryId()).toBe(10);
    });
  });
});
