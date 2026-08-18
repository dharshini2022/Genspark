import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivityPromosComponent } from './activity-promos';
import { CartService } from '../../../../../services/cart.service';
import { AuthService } from '../../../../../services/auth.service';
import { Router } from '@angular/router';
import { signal } from '@angular/core';

describe('ActivityPromosComponent', () => {
  let component: ActivityPromosComponent;
  let fixture: ComponentFixture<ActivityPromosComponent>;
  let mockCartService: any;
  let mockAuthService: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockCartService = {
      cartCountSignal: signal(5),
      updateCartCount: vi.fn()
    };

    mockAuthService = {};

    mockRouter = {
      navigate: vi.fn()
    };

    // Clean up sessionStorage
    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [ActivityPromosComponent],
      providers: [
        { provide: CartService, useValue: mockCartService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ActivityPromosComponent);
    component = fixture.componentInstance;
  });

  it('should create and not update cart count if not logged in', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(component.cartCount()).toBe(5);
    expect(mockCartService.updateCartCount).not.toHaveBeenCalled();
  });

  it('should update cart count if logged in on init', () => {
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
    fixture.detectChanges();
    expect(mockCartService.updateCartCount).toHaveBeenCalled();
  });

  it('should check isLoggedIn correctly', () => {
    expect(component.isLoggedIn()).toBe(false);
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
    expect(component.isLoggedIn()).toBe(true);
  });

  it('should navigate to cart if logged in when calling viewCart', () => {
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
    component.viewCart();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/cart']);
  });

  it('should navigate to login if not logged in when calling viewCart', () => {
    component.viewCart();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should navigate to discounts', () => {
    component.viewDiscounts();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
      queryParams: { sortBy: 'discount' }
    });
  });

  describe('HTML Template rendering and actions', () => {
    it('should render plural items waiting description if cartCount > 1 and logged in', () => {
      sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
      mockCartService.cartCountSignal.set(3);
      fixture.detectChanges();

      const desc = fixture.debugElement.nativeElement.querySelector('.box-description');
      expect(desc.textContent).toContain('You have 3 items waiting in your cart.');
    });

    it('should render singular item waiting description if cartCount === 1 and logged in', () => {
      sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
      mockCartService.cartCountSignal.set(1);
      fixture.detectChanges();

      const desc = fixture.debugElement.nativeElement.querySelector('.box-description');
      expect(desc.textContent).toContain('You have 1 item waiting in your cart.');
    });

    it('should trigger viewCart on clicking shopping-card in DOM', () => {
      fixture.detectChanges();
      const card = fixture.debugElement.nativeElement.querySelector('.shopping-card');
      const spy = vi.spyOn(component, 'viewCart');
      card.click();
      expect(spy).toHaveBeenCalled();
    });

    it('should trigger viewDiscounts on clicking discounts-card in DOM', () => {
      fixture.detectChanges();
      const card = fixture.debugElement.nativeElement.querySelector('.discounts-card');
      const spy = vi.spyOn(component, 'viewDiscounts');
      card.click();
      expect(spy).toHaveBeenCalled();
    });
  });
});
