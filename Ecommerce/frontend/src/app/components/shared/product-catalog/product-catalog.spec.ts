import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductCatalog } from './product-catalog';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ActivatedRoute, RouterModule, convertToParamMap } from '@angular/router';
import { of, throwError, BehaviorSubject } from 'rxjs';
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

describe('ProductCatalog', () => {
  let component: ProductCatalog;
  let fixture: ComponentFixture<ProductCatalog>;
  let mockProductService: any;
  let mockCategoryService: any;
  let mockDiscountService: any;
  let mockActivatedRoute: any;
  let queryParamsSubject: BehaviorSubject<any>;

  const makeVariant = (overrides = {}) => ({
    id: 1, productId: 1, price: 1000, stockQty: 10, orderCount: 0,
    isDefault: true, isActive: true, sku: 'SKU', variantImages: [], availableValues: {}, ...overrides
  });

  const makeProduct = (id: number) => ({
    id,
    name: `Product ${id}`,
    description: 'test',
    categoryId: 1,
    categoryName: 'Cat',
    vendorId: 1,
    averageRating: 4.0,
    reviewCount: 10,
    status: 'Active',
    createdAt: '2024-01-01T00:00:00',
    storeName: 'Test Store',
    variants: [makeVariant()]
  });

  const makePagedResponse = (items: any[]) => ({
    items,
    totalCount: items.length,
    pageNumber: 1,
    pageSize: 12,
    totalPages: 1,
    hasNext: false,
    hasPrevious: false
  });

  beforeEach(async () => {
    sessionStorageMock.clear();

    mockProductService = {
      getCatalog: vi.fn().mockReturnValue(of(makePagedResponse([makeProduct(1)])))
    };

    mockCategoryService = {
      getCategories: vi.fn().mockReturnValue(of([{ id: 1, name: 'Electronics', slug: 'electronics' }]))
    };

    mockDiscountService = {
      getDiscountsOfProduct: vi.fn().mockReturnValue(of([]))
    };

    queryParamsSubject = new BehaviorSubject<any>({});
    mockActivatedRoute = {
      queryParams: queryParamsSubject
    };

    await TestBed.configureTestingModule({
      imports: [ProductCatalog, RouterModule.forRoot([])],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductCatalog);
    component = fixture.componentInstance;
  });

  afterEach(() => sessionStorageMock.clear());

  it('should create', async () => {
    fixture.detectChanges();
    
    expect(component).toBeTruthy();
  });

  it('should load categories on init', async () => {
    fixture.detectChanges();
    
    expect(mockCategoryService.getCategories).toHaveBeenCalled();
    expect(component.categories().length).toBe(1);
  });

  it('should handle category load error gracefully', async () => {
    mockCategoryService.getCategories.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should load products on init', async () => {
    fixture.detectChanges();
    expect(mockProductService.getCatalog).toHaveBeenCalled();
    expect(component.products().length).toBeGreaterThan(0);
  });

  it('should handle product load error on first page', async () => {
    mockProductService.getCatalog.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    expect(component.products()).toEqual([]);
    expect(component.loading()).toBe(false);
    consoleSpy.mockRestore();
  });

  it('should apply query param categoryId', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ categoryId: '3' });
    fixture.detectChanges();
    expect(component.selectedCategoryId()).toBe(3);
  });

  it('should clear categoryId from signal when param is empty', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ categoryId: '' });
    fixture.detectChanges();
    expect(component.selectedCategoryId()).toBeUndefined();
  });

  it('should apply search query param', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ search: 'shirt' });
    fixture.detectChanges();
    expect(component.searchQuery()).toBe('shirt');
  });

  it('should set searchQuery to undefined when search param is empty', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ search: '' });
    fixture.detectChanges();
    expect(component.searchQuery()).toBeUndefined();
  });

  it('should apply price filter from query params', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ minPrice: '100', maxPrice: '5000' });
    fixture.detectChanges();
    expect(component.minPrice()).toBe(100);
    expect(component.maxPrice()).toBe(5000);
    expect(component.isPriceFilterActive()).toBe(true);
  });

  it('should apply sort from query params - price sort', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ sortBy: 'price', sortOrder: 'asc' });
    fixture.detectChanges();
    expect(component.currentSort()).toBe('price_asc');
  });

  it('should apply sort from query params - discount sort', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ sortBy: 'discount' });
    fixture.detectChanges();
    expect(component.currentSort()).toBe('discount');
  });

  it('should apply sort from query params - generic sort', async () => {
    fixture.detectChanges();
    queryParamsSubject.next({ sortBy: 'rating' });
    fixture.detectChanges();
    expect(component.currentSort()).toBe('rating');
  });

  it('should call onLoadNextPage and increment currentPage', async () => {
    fixture.detectChanges();
    
    mockProductService.getCatalog.mockClear();
    component.hasNext = true;
    component.onLoadNextPage();
    expect(component.currentPage).toBe(2);
    expect(mockProductService.getCatalog).toHaveBeenCalled();
  });

  it('should onApplyFilters with category and price', async () => {
    fixture.detectChanges();
    
    mockProductService.getCatalog.mockClear();
    component.onApplyFilters({ categoryId: 5, minPrice: 100, maxPrice: 2000 });
    expect(component.selectedCategoryId()).toBe(5);
    expect(component.minPrice()).toBe(100);
    expect(component.maxPrice()).toBe(2000);
    expect(component.isPriceFilterActive()).toBe(true);
    expect(component.isSidebarOpen()).toBe(false);
    expect(mockProductService.getCatalog).toHaveBeenCalled();
  });

  it('should onSortChanged with price_asc', async () => {
    fixture.detectChanges();
    
    component.onSortChanged({ sortBy: 'price', sortOrder: 'asc' });
    expect(component.currentSort()).toBe('price_asc');
  });

  it('should onSortChanged with discount', async () => {
    fixture.detectChanges();
    
    component.onSortChanged({ sortBy: 'discount', sortOrder: 'desc' });
    expect(component.currentSort()).toBe('discount');
  });

  it('should onSortChanged with generic sort', async () => {
    fixture.detectChanges();
    
    component.onSortChanged({ sortBy: 'newest', sortOrder: 'desc' });
    expect(component.currentSort()).toBe('newest');
  });

  it('should onClearFilters reset all state', async () => {
    fixture.detectChanges();
    
    component.selectedCategoryId.set(5);
    component.minPrice.set(500);
    component.isPriceFilterActive.set(true);
    component.isSidebarOpen.set(true);
    component.onClearFilters();
    expect(component.selectedCategoryId()).toBeUndefined();
    expect(component.minPrice()).toBe(0);
    expect(component.maxPrice()).toBe(1000000);
    expect(component.currentSort()).toBe('newest');
    expect(component.isPriceFilterActive()).toBe(false);
    expect(component.isSidebarOpen()).toBe(false);
    expect(component.searchQuery()).toBeUndefined();
  });

  it('should toggleSidebar', async () => {
    fixture.detectChanges();
    
    component.toggleSidebar();
    expect(component.isSidebarOpen()).toBe(true);
    component.toggleSidebar();
    expect(component.isSidebarOpen()).toBe(false);
  });

  describe('enrichProductsWithDiscounts', () => {
    it('should return empty array for empty products', async () => {
      fixture.detectChanges();
      
      let result: any[] = [];
      component.enrichProductsWithDiscounts([]).subscribe(r => { result = r; });
      expect(result).toEqual([]);
    });

    it('should return product without discount if no variant', async () => {
      fixture.detectChanges();
      
      const prod = { ...makeProduct(99), variants: [] };
      let result: any[] = [];
      component.enrichProductsWithDiscounts([prod] as any).subscribe(r => { result = r; });
      expect(result[0].discountLabel).toBe('');
    });

    it('should enrich with flat discount', async () => {
      fixture.detectChanges();
      
      const flatDiscount = { type: DiscountType.Flat, value: 100, minOrderValue: 500 };
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(of([flatDiscount]));
      let result: any[] = [];
      component.enrichProductsWithDiscounts([makeProduct(2)] as any).subscribe(r => { result = r; });
      expect(result[0].discountLabel).toBe('Rs. 100');
      expect(result[0].discountedPrice).toBe(900);
    });

    it('should ignore discount in catalog enrichment if variant price is less than minOrderValue', async () => {
      fixture.detectChanges();
      
      const flatDiscount = { type: DiscountType.Flat, value: 100, minOrderValue: 2000 };
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(of([flatDiscount]));
      let result: any[] = [];
      component.enrichProductsWithDiscounts([makeProduct(2)] as any).subscribe(r => { result = r; });
      expect(result[0].discountLabel).toBe('');
      expect(result[0].discountedPrice).toBe(1000);
    });

    it('should enrich with percentage discount', async () => {
      fixture.detectChanges();
      
      const pctDiscount = { type: DiscountType.Percentage, value: 10, minOrderValue: 100 };
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(of([pctDiscount]));
      let result: any[] = [];
      component.enrichProductsWithDiscounts([makeProduct(2)] as any).subscribe(r => { result = r; });
      expect(result[0].discountLabel).toBe('10%');
      expect(result[0].discountedPrice).toBe(900);
    });

    it('should handle discount enrichment error gracefully', async () => {
      fixture.detectChanges();
      
      mockDiscountService.getDiscountsOfProduct.mockReturnValue(throwError(() => new Error('fail')));
      let result: any[] = [];
      component.enrichProductsWithDiscounts([makeProduct(2)] as any).subscribe(r => { result = r; });
      expect(result[0].discountLabel).toBe('');
    });
  });

  describe('backendSortBy and backendSortOrder computed', () => {
    it('should compute price for price_asc', async () => {
      fixture.detectChanges();
      
      component.currentSort.set('price_asc');
      expect(component.backendSortBy()).toBe('price');
      expect(component.backendSortOrder()).toBe('asc');
    });

    it('should compute price for price_desc', async () => {
      fixture.detectChanges();
      
      component.currentSort.set('price_desc');
      expect(component.backendSortBy()).toBe('price');
      expect(component.backendSortOrder()).toBe('desc');
    });

    it('should compute rating', async () => {
      fixture.detectChanges();
      
      component.currentSort.set('rating');
      expect(component.backendSortBy()).toBe('rating');
    });

    it('should compute discount', async () => {
      fixture.detectChanges();
      
      component.currentSort.set('discount');
      expect(component.backendSortBy()).toBe('discount');
    });

    it('should default to newest', async () => {
      fixture.detectChanges();
      
      component.currentSort.set('newest');
      expect(component.backendSortBy()).toBe('newest');
    });
  });

  it('should load more products and append on subsequent page load', async () => {
    fixture.detectChanges();
    
    const firstCount = component.products().length;
    component.hasNext = true;
    mockProductService.getCatalog.mockReturnValue(of(makePagedResponse([makeProduct(9)])));
    component.onLoadNextPage();
    expect(component.products().length).toBeGreaterThanOrEqual(firstCount);
  });

  it('should handle subsequent page load error', async () => {
    fixture.detectChanges();
    
    component.hasNext = true;
    mockProductService.getCatalog.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    component.onLoadNextPage();
    expect(component.loadingMore()).toBe(false);
    consoleSpy.mockRestore();
  });
});
