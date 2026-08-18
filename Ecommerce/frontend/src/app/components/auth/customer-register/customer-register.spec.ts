import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CustomerRegister } from './customer-register';
import { AuthService } from '../../../services/auth.service';
import { ToastService } from '../../../services/toast.service';

describe('CustomerRegister', () => {
  let component: CustomerRegister;
  let fixture: ComponentFixture<CustomerRegister>;
  let router: Router;
  let mockAuthService: any;
  let mockToastService: any;
  let navigateSpy: any;

  beforeEach(async () => {
    mockAuthService = {
      register: vi.fn()
    };
    mockToastService = {
      warning: vi.fn(),
      success: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [CustomerRegister, ReactiveFormsModule],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerRegister);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    navigateSpy = vi.spyOn(router, 'navigate').mockImplementation(async () => true);
    fixture.detectChanges();
  });

  afterEach(() => {
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
    it('should validate email format', () => {
      const emailControl = component.form.get('email')!;
      
      emailControl.setValue('invalid');
      expect(emailControl.valid).toBe(false);

      emailControl.setValue('test@gmail.com');
      expect(emailControl.valid).toBe(true);

      emailControl.setValue('test@domain.in');
      expect(emailControl.valid).toBe(true);

      emailControl.setValue('test@domain.org');
      expect(emailControl.valid).toBe(true);

      emailControl.setValue('test@domain.net');
      expect(emailControl.valid).toBe(false); // only .com, .in, .org allowed by emailPattern
    });
  });

  describe('onSubmit', () => {
    it('should show warning toast and mark form as touched if form is invalid', () => {
      component.form.setValue({
        fullName: '',
        email: '',
        password: '',
        agreeTerms: false,
        becomeVendor: false
      });
      component.onSubmit();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please fill all required fields correctly before continuing.');
      expect(component.form.touched).toBe(true);
    });

    it('should register successfully and navigate to login when becomeVendor is false', () => {
      component.form.setValue({
        fullName: 'Test User',
        email: 'test@gmail.com',
        password: 'password123',
        agreeTerms: true,
        becomeVendor: false
      });
      
      mockAuthService.register.mockReturnValue(of({ success: true }));

      component.onSubmit();

      expect(mockAuthService.register).toHaveBeenCalledWith({
        fullName: 'Test User',
        email: 'test@gmail.com',
        password: 'password123'
      });
      expect(mockToastService.success).toHaveBeenCalledWith('Registration Successful!');
      expect(navigateSpy).toHaveBeenCalledWith(['/login']);
    });

    it('should register successfully and navigate to login with queryParams when becomeVendor is true', () => {
      component.form.setValue({
        fullName: 'Test User',
        email: 'test@gmail.com',
        password: 'password123',
        agreeTerms: true,
        becomeVendor: true
      });
      
      mockAuthService.register.mockReturnValue(of({ success: true }));

      component.onSubmit();

      expect(mockToastService.success).toHaveBeenCalledWith('Registration Successful!');
      expect(navigateSpy).toHaveBeenCalledWith(['/login'], { queryParams: { becomeVendor: true } });
    });

    it('should handle registration error with error message object', () => {
      component.form.setValue({
        fullName: 'Test User',
        email: 'test@gmail.com',
        password: 'password123',
        agreeTerms: true,
        becomeVendor: false
      });

      const errResponse = { error: { message: 'Email already exists' } };
      mockAuthService.register.mockReturnValue(throwError(() => errResponse));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(spyConsole).toHaveBeenCalledWith('Registration failed:', errResponse);
      expect(mockToastService.error).toHaveBeenCalledWith('Email already exists');
    });

    it('should handle registration error with string error', () => {
      component.form.setValue({
        fullName: 'Test User',
        email: 'test@gmail.com',
        password: 'password123',
        agreeTerms: true,
        becomeVendor: false
      });

      const errResponse = { error: 'Internal Server Error' };
      mockAuthService.register.mockReturnValue(throwError(() => errResponse));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(spyConsole).toHaveBeenCalledWith('Registration failed:', errResponse);
      expect(mockToastService.error).toHaveBeenCalledWith('Internal Server Error');
    });

    it('should handle registration error with default fallback message', () => {
      component.form.setValue({
        fullName: 'Test User',
        email: 'test@gmail.com',
        password: 'password123',
        agreeTerms: true,
        becomeVendor: false
      });

      const errResponse = {};
      mockAuthService.register.mockReturnValue(throwError(() => errResponse));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(spyConsole).toHaveBeenCalledWith('Registration failed:', errResponse);
      expect(mockToastService.error).toHaveBeenCalledWith('Registration failed. Please try again.');
    });
  });
});
