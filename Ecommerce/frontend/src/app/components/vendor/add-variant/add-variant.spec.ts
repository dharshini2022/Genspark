import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AddVariant } from './add-variant';
import { ProductService } from '../../../services/product.service';
import { of, throwError } from 'rxjs';
import { ProductVariantResponse } from '../../../models/product.model';
import * as rxjs from 'rxjs';

describe('AddVariant', () => {
  let component: AddVariant;
  let fixture: ComponentFixture<AddVariant>;
  let mockProductService: any;

  beforeEach(async () => {
    mockProductService = {
      addVariant: vi.fn(),
      updateVariant: vi.fn(),
      deleteVariantImage: vi.fn(),
      addVariantImage: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [AddVariant],
      providers: [
        { provide: ProductService, useValue: mockProductService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AddVariant);
    component = fixture.componentInstance;
    component.productId = 1;
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('should create with defaults when variantToEdit is not provided', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(component.stockQty()).toBe(100);
    expect(component.price()).toBeNull();
    expect(component.isDefault()).toBe(false);
    expect(component.features()).toEqual([{ key: 'color', value: '' }]);
    expect(component.images()).toEqual([{ url: '', order: 1 }]);
  });

  it('should initialize fields from variantToEdit on ngOnInit', () => {
    component.variantToEdit = {
      id: 123,
      stockQty: 50,
      price: 49.99,
      isDefault: true,
      availableValues: { size: 'M', material: 'Cotton' },
      variantImages: [
        { id: 1, imageUrl: 'img1.jpg', imageOrder: 1, variantId: 123 },
        { id: 2, imageUrl: 'img2.jpg', imageOrder: 2, variantId: 123 }
      ]
    } as any;

    fixture.detectChanges();

    expect(component.stockQty()).toBe(50);
    expect(component.price()).toBe(49.99);
    expect(component.isDefault()).toBe(true);
    expect(component.features()).toEqual([
      { key: 'size', value: 'M' },
      { key: 'material', value: 'Cotton' }
    ]);
    expect(component.images()).toEqual([
      { url: 'img1.jpg', order: 1 },
      { url: 'img2.jpg', order: 2 }
    ]);
  });

  it('should fallback features and images if variantToEdit properties are empty', () => {
    component.variantToEdit = {
      id: 123,
      stockQty: 50,
      price: 49.99,
      isDefault: true,
      availableValues: {},
      variantImages: []
    } as any;

    fixture.detectChanges();

    expect(component.features()).toEqual([{ key: 'color', value: '' }]);
    expect(component.images()).toEqual([{ url: '', order: 1 }]);
  });

  it('should set default state via setDefault', () => {
    fixture.detectChanges();
    component.setDefault(true);
    expect(component.isDefault()).toBe(true);
  });

  it('should add and remove feature pairs', () => {
    fixture.detectChanges();
    component.addFeaturePair();
    expect(component.features().length).toBe(2);

    component.removeFeaturePair(1);
    expect(component.features().length).toBe(1);

    component.removeFeaturePair(0); // If length reaches 0, it reset to default
    expect(component.features()).toEqual([{ key: '', value: '' }]);
  });

  it('should add and remove image rows', () => {
    fixture.detectChanges();
    component.addImageRow();
    expect(component.images().length).toBe(2);
    expect(component.images()[1].order).toBe(2);

    component.removeImageRow(1);
    expect(component.images().length).toBe(1);

    component.removeImageRow(0); // If length reaches 0, it reset to default
    expect(component.images()).toEqual([{ url: '', order: 1 }]);
  });

  describe('saveVariant Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should set errorMsg if price is null or <= 0', () => {
      component.price.set(null);
      component.saveVariant();
      expect(component.errorMsg()).toBe('Price must be greater than 0.');

      component.price.set(0);
      component.saveVariant();
      expect(component.errorMsg()).toBe('Price must be greater than 0.');

      component.price.set(-5);
      component.saveVariant();
      expect(component.errorMsg()).toBe('Price must be greater than 0.');
    });

    it('should set errorMsg if stockQty is negative', () => {
      component.price.set(100);
      component.stockQty.set(-1);
      component.saveVariant();
      expect(component.errorMsg()).toBe('Stock quantity cannot be negative.');
    });
  });

  describe('saveVariant execution', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should add variant successfully and upload images when variantToEdit is null', () => {
      component.price.set(100);
      component.stockQty.set(10);
      component.isDefault.set(true);
      component.features.set([
        { key: 'color', value: 'red' },
        { key: 'size', value: 'L' },
        { key: '  ', value: 'ignored' } // empty key should be filtered out
      ]);
      component.images.set([
        { url: 'img1.jpg', order: 1 },
        { url: '  ', order: 2 } // empty url should be filtered out
      ]);

      const mockNewVariant = { id: 999, stockQty: 10, price: 100 } as ProductVariantResponse;
      mockProductService.addVariant.mockReturnValue(of({ data: mockNewVariant }));
      mockProductService.addVariantImage.mockReturnValue(of({}));

      let savedEmit: any = null;
      component.variantSaved.subscribe(val => savedEmit = val);

      component.saveVariant();

      expect(mockProductService.addVariant).toHaveBeenCalledWith(1, {
        stockQty: 10,
        price: 100,
        isDefault: true,
        availableValues: { color: 'red', size: 'L' }
      });
      expect(mockProductService.addVariantImage).toHaveBeenCalledWith(999, {
        imageUrl: 'img1.jpg',
        imageOrder: 1
      });
      expect(component.saving()).toBe(false);
      expect(savedEmit).toEqual(mockNewVariant);
    });

    it('should handle addVariant failure', () => {
      component.price.set(100);
      const backendError = { error: { message: 'Invalid variant payload' } };
      mockProductService.addVariant.mockReturnValue(throwError(() => backendError));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveVariant();

      expect(spyConsole).toHaveBeenCalledWith('Error adding variant', backendError);
      expect(component.saving()).toBe(false);
      expect(component.errorMsg()).toBe('Invalid variant payload');
    });

    it('should handle addVariant failure with fallback error message', () => {
      component.price.set(100);
      mockProductService.addVariant.mockReturnValue(throwError(() => ({})));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveVariant();

      expect(spyConsole).toHaveBeenCalledWith('Error adding variant', {});
      expect(component.saving()).toBe(false);
      expect(component.errorMsg()).toBe('Failed to add variant. Please try again.');
    });

    it('should update variant successfully and handle image deletes and uploads when variantToEdit is provided', () => {
      component.variantToEdit = {
        id: 123,
        stockQty: 50,
        price: 49.99,
        isDefault: false,
        availableValues: {},
        variantImages: [
          { id: 10, imageUrl: 'old1.jpg', imageOrder: 1, variantId: 123 },
          { id: 11, imageUrl: 'old2.jpg', imageOrder: 2, variantId: 123 }
        ]
      } as any;
      component.ngOnInit(); // reload variantToEdit info

      component.price.set(150);
      component.images.set([{ url: 'new1.jpg', order: 1 }]);

      const mockUpdatedVariant = { id: 123, stockQty: 50, price: 150 } as ProductVariantResponse;
      mockProductService.updateVariant.mockReturnValue(of({ data: mockUpdatedVariant }));
      mockProductService.deleteVariantImage.mockReturnValue(of({}));
      mockProductService.addVariantImage.mockReturnValue(of({}));

      let savedEmit: any = null;
      component.variantSaved.subscribe(val => savedEmit = val);

      component.saveVariant();

      expect(mockProductService.updateVariant).toHaveBeenCalledWith(123, {
        stockQty: 50,
        price: 150,
        isDefault: false,
        availableValues: {}
      });
      expect(mockProductService.deleteVariantImage).toHaveBeenCalledWith(10);
      expect(mockProductService.deleteVariantImage).toHaveBeenCalledWith(11);
      expect(mockProductService.addVariantImage).toHaveBeenCalledWith(123, {
        imageUrl: 'new1.jpg',
        imageOrder: 1
      });
      expect(component.saving()).toBe(false);
      expect(savedEmit).toEqual(mockUpdatedVariant);
    });

    it('should update variant successfully and directly upload images if old variant has no images', () => {
      component.variantToEdit = {
        id: 123,
        stockQty: 50,
        price: 49.99,
        isDefault: false,
        availableValues: {},
        variantImages: []
      } as any;
      component.ngOnInit();

      component.price.set(150);
      component.images.set([{ url: 'new1.jpg', order: 1 }]);

      const mockUpdatedVariant = { id: 123, stockQty: 50, price: 150 } as ProductVariantResponse;
      mockProductService.updateVariant.mockReturnValue(of({ data: mockUpdatedVariant }));
      mockProductService.addVariantImage.mockReturnValue(of({}));

      let savedEmit: any = null;
      component.variantSaved.subscribe(val => savedEmit = val);

      component.saveVariant();

      expect(mockProductService.updateVariant).toHaveBeenCalled();
      expect(mockProductService.deleteVariantImage).not.toHaveBeenCalled();
      expect(mockProductService.addVariantImage).toHaveBeenCalledWith(123, {
        imageUrl: 'new1.jpg',
        imageOrder: 1
      });
      expect(savedEmit).toEqual(mockUpdatedVariant);
    });

    it('should handle error when deleting old images but still upload new ones', () => {
      component.variantToEdit = {
        id: 123,
        stockQty: 50,
        price: 49.99,
        isDefault: false,
        availableValues: {},
        variantImages: [
          { id: 10, imageUrl: 'old1.jpg', imageOrder: 1, variantId: 123 }
        ]
      } as any;
      component.ngOnInit();

      component.price.set(150);
      component.images.set([{ url: 'new1.jpg', order: 1 }]);

      const mockUpdatedVariant = { id: 123, stockQty: 50, price: 150 } as ProductVariantResponse;
      mockProductService.updateVariant.mockReturnValue(of({ data: mockUpdatedVariant }));
      mockProductService.deleteVariantImage.mockReturnValue(throwError(() => new Error('Delete failed')));
      mockProductService.addVariantImage.mockReturnValue(of({}));

      let savedEmit: any = null;
      component.variantSaved.subscribe(val => savedEmit = val);
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveVariant();

      expect(spyConsole).toHaveBeenCalled();
      expect(mockProductService.addVariantImage).toHaveBeenCalledWith(123, {
        imageUrl: 'new1.jpg',
        imageOrder: 1
      });
      expect(savedEmit).toEqual(mockUpdatedVariant);
    });

    it('should handle error when uploading new images but still complete saving process', () => {
      component.price.set(100);
      component.images.set([{ url: 'new1.jpg', order: 1 }]);

      const mockNewVariant = { id: 999, stockQty: 100, price: 100 } as ProductVariantResponse;
      mockProductService.addVariant.mockReturnValue(of({ data: mockNewVariant }));
      mockProductService.addVariantImage.mockReturnValue(throwError(() => new Error('Upload failed')));

      let savedEmit: any = null;
      component.variantSaved.subscribe(val => savedEmit = val);
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveVariant();

      expect(spyConsole).toHaveBeenCalled();
      expect(component.saving()).toBe(false);
      expect(savedEmit).toEqual(mockNewVariant);
    });

    it('should handle error when deleting old images forkJoin fails', () => {
      component.variantToEdit = {
        id: 123,
        stockQty: 50,
        price: 49.99,
        isDefault: false,
        availableValues: {},
        variantImages: [
          { id: 10, imageUrl: 'old1.jpg', imageOrder: 1, variantId: 123 }
        ]
      } as any;
      component.ngOnInit();

      component.price.set(150);
      component.images.set([{ url: 'new1.jpg', order: 1 }]);

      const mockUpdatedVariant = { id: 123, stockQty: 50, price: 150 } as ProductVariantResponse;
      mockProductService.updateVariant.mockReturnValue(of({ data: mockUpdatedVariant }));
      mockProductService.deleteVariantImage.mockReturnValue(of({}));
      mockProductService.addVariantImage.mockReturnValue(of({}));
      
      const forkJoinSpy = vi.spyOn(rxjs, 'forkJoin').mockReturnValue(throwError(() => new Error('forkJoin failed')));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveVariant();

      expect(spyConsole).toHaveBeenCalled();
      forkJoinSpy.mockRestore();
    });

    it('should handle error when uploading new images forkJoin fails', () => {
      component.price.set(100);
      component.images.set([{ url: 'new1.jpg', order: 1 }]);

      const mockNewVariant = { id: 999, stockQty: 100, price: 100 } as ProductVariantResponse;
      mockProductService.addVariant.mockReturnValue(of({ data: mockNewVariant }));
      mockProductService.addVariantImage.mockReturnValue(of({}));
      
      const forkJoinSpy = vi.spyOn(rxjs, 'forkJoin').mockReturnValue(throwError(() => new Error('forkJoin failed')));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveVariant();

      expect(spyConsole).toHaveBeenCalled();
      expect(component.saving()).toBe(false);
      forkJoinSpy.mockRestore();
    });

    it('should emit saved event even if no new images are provided', () => {
      component.price.set(100);
      component.images.set([]); // empty

      const mockNewVariant = { id: 999, stockQty: 100, price: 100 } as ProductVariantResponse;
      mockProductService.addVariant.mockReturnValue(of({ data: mockNewVariant }));

      let savedEmit: any = null;
      component.variantSaved.subscribe(val => savedEmit = val);

      component.saveVariant();

      expect(savedEmit).toEqual(mockNewVariant);
      expect(component.saving()).toBe(false);
    });

    it('should handle updateVariant failure', () => {
      component.variantToEdit = { id: 123 } as any;
      component.ngOnInit();
      component.price.set(100);

      const backendError = { error: { message: 'Update failed' } };
      mockProductService.updateVariant.mockReturnValue(throwError(() => backendError));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.saveVariant();

      expect(spyConsole).toHaveBeenCalledWith('Error updating variant', backendError);
      expect(component.saving()).toBe(false);
      expect(component.errorMsg()).toBe('Update failed');
    });
  });

  describe('discard', () => {
    it('should emit variantDiscarded event', () => {
      fixture.detectChanges();
      let discarded = false;
      component.variantDiscarded.subscribe(() => discarded = true);
      component.discard();
      expect(discarded).toBe(true);
    });
  });
});
