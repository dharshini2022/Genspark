import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { VendorNavbar } from './vendor-navbar';
import { AuthService } from '../../../services/auth.service';

describe('VendorNavbar', () => {
  let component: VendorNavbar;
  let fixture: ComponentFixture<VendorNavbar>;
  let currentUserSubject: BehaviorSubject<any>;
  let mockAuthService: any;
  let router: Router;
  let navigateSpy: any;

  beforeEach(async () => {
    currentUserSubject = new BehaviorSubject<any>(null);
    mockAuthService = {
      logout: vi.fn(),
      currentUser$: currentUserSubject.asObservable()
    };

    await TestBed.configureTestingModule({
      imports: [VendorNavbar],
      providers: [
        provideRouter([{ path: '**', redirectTo: '' }]),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorNavbar);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    navigateSpy = vi.spyOn(router, 'navigate').mockImplementation(async () => true);
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it('should create and set default name', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(component.name()).toBe('Vendor');
  });

  it('should update name when currentUser$ emits', () => {
    fixture.detectChanges();
    currentUserSubject.next({ fullName: 'Super Seller', role: 'Vendor' });
    expect(component.name()).toBe('Super Seller');

    currentUserSubject.next(null);
    expect(component.name()).toBe('Vendor');
  });

  it('should toggle and close dropdown', () => {
    fixture.detectChanges();
    expect(component.isDropdownOpen()).toBe(false);

    const mockEvent = { stopPropagation: vi.fn() } as any;
    component.toggleDropdown(mockEvent);
    expect(mockEvent.stopPropagation).toHaveBeenCalled();
    expect(component.isDropdownOpen()).toBe(true);

    component.closeDropdown();
    expect(component.isDropdownOpen()).toBe(false);
  });

  it('should render dropdown menu when isDropdownOpen is true', () => {
    fixture.detectChanges();
    component.isDropdownOpen.set(true);
    fixture.detectChanges();

    const dropdownMenu = fixture.nativeElement.querySelector('.dropdown-menu');
    expect(dropdownMenu).not.toBeNull();

    const profileLink = dropdownMenu.querySelector('a.dropdown-item');
    expect(profileLink).not.toBeNull();
    expect(profileLink.textContent).toContain('View Profile');

    const logoutBtn = dropdownMenu.querySelector('button.dropdown-item.logout-item');
    expect(logoutBtn).not.toBeNull();
    expect(logoutBtn.textContent).toContain('Logout');
  });

  it('should close dropdown when View Profile link is clicked', () => {
    fixture.detectChanges();
    component.isDropdownOpen.set(true);
    fixture.detectChanges();

    const profileLink = fixture.nativeElement.querySelector('.dropdown-menu a.dropdown-item');
    expect(profileLink).not.toBeNull();
    profileLink.click();
    fixture.detectChanges();

    expect(component.isDropdownOpen()).toBe(false);
  });

  it('should NOT render dropdown menu when isDropdownOpen is false', () => {
    fixture.detectChanges();
    component.isDropdownOpen.set(false);
    fixture.detectChanges();

    const dropdownMenu = fixture.nativeElement.querySelector('.dropdown-menu');
    expect(dropdownMenu).toBeNull();
  });

  it('should display the current user name and role in the profile button', () => {
    currentUserSubject.next({ fullName: 'Jane Smith', role: 'Vendor' });
    fixture.detectChanges();

    const nameSpan = fixture.nativeElement.querySelector('.profile-name');
    expect(nameSpan.textContent.trim()).toBe('Jane Smith');

    const roleSpan = fixture.nativeElement.querySelector('.profile-role');
    expect(roleSpan.textContent.trim()).toBe('Vendor');
  });

  describe('onClickOutside (@HostListener)', () => {
    it('should close dropdown if clicked outside nativeElement', () => {
      fixture.detectChanges();
      component.isDropdownOpen.set(true);

      const mockEvent = { target: document.createElement('div') } as any;
      component.onClickOutside(mockEvent);

      expect(component.isDropdownOpen()).toBe(false);
    });

    it('should not close dropdown if clicked inside nativeElement', () => {
      fixture.detectChanges();
      component.isDropdownOpen.set(true);

      // Create a div inside the host element
      const hostElement = fixture.nativeElement;
      const childDiv = document.createElement('div');
      hostElement.appendChild(childDiv);

      const mockEvent = { target: childDiv } as any;
      component.onClickOutside(mockEvent);

      expect(component.isDropdownOpen()).toBe(true);
    });
  });

  describe('logout', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.isDropdownOpen.set(true);
      sessionStorage.setItem('user', JSON.stringify({ fullName: 'Vendor User', role: 'Vendor' }));
    });

    it('should clear sessionStorage and navigate to login on successful logout', () => {
      mockAuthService.logout.mockReturnValue(of({}));

      component.logout();

      expect(component.isDropdownOpen()).toBe(false);
      expect(mockAuthService.logout).toHaveBeenCalledWith({});
      expect(sessionStorage.getItem('user')).toBeNull();
      expect(navigateSpy).toHaveBeenCalledWith(['/login']);
    });

    it('should clear sessionStorage and navigate to login on logout failure', () => {
      mockAuthService.logout.mockReturnValue(throwError(() => new Error('Logout failed')));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.logout();

      expect(spyConsole).toHaveBeenCalled();
      expect(component.isDropdownOpen()).toBe(false);
      expect(sessionStorage.getItem('user')).toBeNull();
      expect(navigateSpy).toHaveBeenCalledWith(['/login']);
    });
  });
});
