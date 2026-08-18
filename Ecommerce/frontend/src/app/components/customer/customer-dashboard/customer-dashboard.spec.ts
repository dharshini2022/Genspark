import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomerDashboard } from './customer-dashboard';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { DiscountService } from '../../../services/disocunt.service';
import { CartService } from '../../../services/cart.service';
import { AuthService } from '../../../services/auth.service';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

describe('CustomerDashboard', () => {
  let component: CustomerDashboard;
  let fixture: ComponentFixture<CustomerDashboard>;
  let mockProductService: any;
  let mockCategoryService: any;
  let mockDiscountService: any;
  let mockCartService: any;
  let mockAuthService: any;

  const mockCatalogResponse = {
    items: [
      {
        id: 1,
        name: 'Product A',
        variants: [{ id: 10, price: 100, isDefault: true, isActive: true }]
      }
    ],
    totalCount: 1,
    pageNumber: 1,
    pageSize: 6,
    totalPages: 1
  };

  beforeEach(async () => {
    mockProductService = {
      getCatalog: vi.fn().mockReturnValue(of(mockCatalogResponse))
    };

    mockCategoryService = {
      getCategories: vi.fn().mockReturnValue(of([]))
    };

    mockDiscountService = {
      getApplicableLockedDiscounts: vi.fn().mockReturnValue(of([])),
      getDiscountsOfProduct: vi.fn().mockReturnValue(of([]))
    };

    mockCartService = {
      cartCountSignal: signal(0),
      updateCartCount: vi.fn()
    };

    mockAuthService = {};

    await TestBed.configureTestingModule({
      imports: [CustomerDashboard],
      providers: [
        provideRouter([]),
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: CartService, useValue: mockCartService },
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerDashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load new arrivals', () => {
    expect(component).toBeTruthy();
    expect(mockProductService.getCatalog).toHaveBeenCalledWith({
      sortBy: 'newest',
      sortOrder: 'desc',
      pageSize: 6,
      pageNumber: 1
    });
    expect(component.newArrivals()).toEqual(mockCatalogResponse.items);
    expect(component.loading()).toBe(false);
  });

  it('should handle catalog loading error', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockProductService.getCatalog.mockReturnValue(throwError(() => new Error('API Error')));
    component.loadNewArrivals();
    expect(component.loading()).toBe(false);
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });
});
