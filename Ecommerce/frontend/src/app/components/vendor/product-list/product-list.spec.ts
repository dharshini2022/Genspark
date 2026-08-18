import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductList } from './product-list';
import { ProductService } from '../../../services/product.service';
import { ToastService } from '../../../services/toast.service';
import { CategoryService } from '../../../services/category.service';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Category } from '../../../models/category.model';
import { ProductResponse } from '../../../models/product.model';

describe('ProductList', () => {
  let component: ProductList;
  let fixture: ComponentFixture<ProductList>;
  let mockProductService: any;
  let mockToastService: any;
  let mockCategoryService: any;
  let router: Router;
  let navigateSpy: any;

  beforeEach(async () => {
    mockProductService = {
      getProductsByVendorId: vi.fn().mockReturnValue(of({ items: [], totalCount: 0 })),
      getVendorProducts: vi.fn().mockReturnValue(of({ items: [], totalCount: 0 })),
      updateProduct: vi.fn(),
      toggleProductStatus: vi.fn()
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    mockCategoryService = {
      getCategories: vi.fn().mockReturnValue(of([]))
    };

    await TestBed.configureTestingModule({
      imports: [ProductList],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: ToastService, useValue: mockToastService },
        { provide: CategoryService, useValue: mockCategoryService },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductList);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    navigateSpy = vi.spyOn(router, 'navigate').mockImplementation(async () => true);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create and load products and categories on init', () => {
    const mockCats: Category[] = [{ id: 1, name: 'Electronics', slug: 'electronics' }];
    mockCategoryService.getCategories.mockReturnValue(of(mockCats));
    
    const mockProducts = {
      items: [{ id: 101, name: 'Phone', categoryId: 1, status: 'Active', variants: [] }],
      totalCount: 1
    };
    mockProductService.getVendorProducts.mockReturnValue(of(mockProducts));

    fixture.detectChanges(); // ngOnInit

    expect(component.categories()).toEqual(mockCats);
    expect(component.products()).toEqual(mockProducts.items as any);
    expect(component.totalCount()).toBe(1);
    expect(component.totalPages()).toBe(1);
    expect(mockProductService.getVendorProducts).toHaveBeenCalledWith(1, 5);
  });

  it('should handle error when loadCategories fails', () => {
    mockCategoryService.getCategories.mockReturnValue(throwError(() => new Error('Cat error')));
    const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

    fixture.detectChanges();

    expect(spyConsole).toHaveBeenCalled();
  });

  describe('loadProducts scenarios', () => {
    it('should call getProductsByVendorId when isAdminView is true and vendorId is set', () => {
      component.isAdminView = true;
      component.vendorId = 99;
      
      const mockProducts = {
        items: [{ id: 101, name: 'Phone', categoryId: 1, status: 'Active', variants: [] }],
        totalCount: 1
      };
      mockProductService.getProductsByVendorId.mockReturnValue(of(mockProducts));

      fixture.detectChanges();

      expect(mockProductService.getProductsByVendorId).toHaveBeenCalledWith(99, 1, 5);
      expect(mockProductService.getVendorProducts).not.toHaveBeenCalled();
      expect(component.products()).toEqual(mockProducts.items as any);
    });

    it('should toast error and set loading to false when loadProducts fails', () => {
      mockProductService.getVendorProducts.mockReturnValue(throwError(() => new Error('Load failed')));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      fixture.detectChanges();

      expect(spyConsole).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load products list.');
      expect(component.loading()).toBe(false);
    });
  });

  describe('pagination (setPage)', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should change page and reload products if page is valid', () => {
      component.totalCount.set(12); // totalPages will be Math.ceil(12 / 5) = 3
      expect(component.totalPages()).toBe(3);

      component.setPage(2);
      expect(component.page()).toBe(2);
      expect(mockProductService.getVendorProducts).toHaveBeenCalledWith(2, 5);
    });

    it('should not change page if pageNum is out of bounds', () => {
      component.totalCount.set(12); // totalPages = 3
      component.page.set(2);
      
      component.setPage(0);
      expect(component.page()).toBe(2);

      component.setPage(4);
      expect(component.page()).toBe(2);
    });
  });

  describe('addProduct', () => {
    it('should navigate to add-product page', () => {
      fixture.detectChanges();
      component.addProduct();
      expect(navigateSpy).toHaveBeenCalledWith(['/vendor-home/add-product']);
    });
  });

  describe('edit product modal operations', () => {
    let testProduct: ProductResponse;

    beforeEach(() => {
      testProduct = {
        id: 101,
        name: 'Initial Name',
        description: 'Initial Desc',
        categoryId: 1,
        status: 'Active',
        variants: []
      } as any;
      fixture.detectChanges();
    });

    it('should open modal and populate fields', () => {
      component.openEditProductModal(testProduct);
      expect(component.editingProduct()).toBe(testProduct);
      expect(component.editProductName).toBe('Initial Name');
      expect(component.editProductDescription).toBe('Initial Desc');
      expect(component.editProductCategoryId).toBe(1);
      expect(component.showEditProductModal()).toBe(true);
    });

    it('should close modal and reset editingProduct', () => {
      component.openEditProductModal(testProduct);
      component.closeEditProductModal();
      expect(component.showEditProductModal()).toBe(false);
      expect(component.editingProduct()).toBeNull();
    });

    it('should do nothing in saveProductDetails if editingProduct is null', () => {
      const mockEvent = { preventDefault: vi.fn() } as any;
      component.editingProduct.set(null);

      component.saveProductDetails(mockEvent);

      expect(mockEvent.preventDefault).toHaveBeenCalled();
      expect(mockProductService.updateProduct).not.toHaveBeenCalled();
    });

    it('should save product details successfully', () => {
      const mockEvent = { preventDefault: vi.fn() } as any;
      component.openEditProductModal(testProduct);
      
      component.editProductName = 'New Name';
      component.editProductDescription = 'New Desc';
      component.editProductCategoryId = 2;

      mockProductService.updateProduct.mockReturnValue(of({}));

      component.saveProductDetails(mockEvent);

      expect(mockEvent.preventDefault).toHaveBeenCalled();
      expect(mockProductService.updateProduct).toHaveBeenCalledWith(101, {
        name: 'New Name',
        description: 'New Desc',
        categoryId: 2
      });
      expect(mockToastService.success).toHaveBeenCalledWith('Product details updated successfully.');
      expect(component.showEditProductModal()).toBe(false);
    });

    it('should handle save product details failure', () => {
      const mockEvent = { preventDefault: vi.fn() } as any;
      component.openEditProductModal(testProduct);
      mockProductService.updateProduct.mockReturnValue(throwError(() => new Error('Update failed')));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveProductDetails(mockEvent);

      expect(spyConsole).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to update product details.');
    });
  });

  describe('toggleProductStatus', () => {
    let testProduct: ProductResponse;

    beforeEach(() => {
      testProduct = {
        id: 101,
        name: 'My Product',
        status: 'Active'
      } as any;
      fixture.detectChanges();
    });

    it('should call toggleProductStatus on confirm and success', () => {
      vi.spyOn(window, 'confirm').mockReturnValue(true);
      mockProductService.toggleProductStatus.mockReturnValue(of({}));

      component.toggleProductStatus(testProduct);

      expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to deactivate this product listing?');
      expect(mockProductService.toggleProductStatus).toHaveBeenCalledWith(101);
      expect(mockToastService.success).toHaveBeenCalledWith('Product has been successfully deactivated.');
    });

    it('should call toggleProductStatus for archived product (activate action)', () => {
      testProduct.status = 'Archived';
      vi.spyOn(window, 'confirm').mockReturnValue(true);
      mockProductService.toggleProductStatus.mockReturnValue(of({}));

      component.toggleProductStatus(testProduct);

      expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to activate this product listing?');
      expect(mockToastService.success).toHaveBeenCalledWith('Product has been successfully activated.');
    });

    it('should handle error when toggleProductStatus fails', () => {
      vi.spyOn(window, 'confirm').mockReturnValue(true);
      mockProductService.toggleProductStatus.mockReturnValue(throwError(() => new Error('Toggle error')));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.toggleProductStatus(testProduct);

      expect(spyConsole).toHaveBeenCalled();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to deactivate product listing.');
    });

    it('should do nothing if confirm returns false', () => {
      vi.spyOn(window, 'confirm').mockReturnValue(false);

      component.toggleProductStatus(testProduct);

      expect(mockProductService.toggleProductStatus).not.toHaveBeenCalled();
    });
  });
});
