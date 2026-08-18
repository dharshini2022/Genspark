import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AccessDenied } from './access-denied';

describe('AccessDenied', () => {
  let component: AccessDenied;
  let fixture: ComponentFixture<AccessDenied>;
  let mockRouter: any;

  beforeEach(async () => {
    mockRouter = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [AccessDenied],
      providers: [
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AccessDenied);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should set isLoggedIn to false if user is not found in sessionStorage', () => {
      sessionStorage.removeItem('user');
      fixture.detectChanges();
      expect(component.isLoggedIn).toBe(false);
    });

    it('should set isLoggedIn to true if user is found in sessionStorage', () => {
      sessionStorage.setItem('user', JSON.stringify({ fullName: 'JohnDoe', role: 'Customer' }));
      fixture.detectChanges();
      expect(component.isLoggedIn).toBe(true);
    });
  });

  describe('handleAction', () => {
    it('should navigate to /dashboard if logged in', () => {
      component.isLoggedIn = true;
      component.handleAction();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should navigate to /login if not logged in', () => {
      component.isLoggedIn = false;
      component.handleAction();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    });
  });
});
