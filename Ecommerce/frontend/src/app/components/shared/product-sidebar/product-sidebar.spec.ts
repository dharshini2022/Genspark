import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductSidebar } from './product-sidebar';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('ProductSidebar', () => {
  let component: ProductSidebar;
  let fixture: ComponentFixture<ProductSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductSidebar]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductSidebar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default localMinPrice of 0 and localMaxPrice of 1000000', () => {
    expect(component.localMinPrice()).toBe(0);
    expect(component.localMaxPrice()).toBe(1000000);
  });

  it('should set localSelectedCategoryId on onCategoryClick', () => {
    component.onCategoryClick(5);
    expect(component.localSelectedCategoryId()).toBe(5);
  });

  describe('onMinPriceInput', () => {
    it('should set localMinPrice from input event', () => {
      const event = { target: { value: '2000' } } as any;
      component.onMinPriceInput(event);
      expect(component.localMinPrice()).toBe(2000);
    });

    it('should cap localMinPrice to localMaxPrice if val > localMaxPrice', () => {
      component.localMaxPrice.set(5000);
      const event = { target: { value: '8000' } } as any;
      component.onMinPriceInput(event);
      expect(component.localMinPrice()).toBe(5000);
    });
  });

  describe('onMaxPriceInput', () => {
    it('should set localMaxPrice from input event', () => {
      const event = { target: { value: '50000' } } as any;
      component.onMaxPriceInput(event);
      expect(component.localMaxPrice()).toBe(50000);
    });

    it('should cap localMaxPrice to localMinPrice if val < localMinPrice', () => {
      component.localMinPrice.set(5000);
      const event = { target: { value: '1000' } } as any;
      component.onMaxPriceInput(event);
      expect(component.localMaxPrice()).toBe(5000);
    });
  });

  describe('onMinSelectChange', () => {
    it('should set localMinPrice from select event', () => {
      const event = { target: { value: '3000' } } as any;
      component.onMinSelectChange(event);
      expect(component.localMinPrice()).toBe(3000);
    });

    it('should cap to localMaxPrice if val > max', () => {
      component.localMaxPrice.set(2000);
      const event = { target: { value: '5000' } } as any;
      component.onMinSelectChange(event);
      expect(component.localMinPrice()).toBe(2000);
    });
  });

  describe('onMaxSelectChange', () => {
    it('should set localMaxPrice from select event', () => {
      const event = { target: { value: '20000' } } as any;
      component.onMaxSelectChange(event);
      expect(component.localMaxPrice()).toBe(20000);
    });

    it('should cap to localMinPrice if val < min', () => {
      component.localMinPrice.set(10000);
      const event = { target: { value: '1000' } } as any;
      component.onMaxSelectChange(event);
      expect(component.localMaxPrice()).toBe(10000);
    });
  });

  it('should reset prices on onClearPrice', () => {
    component.localMinPrice.set(5000);
    component.localMaxPrice.set(50000);
    component.onClearPrice();
    expect(component.localMinPrice()).toBe(0);
    expect(component.localMaxPrice()).toBe(1000000);
  });

  it('should emit applyFilters with current values on onApplyClick', () => {
    const spy = vi.fn();
    component.applyFilters.subscribe(spy);
    component.localSelectedCategoryId.set(3);
    component.localMinPrice.set(1000);
    component.localMaxPrice.set(50000);
    component.onApplyClick();
    expect(spy).toHaveBeenCalledWith({ categoryId: 3, minPrice: 1000, maxPrice: 50000 });
  });

  describe('computed getMinPriceOptions', () => {
    it('should include value in list even if not in defaults', () => {
      component.localMinPrice.set(1500);
      const opts = component.getMinPriceOptions();
      expect(opts).toContain(1500);
    });
  });

  describe('computed getMaxPriceOptions', () => {
    it('should include value in list even if not in defaults', () => {
      component.localMaxPrice.set(750000);
      const opts = component.getMaxPriceOptions();
      expect(opts).toContain(750000);
    });
  });

  describe('computed minPercent and maxPercent', () => {
    it('should compute minPercent correctly', () => {
      component.localMinPrice.set(500000);
      expect(component.minPercent()).toBe(50);
    });

    it('should compute maxPercent correctly', () => {
      component.localMaxPrice.set(500000);
      expect(component.maxPercent()).toBe(50);
    });

    it('should compute minPercent = 0 for price 0', () => {
      component.localMinPrice.set(0);
      expect(component.minPercent()).toBe(0);
    });

    it('should compute maxPercent = 0 for price 1000000', () => {
      component.localMaxPrice.set(1000000);
      expect(component.maxPercent()).toBe(0);
    });
  });
});
