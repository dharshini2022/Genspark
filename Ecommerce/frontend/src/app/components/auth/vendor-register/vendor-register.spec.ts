import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { VendorRegister } from './vendor-register';
import { VendorService } from '../../../services/vendor.service';
import { ToastService } from '../../../services/toast.service';

describe('VendorRegister', () => {
  let component: VendorRegister;
  let fixture: ComponentFixture<VendorRegister>;
  let router: Router;
  let mockVendorService: any;
  let mockToastService: any;
  let navigateSpy: any;

  beforeEach(async () => {
    mockVendorService = {
      registerVendor: vi.fn()
    };

    mockToastService = {
      warning: vi.fn(),
      success: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [VendorRegister, ReactiveFormsModule],
      providers: [
        provideRouter([]),
        { provide: VendorService, useValue: mockVendorService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorRegister);
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

  describe('Form Validation', () => {
    it('should validate storeEmail pattern', () => {
      const storeEmail = component.form.get('storeEmail')!;
      storeEmail.setValue('invalid');
      expect(storeEmail.valid).toBe(false);

      storeEmail.setValue('vendor@store.com');
      expect(storeEmail.valid).toBe(true);

      storeEmail.setValue('vendor@store.co');
      expect(storeEmail.valid).toBe(true);

      storeEmail.setValue('vendor@store.net');
      expect(storeEmail.valid).toBe(false);
    });

    it('should validate gstNumber pattern', () => {
      const gstNumber = component.form.get('gstNumber')!;
      gstNumber.setValue('12345');
      expect(gstNumber.valid).toBe(false);

      gstNumber.setValue('22AAAAA1111A1Z5'); // 15 characters alphanumeric
      expect(gstNumber.valid).toBe(true);
    });

    it('should validate panNumber pattern', () => {
      const panNumber = component.form.get('panNumber')!;
      panNumber.setValue('12345');
      expect(panNumber.valid).toBe(false);

      panNumber.setValue('ABCDE1234F'); // 10 characters alphanumeric
      expect(panNumber.valid).toBe(true);
    });
  });

  describe('onSubmit', () => {
    it('should show warning toast and mark form as touched if form is invalid', () => {
      component.form.setValue({
        storeName: '',
        storeEmail: '',
        gstNumber: '',
        panNumber: '',
        description: '',
        logoUrl: ''
      });
      component.onSubmit();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please fill all required vendor details correctly.');
      expect(component.form.touched).toBe(true);
    });

    it('should submit successfully and navigate to dashboard', () => {
      const testData = {
        storeName: 'My Awesome Store',
        storeEmail: 'vendor@store.com',
        gstNumber: '22AAAAA1111A1Z5',
        panNumber: 'ABCDE1234F',
        description: 'A great store description',
        logoUrl: 'http://example.com/logo.png'
      };
      component.form.setValue(testData);
      mockVendorService.registerVendor.mockReturnValue(of({ success: true }));

      component.onSubmit();

      expect(mockVendorService.registerVendor).toHaveBeenCalledWith(testData);
      expect(mockToastService.success).toHaveBeenCalledWith('Vendor registered successfully! Waiting for admin approval.');
      expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should handle registration error with error message object', () => {
      const testData = {
        storeName: 'My Awesome Store',
        storeEmail: 'vendor@store.com',
        gstNumber: '22AAAAA1111A1Z5',
        panNumber: 'ABCDE1234F',
        description: 'A great store description',
        logoUrl: 'http://example.com/logo.png'
      };
      component.form.setValue(testData);

      const errResponse = { error: { message: 'GST Number already registered' } };
      mockVendorService.registerVendor.mockReturnValue(throwError(() => errResponse));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(spyConsole).toHaveBeenCalledWith('Vendor registration failed:', errResponse);
      expect(mockToastService.error).toHaveBeenCalledWith('GST Number already registered');
    });

    it('should handle registration error with string error', () => {
      const testData = {
        storeName: 'My Awesome Store',
        storeEmail: 'vendor@store.com',
        gstNumber: '22AAAAA1111A1Z5',
        panNumber: 'ABCDE1234F',
        description: 'A great store description',
        logoUrl: 'http://example.com/logo.png'
      };
      component.form.setValue(testData);

      const errResponse = { error: 'Registration Rejected' };
      mockVendorService.registerVendor.mockReturnValue(throwError(() => errResponse));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(spyConsole).toHaveBeenCalledWith('Vendor registration failed:', errResponse);
      expect(mockToastService.error).toHaveBeenCalledWith('Registration Rejected');
    });

    it('should handle registration error with default fallback message', () => {
      const testData = {
        storeName: 'My Awesome Store',
        storeEmail: 'vendor@store.com',
        gstNumber: '22AAAAA1111A1Z5',
        panNumber: 'ABCDE1234F',
        description: 'A great store description',
        logoUrl: 'http://example.com/logo.png'
      };
      component.form.setValue(testData);

      const errResponse = {};
      mockVendorService.registerVendor.mockReturnValue(throwError(() => errResponse));
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

      component.onSubmit();

      expect(spyConsole).toHaveBeenCalledWith('Vendor registration failed:', errResponse);
      expect(mockToastService.error).toHaveBeenCalledWith('Vendor registration failed. Make sure you are signed in and have a valid customer account.');
    });
  });
});
