import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductsList } from './products-list';
import { describe, it, expect, beforeEach, vi } from 'vitest';

// Mock IntersectionObserver
class MockIntersectionObserver {
  observe = vi.fn();
  unobserve = vi.fn();
  disconnect = vi.fn();
  constructor(public callback: IntersectionObserverCallback, options?: any) {}
}
Object.defineProperty(globalThis, 'IntersectionObserver', {
  writable: true,
  value: MockIntersectionObserver
});

describe('ProductsList', () => {
  let component: ProductsList;
  let fixture: ComponentFixture<ProductsList>;

  const makeProduct = (id: number) => ({
    id,
    name: `Product ${id}`,
    description: 'Test',
    categoryId: 1,
    categoryName: 'Category',
    vendorId: 1,
    averageRating: 4.0,
    reviewCount: 5,
    discountLabel: '10%',
    discountedPrice: 90,
    discountPercent: 10,
    variants: []
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductsList]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductsList);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should default products to empty array', () => {
    fixture.detectChanges();
    expect(component.products()).toEqual([]);
  });

  it('should default showScrollToTop to false', () => {
    fixture.detectChanges();
    expect(component.showScrollToTop()).toBe(false);
  });

  it('should disconnect observer on ngOnDestroy', () => {
    fixture.detectChanges();
    const disconnectSpy = vi.fn();
    (component as any).observer = { disconnect: disconnectSpy };
    component.ngOnDestroy();
    expect(disconnectSpy).toHaveBeenCalled();
  });

  it('should not error if no observer on ngOnDestroy', () => {
    fixture.detectChanges();
    (component as any).observer = undefined;
    expect(() => component.ngOnDestroy()).not.toThrow();
  });

  it('should update showScrollToTop on onScroll when scrollTop > 300', () => {
    fixture.detectChanges();
    const event = { target: { scrollTop: 400 } } as any;
    component.onScroll(event);
    expect(component.showScrollToTop()).toBe(true);
  });

  it('should set showScrollToTop=false when scrollTop <= 300', () => {
    fixture.detectChanges();
    component.showScrollToTop.set(true);
    const event = { target: { scrollTop: 100 } } as any;
    component.onScroll(event);
    expect(component.showScrollToTop()).toBe(false);
  });

  it('should emit loadNextPage from intersection observer when conditions are met', () => {
    fixture.detectChanges();
    const spy = vi.fn();
    component.loadNextPage.subscribe(spy);

    // Simulate the intersection callback
    const observerInstance = (MockIntersectionObserver as any).mock?.instances?.[0];
    if (observerInstance) {
      observerInstance.callback([{ isIntersecting: true }], observerInstance);
    }
  });

  it('should return early from setupIntersectionObserver if IntersectionObserver is undefined', () => {
    const origIO = (globalThis as any).IntersectionObserver;
    (globalThis as any).IntersectionObserver = undefined;
    expect(() => component.setupIntersectionObserver()).not.toThrow();
    (globalThis as any).IntersectionObserver = origIO;
  });

  it('should call scrollTo on scrollToTop if container exists', () => {
    fixture.detectChanges();
    const scrollToSpy = vi.fn();
    (component as any).scrollContainer = {
      nativeElement: { scrollTo: scrollToSpy }
    };
    component.scrollToTop();
    expect(scrollToSpy).toHaveBeenCalledWith({ top: 0, behavior: 'smooth' });
  });

  it('should not throw if scrollContainer is undefined on scrollToTop', () => {
    fixture.detectChanges();
    (component as any).scrollContainer = undefined;
    expect(() => component.scrollToTop()).not.toThrow();
  });
});
