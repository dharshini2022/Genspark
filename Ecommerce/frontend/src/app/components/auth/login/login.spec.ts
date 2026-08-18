import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { AuthService } from '../../../services/auth.service';
import { ToastService } from '../../../services/toast.service';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let router: Router;
  let mockAuthService: any;
  let mockToastService: any;
  let navigateSpy: any;
  let becomeVendorVal: string | null = null;
  let mockCurrentUserValue: any = null;

  beforeEach(async () => {
    becomeVendorVal = null;
    mockCurrentUserValue = null;

    mockAuthService = {
      login: vi.fn(),
      get currentUserValue() {
        return mockCurrentUserValue;
      }
    };

    mockToastService = {
      warning: vi.fn(),
      success: vi.fn(),
      error: vi.fn()
    };

    const mockActivatedRoute = {
      snapshot: {
        queryParamMap: {
          get: (key: string) => {
            if (key === 'becomeVendor') {
              return becomeVendorVal;
            }
            return null;
          }
        }
      }
    };

    await TestBed.configureTestingModule({
      imports: [Login, ReactiveFormsModule],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService },
        { provide: ToastService, useValue: mockToastService },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    navigateSpy = vi.spyOn(router, 'navigate').mockImplementation(async () => true);
    fixture.detectChanges();
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.restoreAllMocks();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return form controls from f getter', () => {
    expect(component.f).toEqual(component.form.controls);
  });

  it('should toggle showPassword when calling togglePassword()', () => {
    expect(component.showPassword).toBe(false);
    component.togglePassword();
    expect(component.showPassword).toBe(true);
    component.togglePassword();
    expect(component.showPassword).toBe(false);
  });

  describe('Form Validation', () => {
    it('should validate email pattern', () => {
      const emailControl = component.form.get('email')!;
      emailControl.setValue('invalid');
      expect(emailControl.valid).toBe(false);

      emailControl.setValue('test@example.com');
      expect(emailControl.valid).toBe(true);

      emailControl.setValue('test@example.org');
      expect(emailControl.valid).toBe(true);

      emailControl.setValue('test@example.net');
      expect(emailControl.valid).toBe(false);
    });
  });

  describe('onSubmit', () => {
    it('should show warning toast and mark form as touched if form is invalid', () => {
      component.form.setValue({
        email: '',
        password: ''
      });
      component.onSubmit();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please enter a valid email and password to continue.');
      expect(component.form.touched).toBe(true);
    });

    describe('Success Login Paths', () => {
      it('should set role from identity claim, set username, and redirect to customer-home by default', () => {
        component.form.setValue({
          email: 'customer@gmail.com',
          password: 'password123'
        });

        mockAuthService.login.mockReturnValue(of({ fullName: 'Customer User', role: 'Customer' }));
        mockCurrentUserValue = {
          fullName: 'Customer User',
          role: 'Customer'
        };

        component.onSubmit();

        expect(sessionStorage.getItem('role')).toBe('Customer');
        expect(sessionStorage.getItem('name')).toBe('Customer User');
        expect(navigateSpy).toHaveBeenCalledWith(['/customer-home']);
      });

      it('should set role from role key, and redirect to admin-home when role is ADMIN', () => {
        component.form.setValue({
          email: 'admin@gmail.com',
          password: 'password123'
        });

        mockAuthService.login.mockReturnValue(of({ fullName: 'Admin User', role: 'admin' }));
        mockCurrentUserValue = {
          role: 'admin'
        };

        component.onSubmit();

        expect(sessionStorage.getItem('role')).toBe('admin');
        expect(navigateSpy).toHaveBeenCalledWith(['/admin-home']);
      });

      it('should redirect to vendor-home when role is VENDOR', () => {
        component.form.setValue({
          email: 'vendor@gmail.com',
          password: 'password123'
        });

        mockAuthService.login.mockReturnValue(of({ accessToken: 'vendor-token' }));
        mockCurrentUserValue = {
          'role': 'vendor'
        };

        component.onSubmit();

        expect(navigateSpy).toHaveBeenCalledWith(['/vendor-home']);
      });

      it('should redirect to vendor-register when becomeVendor query parameter is true', () => {
        becomeVendorVal = 'true';
        component.form.setValue({
          email: 'user@gmail.com',
          password: 'password123'
        });

        mockAuthService.login.mockReturnValue(of({ accessToken: 'some-token' }));
        mockCurrentUserValue = null;

        component.onSubmit();

        expect(navigateSpy).toHaveBeenCalledWith(['/vendor-register']);
      });

      it('should handle successful login when currentUserValue is null or has no role claim', () => {
        component.form.setValue({
          email: 'user@gmail.com',
          password: 'password123'
        });

        mockAuthService.login.mockReturnValue(of({ accessToken: 'some-token' }));
        mockCurrentUserValue = {}; // no role claim

        component.onSubmit();

        expect(sessionStorage.getItem('role')).toBeNull();
        expect(navigateSpy).toHaveBeenCalledWith(['/customer-home']);
      });
    });

    describe('Error Login Paths', () => {
      it('should handle login error with error message object', () => {
        component.form.setValue({
          email: 'user@gmail.com',
          password: 'password123'
        });

        const errorObj = { error: { message: 'Invalid credentials' } };
        mockAuthService.login.mockReturnValue(throwError(() => errorObj));
        const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

        component.onSubmit();

        expect(spyConsole).toHaveBeenCalledWith('Login failed:', errorObj);
        expect(mockToastService.error).toHaveBeenCalledWith('Invalid credentials');
      });

      it('should handle login error with string error', () => {
        component.form.setValue({
          email: 'user@gmail.com',
          password: 'password123'
        });

        const errorObj = { error: 'Server Error' };
        mockAuthService.login.mockReturnValue(throwError(() => errorObj));
        const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

        component.onSubmit();

        expect(spyConsole).toHaveBeenCalledWith('Login failed:', errorObj);
        expect(mockToastService.error).toHaveBeenCalledWith('Server Error');
      });

      it('should handle login error with default fallback message', () => {
        component.form.setValue({
          email: 'user@gmail.com',
          password: 'password123'
        });

        const errorObj = {};
        mockAuthService.login.mockReturnValue(throwError(() => errorObj));
        const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

        component.onSubmit();

        expect(spyConsole).toHaveBeenCalledWith('Login failed:', errorObj);
        expect(mockToastService.error).toHaveBeenCalledWith('Login failed. Please check your credentials.');
      });
    });
  });
});
