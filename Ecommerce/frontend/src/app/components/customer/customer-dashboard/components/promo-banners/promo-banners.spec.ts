import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PromoBannersComponent } from './promo-banners';
import { Router } from '@angular/router';

describe('PromoBannersComponent', () => {
  let component: PromoBannersComponent;
  let fixture: ComponentFixture<PromoBannersComponent>;
  let mockRouter: any;

  beforeEach(async () => {
    mockRouter = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [PromoBannersComponent],
      providers: [
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PromoBannersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should navigate to products-list with query parameters on CTA click (with sortOrder)', () => {
    component.onCtaClick('discount', 'desc');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
      queryParams: { sortBy: 'discount', sortOrder: 'desc' }
    });
  });

  it('should navigate to products-list with query parameters on CTA click (without sortOrder)', () => {
    component.onCtaClick('rating');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
      queryParams: { sortBy: 'rating' }
    });
  });
});
