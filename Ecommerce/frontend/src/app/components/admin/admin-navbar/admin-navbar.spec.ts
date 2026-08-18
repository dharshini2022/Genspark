import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AdminNavbar } from './admin-navbar';
import { AuthService } from '../../../services/auth.service';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { By } from '@angular/platform-browser';

describe('AdminNavbar', () => {
  let component: AdminNavbar;
  let fixture: ComponentFixture<AdminNavbar>;
  let currentUserSubject: BehaviorSubject<any>;
  let mockAuthService: any;
  let router: Router;

  beforeEach(async () => {
    currentUserSubject = new BehaviorSubject<any>(null);
    mockAuthService = {
      logout: vi.fn().mockReturnValue(of({})),
      currentUser$: currentUserSubject.asObservable()
    };

    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [AdminNavbar],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(AdminNavbar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should subscribe to currentUser$ and update name', () => {
    fixture.detectChanges();
    currentUserSubject.next({ fullName: 'Super Admin', role: 'ADMIN' });
    fixture.detectChanges();
    expect(component.name()).toBe('Super Admin');
    const profileName = fixture.debugElement.query(By.css('.profile-name'));
    expect(profileName.nativeElement.textContent).toContain('Super Admin');

    currentUserSubject.next(null);
    fixture.detectChanges();
    expect(component.name()).toBe('Admin');
  });

  it('should toggle dropdown state on toggleDropdown button click', () => {
    fixture.detectChanges();
    const button = fixture.debugElement.query(By.css('.profile-btn'));
    
    expect(component.isDropdownOpen()).toBe(false);
    button.nativeElement.click();
    fixture.detectChanges();

    expect(component.isDropdownOpen()).toBe(true);
    const dropdown = fixture.debugElement.query(By.css('.dropdown-menu'));
    expect(dropdown).toBeTruthy();

    button.nativeElement.click();
    fixture.detectChanges();
    expect(component.isDropdownOpen()).toBe(false);
  });

  it('should close dropdown on closeDropdown and when clicking items', () => {
    fixture.detectChanges();
    component.isDropdownOpen.set(true);
    fixture.detectChanges();

    const items = fixture.debugElement.queryAll(By.css('.dropdown-item'));
    // Click 'View Profile'
    items[0].nativeElement.click();
    fixture.detectChanges();
    expect(component.isDropdownOpen()).toBe(false);

    component.isDropdownOpen.set(true);
    fixture.detectChanges();
    const newItems = fixture.debugElement.queryAll(By.css('.dropdown-item'));
    // Click 'Add Admin'
    newItems[1].nativeElement.click();
    fixture.detectChanges();
    expect(component.isDropdownOpen()).toBe(false);
  });

  it('should close dropdown if clicked outside the element', () => {
    fixture.detectChanges();
    const elementRef = fixture.elementRef;
    const mockContains = vi.spyOn(elementRef.nativeElement, 'contains').mockReturnValue(false);
    
    component.isDropdownOpen.set(true);
    fixture.detectChanges();

    component.onClickOutside({ target: {} } as unknown as MouseEvent);
    fixture.detectChanges();

    expect(mockContains).toHaveBeenCalled();
    expect(component.isDropdownOpen()).toBe(false);
  });

  it('should not close dropdown if clicked inside the element', () => {
    fixture.detectChanges();
    const elementRef = fixture.elementRef;
    const mockContains = vi.spyOn(elementRef.nativeElement, 'contains').mockReturnValue(true);
    
    component.isDropdownOpen.set(true);
    fixture.detectChanges();

    component.onClickOutside({ target: {} } as unknown as MouseEvent);
    fixture.detectChanges();

    expect(mockContains).toHaveBeenCalled();
    expect(component.isDropdownOpen()).toBe(true);
  });

  it('should call logout and navigate to login on success when logout button is clicked', async () => {
    fixture.detectChanges();
    const navigateSpy = vi.spyOn(router, 'navigate').mockImplementation(async () => true);
    const clearSpy = vi.spyOn(Storage.prototype, 'clear');

    component.isDropdownOpen.set(true);
    fixture.detectChanges();

    const logoutBtn = fixture.debugElement.query(By.css('.logout-item'));
    logoutBtn.nativeElement.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.isDropdownOpen()).toBe(false);
    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(clearSpy).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });

  it('should log error, clear sessionStorage, and navigate to login on logout failure', async () => {
    fixture.detectChanges();
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    const navigateSpy = vi.spyOn(router, 'navigate').mockImplementation(async () => true);
    const clearSpy = vi.spyOn(Storage.prototype, 'clear');
    mockAuthService.logout.mockReturnValue(throwError(() => new Error('Logout failed')));

    component.logout();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(consoleSpy).toHaveBeenCalled();
    expect(clearSpy).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });
});
