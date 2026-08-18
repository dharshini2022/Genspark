import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AddProduct } from './add-product';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Category } from '../../../models/category.model';
import { ProductVariantResponse } from '../../../models/product.model';

describe('AddProduct', () => {
  let component: AddProduct;
  let fixture: ComponentFixture<AddProduct>;
  let mockProductService: any;
  let mockCategoryService: any;
  let router: Router;
  let navigateSpy: any;

  beforeEach(async () => {
    mockProductService = {
      createProduct: vi.fn(),
      publishProduct: vi.fn()
    };

    mockCategoryService = {
      getCategories: vi.fn().mockReturnValue(of([]))
    };

    await TestBed.configureTestingModule({
      imports: [AddProduct],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddProduct);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    navigateSpy = vi.spyOn(router, 'navigate').mockImplementation(async () => true);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  describe('ngOnInit / loadCategories', () => {
    it('should load categories and select the first category if response is not empty', () => {
      const mockCats: Category[] = [
        { id: 10, name: 'Electronics', slug: 'electronics' },
        { id: 20, name: 'Clothing', slug: 'clothing' }
      ];
      mockCategoryService.getCategories.mockReturnValue(of(mockCats));

      fixture.detectChanges(); // triggers ngOnInit -> loadCategories

      expect(component.categories()).toEqual(mockCats);
      expect(component.categoryId()).toBe(10);
    });

    it('should handle empty categories response', () => {
      mockCategoryService.getCategories.mockReturnValue(of([]));

      fixture.detectChanges();

      expect(component.categories()).toEqual([]);
      expect(component.categoryId()).toBeNull();
    });

    it('should set errorMsg when getCategories fails', () => {
      const err = new Error('Network error');
      mockCategoryService.getCategories.mockReturnValue(throwError(() => err));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      fixture.detectChanges();

      expect(spyConsole).toHaveBeenCalledWith('Error loading categories', err);
      expect(component.errorMsg()).toBe('Failed to load product categories.');
    });
  });

  describe('saveProduct', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should show error if product name is empty or only whitespace', () => {
      component.productName.set('   ');
      component.saveProduct();
      expect(component.errorMsg()).toBe('Product name is required.');

      component.productName.set('');
      component.saveProduct();
      expect(component.errorMsg()).toBe('Product name is required.');
    });

    it('should show error if categoryId is not selected', () => {
      component.productName.set('Test Product');
      component.categoryId.set(null);
      component.saveProduct();
      expect(component.errorMsg()).toBe('Please select a product category.');
    });

    it('should save product details successfully', () => {
      component.productName.set('Test Product');
      component.description.set('Test Description');
      component.categoryId.set(10);
      mockProductService.createProduct.mockReturnValue(of({ data: { id: 42 } }));

      component.saveProduct();

      expect(component.savingProduct()).toBe(false);
      expect(component.createdProductId()).toBe(42);
      expect(component.successMsg()).toBe('Product details saved successfully. You can now add variants.');
      expect(mockProductService.createProduct).toHaveBeenCalledWith({
        name: 'Test Product',
        description: 'Test Description',
        categoryId: 10
      });
    });

    it('should handle product creation error with error message from backend', () => {
      component.productName.set('Test Product');
      component.categoryId.set(10);
      const backendError = { error: { message: 'Duplicate name' } };
      mockProductService.createProduct.mockReturnValue(throwError(() => backendError));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveProduct();

      expect(spyConsole).toHaveBeenCalledWith('Error creating product', backendError);
      expect(component.savingProduct()).toBe(false);
      expect(component.errorMsg()).toBe('Duplicate name');
    });

    it('should handle product creation error with default fallback message', () => {
      component.productName.set('Test Product');
      component.categoryId.set(10);
      mockProductService.createProduct.mockReturnValue(throwError(() => ({})));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveProduct();

      expect(spyConsole).toHaveBeenCalledWith('Error creating product', {});
      expect(component.savingProduct()).toBe(false);
      expect(component.errorMsg()).toBe('Failed to create product. Please try again.');
    });
  });

  describe('Variant Form Controls', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should open variant form when openAddVariant is called', () => {
      component.showAddVariantForm.set(false);
      component.openAddVariant();
      expect(component.showAddVariantForm()).toBe(true);
    });

    it('should close variant form and append variant to list when onVariantSaved is called', () => {
      const mockVariant = { id: 101, stockQty: 10, price: 99.99 } as ProductVariantResponse;
      component.variantsAdded.set([]);
      component.showAddVariantForm.set(true);

      component.onVariantSaved(mockVariant);

      expect(component.variantsAdded()).toEqual([mockVariant]);
      expect(component.showAddVariantForm()).toBe(false);
      expect(component.successMsg()).toBe('Variant #1 added successfully!');
    });

    it('should close variant form when onVariantDiscarded is called', () => {
      component.showAddVariantForm.set(true);
      component.onVariantDiscarded();
      expect(component.showAddVariantForm()).toBe(false);
    });
  });

  describe('publishProduct', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should do nothing if createdProductId is null', () => {
      component.createdProductId.set(null);
      component.publishProduct();
      expect(mockProductService.publishProduct).not.toHaveBeenCalled();
    });

    it('should set errorMsg if no variants are added', () => {
      component.createdProductId.set(42);
      component.variantsAdded.set([]);
      component.publishProduct();
      expect(component.errorMsg()).toBe('You must add at least one variant before publishing.');
      expect(mockProductService.publishProduct).not.toHaveBeenCalled();
    });

    it('should publish successfully and navigate to products list', () => {
      component.createdProductId.set(42);
      component.variantsAdded.set([{ id: 101 } as ProductVariantResponse]);
      mockProductService.publishProduct.mockReturnValue(of({}));

      component.publishProduct();

      expect(component.publishing()).toBe(false);
      expect(navigateSpy).toHaveBeenCalledWith(['/vendor-home/products-list']);
    });

    it('should handle publishing error', () => {
      component.createdProductId.set(42);
      component.variantsAdded.set([{ id: 101 } as ProductVariantResponse]);
      const backendError = { error: { message: 'Publishing forbidden' } };
      mockProductService.publishProduct.mockReturnValue(throwError(() => backendError));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.publishProduct();

      expect(spyConsole).toHaveBeenCalledWith('Error publishing product', backendError);
      expect(component.publishing()).toBe(false);
      expect(component.errorMsg()).toBe('Publishing forbidden');
    });

    it('should handle publishing error with default fallback message', () => {
      component.createdProductId.set(42);
      component.variantsAdded.set([{ id: 101 } as ProductVariantResponse]);
      mockProductService.publishProduct.mockReturnValue(throwError(() => ({})));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.publishProduct();

      expect(spyConsole).toHaveBeenCalledWith('Error publishing product', {});
      expect(component.publishing()).toBe(false);
      expect(component.errorMsg()).toBe('Failed to publish product. Please try again.');
    });
  });

  describe('goBack', () => {
    it('should navigate to products-list', () => {
      fixture.detectChanges();
      component.goBack();
      expect(navigateSpy).toHaveBeenCalledWith(['/vendor-home/products-list']);
    });
  });
});
