import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CategoryListComponent } from './category-list';
import { CategoryService } from '../../../../../services/category.service';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('CategoryListComponent', () => {
  let component: CategoryListComponent;
  let fixture: ComponentFixture<CategoryListComponent>;
  let mockCategoryService: any;
  let mockRouter: any;

  const mockCategories = [
    { id: 1, name: 'Electronics', parentId: null },
    { id: 2, name: 'Laptops & PCs', parentId: null },
    { id: 3, name: 'Mobile Phones', parentId: null },
    { id: 4, name: 'Television & TVs', parentId: null },
    { id: 5, name: 'Audio Sound', parentId: null },
    { id: 6, name: 'Fashion Clothing', parentId: null },
    { id: 7, name: 'Home Living', parentId: null },
    { id: 8, name: 'Books', parentId: null },
    { id: 9, name: 'Sports Fitness', parentId: null },
    { id: 10, name: 'Beauty Cosmetics', parentId: null },
    { id: 11, name: 'Grocery Food', parentId: null },
    { id: 12, name: 'Toys Games', parentId: null },
    { id: 13, name: 'Other', parentId: null }
  ];

  beforeEach(async () => {
    mockCategoryService = {
      getCategories: vi.fn().mockReturnValue(of(mockCategories))
    };

    mockRouter = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [CategoryListComponent],
      providers: [
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load categories', () => {
    expect(component).toBeTruthy();
    expect(mockCategoryService.getCategories).toHaveBeenCalled();
    expect(component.categories().length).toBe(13);
    expect(component.loading()).toBe(false);
  });

  it('should handle category load error', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockCategoryService.getCategories.mockReturnValue(throwError(() => new Error('API Error')));
    component.ngOnInit();
    expect(component.loading()).toBe(false);
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should get correct category meta configurations', () => {
    // Electronics
    let meta = component.getCategoryMeta('Electronics');
    expect(meta.icon).toBe('bi-lightning-charge');
    expect(meta.bgClass).toBe('bg-electronics');

    // Laptop
    meta = component.getCategoryMeta('Laptops & PCs');
    expect(meta.icon).toBe('bi-laptop');

    // Mobile
    meta = component.getCategoryMeta('Mobile Phones');
    expect(meta.icon).toBe('bi-phone');

    // TV
    meta = component.getCategoryMeta('Television & TVs');
    expect(meta.icon).toBe('bi-tv');

    // Audio
    meta = component.getCategoryMeta('Audio Sound');
    expect(meta.icon).toBe('bi-headphones');

    // Fashion
    meta = component.getCategoryMeta('Fashion Clothing');
    expect(meta.icon).toBe('bi-handbag');

    // Home
    meta = component.getCategoryMeta('Home Living');
    expect(meta.icon).toBe('bi-house');

    // Books
    meta = component.getCategoryMeta('Books');
    expect(meta.icon).toBe('bi-book');

    // Sports
    meta = component.getCategoryMeta('Sports Fitness');
    expect(meta.icon).toBe('bi-award');

    // Beauty
    meta = component.getCategoryMeta('Beauty Cosmetics');
    expect(meta.icon).toBe('bi-sparkles');

    // Grocery
    meta = component.getCategoryMeta('Grocery Food');
    expect(meta.icon).toBe('bi-basket');

    // Toys
    meta = component.getCategoryMeta('Toys Games');
    expect(meta.icon).toBe('bi-gift');

    // Other
    meta = component.getCategoryMeta('Other');
    expect(meta.icon).toBe('bi-tag');
    expect(meta.bgClass).toBe('bg-default');
  });

  it('should navigate to products list when category is selected', () => {
    component.selectCategory(5);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
      queryParams: { categoryId: 5 }
    });
  });

  describe('HTML Template rendering and actions', () => {
    it('should render skeleton container when loading is true', () => {
      component.loading.set(true);
      fixture.detectChanges();

      const skeleton = fixture.debugElement.nativeElement.querySelector('.skeleton-container');
      expect(skeleton).toBeTruthy();
    });

    it('should render category grid and execute all switch cases in template', () => {
      component.loading.set(false);
      component.categories.set([
        { id: 101, name: 'laptop', slug: 'laptop', parentId: undefined },
        { id: 102, name: 'mobile', slug: 'mobile', parentId: undefined },
        { id: 103, name: 'tv', slug: 'tv', parentId: undefined },
        { id: 104, name: 'audio devices', slug: 'audio-devices', parentId: undefined },
        { id: 105, name: 'home decor', slug: 'home-decor', parentId: undefined },
        { id: 106, name: 'books', slug: 'books', parentId: undefined }
      ]);
      fixture.detectChanges();

      const cards = fixture.debugElement.nativeElement.querySelectorAll('.category-card');
      expect(cards.length).toBe(6);

      // Trigger click on first card
      cards[0].click();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
        queryParams: { categoryId: 101 }
      });
    });
  });
});
