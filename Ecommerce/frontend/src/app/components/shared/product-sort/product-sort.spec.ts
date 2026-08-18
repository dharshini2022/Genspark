import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductSort } from './product-sort';
import { RouterModule } from '@angular/router';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('ProductSort', () => {
  let component: ProductSort;
  let fixture: ComponentFixture<ProductSort>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductSort, RouterModule.forRoot([])]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductSort);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit sortChanged with sortBy and sortOrder when onSortClick is called', () => {
    const spy = vi.fn();
    component.sortChanged.subscribe(spy);
    component.onSortClick('price', 'asc');
    expect(spy).toHaveBeenCalledWith({ sortBy: 'price', sortOrder: 'asc' });
  });

  it('should emit sortChanged with different values', () => {
    const spy = vi.fn();
    component.sortChanged.subscribe(spy);
    component.onSortClick('rating', 'desc');
    expect(spy).toHaveBeenCalledWith({ sortBy: 'rating', sortOrder: 'desc' });
  });

  it('should emit sortChanged for discount sort', () => {
    const spy = vi.fn();
    component.sortChanged.subscribe(spy);
    component.onSortClick('discount', 'desc');
    expect(spy).toHaveBeenCalledWith({ sortBy: 'discount', sortOrder: 'desc' });
  });

  it('should emit sortChanged for newest sort', () => {
    const spy = vi.fn();
    component.sortChanged.subscribe(spy);
    component.onSortClick('newest', 'desc');
    expect(spy).toHaveBeenCalledWith({ sortBy: 'newest', sortOrder: 'desc' });
  });

  it('should default totalProducts to 0', () => {
    expect(component.totalProducts()).toBe(0);
  });

  it('should default currentSort to newest', () => {
    expect(component.currentSort()).toBe('newest');
  });
});
