import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomerNavbar } from './customer-navbar';
import { AuthService } from '../../../services/auth.service';
import { CartService } from '../../../services/cart.service';
import { WishlistService } from '../../../services/wishlist.service';
import { NotificationService } from '../../../services/notification.service';
import { Router, ActivatedRoute } from '@angular/router';
import { signal } from '@angular/core';
import { of, throwError, BehaviorSubject } from 'rxjs';

describe('CustomerNavbar', () => {
  let component: CustomerNavbar;
  let fixture: ComponentFixture<CustomerNavbar>;
  let currentUserSubject: BehaviorSubject<any>;
  let mockAuthService: any;
  let mockCartService: any;
  let mockWishlistService: any;
  let mockNotificationService: any;
  let mockRouter: any;

  beforeEach(async () => {
    currentUserSubject = new BehaviorSubject<any>(null);
    mockAuthService = {
      logout: vi.fn().mockReturnValue(of({})),
      currentUser$: currentUserSubject.asObservable()
    };

    mockCartService = {
      cartCountSignal: signal(2),
      updateCartCount: vi.fn()
    };

    mockWishlistService = {
      wishlistCountSignal: signal(1),
      updateWishlistCount: vi.fn()
    };

    mockNotificationService = {
      startConnection: vi.fn(),
      loadNotifications: vi.fn(),
      stopConnection: vi.fn(),
      notifications: signal([]),
      unreadCount: signal(0)
    };

    mockRouter = {
      navigate: vi.fn()
    };

    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [CustomerNavbar],
      providers: [
        { provide: ActivatedRoute, useValue: {} },
        { provide: AuthService, useValue: mockAuthService },
        { provide: CartService, useValue: mockCartService },
        { provide: WishlistService, useValue: mockWishlistService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerNavbar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and initialize with guest name', () => {
    expect(component).toBeTruthy();
    expect(component.name()).toBe('Guest');
    expect(mockCartService.updateCartCount).not.toHaveBeenCalled();
  });

  it('should update cart/wishlist and start notifications when currentUser$ emits logged-in user and user exists', () => {
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
    
    // Trigger currentUser$ next
    currentUserSubject.next({ fullName: 'JohnDoe', role: 'Customer' });
    
    expect(component.name()).toBe('JohnDoe');
    expect(mockCartService.updateCartCount).toHaveBeenCalled();
    expect(mockWishlistService.updateWishlistCount).toHaveBeenCalled();
    expect(mockNotificationService.startConnection).toHaveBeenCalled();
    expect(mockNotificationService.loadNotifications).toHaveBeenCalled();
  });

  it('should not update cart if cart_count is already present in sessionStorage', () => {
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
    sessionStorage.setItem('cart_count', '5');
    sessionStorage.setItem('wishlist_count', '3');
    
    currentUserSubject.next({ fullName: 'JohnDoe', role: 'Customer' });
    
    expect(mockCartService.updateCartCount).not.toHaveBeenCalled();
    expect(mockWishlistService.updateWishlistCount).not.toHaveBeenCalled();
  });

  it('should clear notifications and reset counts when currentUser$ emits null or user missing', () => {
    // Start as logged in
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
    currentUserSubject.next({ fullName: 'JohnDoe', role: 'Customer' });
    
    // Now trigger logout state via currentUser$
    currentUserSubject.next(null);
    
    expect(component.name()).toBe('Guest');
    expect(mockNotificationService.stopConnection).toHaveBeenCalled();
    expect(mockNotificationService.unreadCount()).toBe(0);
  });

  it('should toggle dropdown', () => {
    const event = new MouseEvent('click');
    const stopPropagationSpy = vi.spyOn(event, 'stopPropagation');
    
    expect(component.isDropdownOpen()).toBe(false);
    component.toggleDropdown(event);
    expect(stopPropagationSpy).toHaveBeenCalled();
    expect(component.isDropdownOpen()).toBe(true);

    component.closeDropdown();
    expect(component.isDropdownOpen()).toBe(false);
  });

  it('should handle search with query', () => {
    component.searchQuery = ' laptop ';
    component.onSearch();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
      queryParams: { search: 'laptop' }
    });
  });

  it('should handle search with empty query', () => {
    component.searchQuery = '   ';
    component.onSearch();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list']);
  });

  it('should close dropdown on click outside', () => {
    component.isDropdownOpen.set(true);
    
    // Create click event outside element
    const event = new MouseEvent('click');
    // Mock nativeElement contains to return false
    const containsSpy = vi.spyOn(fixture.debugElement.nativeElement, 'contains').mockReturnValue(false);
    
    component.onClickOutside(event);
    expect(component.isDropdownOpen()).toBe(false);
  });

  it('should not close dropdown on click inside', () => {
    component.isDropdownOpen.set(true);
    const event = new MouseEvent('click');
    const containsSpy = vi.spyOn(fixture.debugElement.nativeElement, 'contains').mockReturnValue(true);
    
    component.onClickOutside(event);
    expect(component.isDropdownOpen()).toBe(true);
  });

  it('should toggle notification sidebar', () => {
    const event = new MouseEvent('click');
    const stopSpy = vi.spyOn(event, 'stopPropagation');
    
    expect(component.isNotificationSidebarOpen()).toBe(false);
    component.toggleNotificationSidebar(event);
    expect(stopSpy).toHaveBeenCalled();
    expect(component.isNotificationSidebarOpen()).toBe(true);

    component.closeNotificationSidebar();
    expect(component.isNotificationSidebarOpen()).toBe(false);
  });

  it('should perform logout successfully', () => {
    component.logout();
    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(mockNotificationService.stopConnection).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should handle logout API error gracefully', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockAuthService.logout.mockReturnValue(throwError(() => new Error('Logout API failed')));
    
    component.logout();
    expect(mockNotificationService.stopConnection).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should initialize isLoggedIn to true if user exists on construction', () => {
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
    const localFixture = TestBed.createComponent(CustomerNavbar);
    const localComp = localFixture.componentInstance;
    expect(localComp.isLoggedIn).toBe(true);
  });

  describe('HTML Template rendering and actions', () => {
    it('should display badges when cart, wishlist, and notifications counts are > 0', () => {
      mockCartService.cartCountSignal.set(5);
      mockWishlistService.wishlistCountSignal.set(3);
      mockNotificationService.unreadCount.set(4);
      fixture.detectChanges();

      const badges = fixture.debugElement.nativeElement.querySelectorAll('.cart-badge');
      expect(badges.length).toBe(3);
      expect(badges[0].textContent.trim()).toBe('4'); // notifications
      expect(badges[1].textContent.trim()).toBe('3'); // wishlist
      expect(badges[2].textContent.trim()).toBe('5'); // cart
    });

    it('should display login button if isLoggedIn is false', () => {
      component.isLoggedIn = false;
      fixture.detectChanges();

      const loginBtn = fixture.debugElement.nativeElement.querySelector('.btn-primary');
      expect(loginBtn).toBeTruthy();
      expect(loginBtn.textContent.trim()).toBe('Login');
    });

    it('should render profile dropdown and trigger dropdown actions when isLoggedIn is true', () => {
      component.isLoggedIn = true;
      fixture.detectChanges();

      const profileBtn = fixture.debugElement.nativeElement.querySelector('.profile-btn');
      expect(profileBtn).toBeTruthy();

      // Click to open dropdown
      profileBtn.click();
      fixture.detectChanges();

      const dropdownMenu = fixture.debugElement.nativeElement.querySelector('.dropdown-menu');
      expect(dropdownMenu).toBeTruthy();

      const items = dropdownMenu.querySelectorAll('.dropdown-item');
      expect(items.length).toBe(3); // Profile, Orders, Logout

      // Click View Profile
      items[0].click();
      expect(component.isDropdownOpen()).toBe(false);

      // Open again to click logout
      profileBtn.click();
      fixture.detectChanges();
      const logoutItem = dropdownMenu.querySelector('.logout-item');
      logoutItem.click();
      expect(mockAuthService.logout).toHaveBeenCalled();
    });

    it('should trigger search on enter key or search button click', () => {
      const searchInput = fixture.debugElement.nativeElement.querySelector('input[type="text"]');
      searchInput.value = 'shoes';
      searchInput.dispatchEvent(new Event('input'));
      
      const searchBtn = fixture.debugElement.nativeElement.querySelector('.search-action-btn');
      searchBtn.click();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
        queryParams: { search: 'shoes' }
      });
    });
  });
});
