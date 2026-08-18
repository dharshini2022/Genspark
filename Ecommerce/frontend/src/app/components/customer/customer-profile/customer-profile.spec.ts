import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomerProfile } from './customer-profile';
import { UserService } from '../../../services/user.service';
import { ToastService } from '../../../services/toast.service';
import { ReviewService } from '../../../services/review.service';
import { AuthService } from '../../../services/auth.service';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

describe('CustomerProfile', () => {
  let component: CustomerProfile;
  let fixture: ComponentFixture<CustomerProfile>;
  let mockUserService: any;
  let mockToastService: any;
  let mockReviewService: any;
  let mockAuthService: any;

  const mockProfileData = {
    id: 1,
    fullName: 'Jane Doe',
    email: 'jane@example.com',
    role: 'Customer'
  };

  const mockAddresses = [
    {
      id: 10,
      recipientName: 'Jane Doe',
      phone: '1234567890',
      line1: '123 Main St',
      city: 'Metro',
      state: 'State',
      postalCode: '100001',
      country: 'India',
      label: 'Home'
    }
  ];

  const mockReviews = [
    {
      id: 1,
      productId: 101,
      productName: 'Product 101',
      userId: 1,
      userFullName: 'Jane Doe',
      orderId: 1001,
      rating: 5,
      title: 'Great product',
      body: 'I really loved this product!',
      createdAt: '2026-07-13T12:00:00Z',
      reviewImages: []
    }
  ];

  beforeEach(async () => {
    mockUserService = {
      getProfile: vi.fn().mockReturnValue(of(mockProfileData)),
      getMyAddresses: vi.fn().mockReturnValue(of(mockAddresses)),
      updateProfile: vi.fn().mockReturnValue(of(mockProfileData)),
      addUserAddress: vi.fn().mockReturnValue(of({})),
      updateUserAddress: vi.fn().mockReturnValue(of({})),
      changePassword: vi.fn().mockReturnValue(of({}))
    };

    mockReviewService = {
      getMyReviews: vi.fn().mockReturnValue(of(mockReviews))
    };

    mockAuthService = {
      updateCurrentUser: vi.fn()
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [CustomerProfile, ReactiveFormsModule],
      providers: [
        { provide: UserService, useValue: mockUserService },
        { provide: ReviewService, useValue: mockReviewService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerProfile);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load profile and addresses', () => {
    expect(component).toBeTruthy();
    expect(mockUserService.getProfile).toHaveBeenCalled();
    expect(mockUserService.getMyAddresses).toHaveBeenCalled();
    expect(component.profile()).toEqual(mockProfileData);
    expect(component.addresses()).toEqual(mockAddresses);
    expect(component.profileForm.value).toEqual({
      fullName: 'Jane Doe',
      email: 'jane@example.com'
    });
  });

  it('should handle error when fetching profile', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.getProfile.mockReturnValue(throwError(() => new Error('Profile API failure')));
    component.loadProfile();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load profile details.');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should handle error when fetching addresses', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.getMyAddresses.mockReturnValue(throwError(() => new Error('Address API failure')));
    component.loadAddresses();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load addresses.');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should edit and cancel profile editing', () => {
    component.onEditProfile();
    expect(component.isEditingProfile()).toBe(true);

    component.onCancelEditProfile();
    expect(component.isEditingProfile()).toBe(false);
  });

  it('should not open edit mode if profile is null', () => {
    component.profile.set(null);
    component.onEditProfile();
    expect(component.isEditingProfile()).toBe(false);
  });

  it('should validate form and touch all fields if invalid profile submit', () => {
    component.profileForm.patchValue({ fullName: '', email: 'invalid-email' });
    component.onSubmitProfile();
    expect(component.profileForm.touched).toBe(true);
    expect(mockUserService.updateProfile).not.toHaveBeenCalled();
  });

  it('should submit updated profile successfully', () => {
    component.profileForm.patchValue({ fullName: 'New Jane', email: 'jane.new@example.com' });
    mockUserService.updateProfile.mockReturnValue(of({
      ...mockProfileData,
      fullName: 'New Jane',
      email: 'jane.new@example.com'
    }));

    component.onSubmitProfile();
    expect(mockUserService.updateProfile).toHaveBeenCalledWith({
      fullName: 'New Jane',
      email: 'jane.new@example.com'
    });
    expect(mockAuthService.updateCurrentUser).toHaveBeenCalledWith('New Jane');
    expect(mockToastService.success).toHaveBeenCalledWith('Profile updated successfully.');
    expect(component.isEditingProfile()).toBe(false);
  });

  it('should handle profile update API error', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.updateProfile.mockReturnValue(throwError(() => ({ error: { message: 'Update failed' } })));
    
    component.onSubmitProfile();
    expect(mockToastService.error).toHaveBeenCalledWith('Update failed');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should toggle adding address and editing address state', () => {
    component.onAddAddressClick();
    expect(component.isAddingAddress()).toBe(true);
    expect(component.selectedAddressForEdit()).toBeNull();

    component.onCancelAddressForm();
    expect(component.isAddingAddress()).toBe(false);

    component.onEditAddressClick(mockAddresses[0]);
    expect(component.isAddingAddress()).toBe(true);
    expect(component.selectedAddressForEdit()).toEqual(mockAddresses[0]);
  });

  it('should add new address onSaveAddress', () => {
    const newAddress = {
      recipientName: 'New Person',
      phone: '9999999999',
      line1: '456 New St',
      city: 'City',
      state: 'State',
      postalCode: '200002',
      country: 'India',
      label: 'Work'
    };
    
    component.onSaveAddress(newAddress as any);
    expect(mockUserService.addUserAddress).toHaveBeenCalledWith(newAddress);
    expect(mockToastService.success).toHaveBeenCalledWith('Address added successfully.');
    expect(component.isAddingAddress()).toBe(false);
  });

  it('should handle error when adding address', () => {
    const newAddress = {
      recipientName: 'New Person',
      phone: '9999999999',
      line1: '456 New St',
      city: 'City',
      state: 'State',
      postalCode: '200002',
      country: 'India',
      label: 'Work'
    };
    
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.addUserAddress.mockReturnValue(throwError(() => ({ error: 'Fail' })));
    
    component.onSaveAddress(newAddress as any);
    expect(mockToastService.error).toHaveBeenCalledWith('Fail');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should update existing address onSaveAddress', () => {
    const updatedAddress = {
      id: 10,
      recipientName: 'Jane Updated',
      phone: '1234567890',
      line1: '123 Main St',
      city: 'Metro',
      state: 'State',
      postalCode: '100001',
      country: 'India',
      label: 'Home'
    };

    component.onSaveAddress(updatedAddress as any);
    expect(mockUserService.updateUserAddress).toHaveBeenCalledWith(10, updatedAddress);
    expect(mockToastService.success).toHaveBeenCalledWith('Address updated successfully.');
  });

  it('should handle error when updating address', () => {
    const updatedAddress = {
      id: 10,
      recipientName: 'Jane Updated',
      phone: '1234567890',
      line1: '123 Main St',
      city: 'Metro',
      state: 'State',
      postalCode: '100001',
      country: 'India',
      label: 'Home'
    };

    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.updateUserAddress.mockReturnValue(throwError(() => new Error('err')));

    component.onSaveAddress(updatedAddress as any);
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to update address.');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should handle getMyAddresses returning null/empty list', () => {
    mockUserService.getMyAddresses.mockReturnValue(of(null));
    component.loadAddresses();
    expect(component.addresses()).toEqual([]);
  });

  it('should handle profile update failure with string error', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.updateProfile.mockReturnValue(throwError(() => ({ error: 'Error String' })));
    component.onSubmitProfile();
    expect(mockToastService.error).toHaveBeenCalledWith('Error String');
    consoleSpy.mockRestore();
  });

  it('should handle profile update failure with empty error payload', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.updateProfile.mockReturnValue(throwError(() => ({})));
    component.onSubmitProfile();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to update profile.');
    consoleSpy.mockRestore();
  });

  it('should handle add address failure with empty error payload', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.addUserAddress.mockReturnValue(throwError(() => ({})));
    component.onSaveAddress({ recipientName: 'Alice' } as any);
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to add address.');
    consoleSpy.mockRestore();
  });

  it('should handle update address failure with error object containing message', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockUserService.updateUserAddress.mockReturnValue(throwError(() => ({ error: { message: 'Spec msg' } })));
    component.onSaveAddress({ id: 10, recipientName: 'Alice' } as any);
    expect(mockToastService.error).toHaveBeenCalledWith('Spec msg');
    consoleSpy.mockRestore();
  });

  describe('HTML Template rendering and actions', () => {
    it('should render address loading spinner when loadingAddresses is true', () => {
      component.loadingAddresses.set(true);
      fixture.detectChanges();

      const spinner = fixture.debugElement.nativeElement.querySelector('.loading-state');
      expect(spinner).toBeTruthy();
    });

    it('should render empty state and trigger onAddAddressClick when addresses is empty', () => {
      component.loadingAddresses.set(false);
      component.addresses.set([]);
      fixture.detectChanges();

      const emptyState = fixture.debugElement.nativeElement.querySelector('.empty-state');
      expect(emptyState).toBeTruthy();

      const addBtn = emptyState.querySelector('button');
      expect(addBtn).toBeTruthy();

      const spy = vi.spyOn(component, 'onAddAddressClick');
      addBtn.click();
      expect(spy).toHaveBeenCalled();
    });

    it('should render profile form when isEditingProfile is true and trigger submit', () => {
      component.isEditingProfile.set(true);
      fixture.detectChanges();

      const form = fixture.debugElement.nativeElement.querySelector('.profile-form');
      expect(form).toBeTruthy();

      const fullNameInput = form.querySelector('#fullName');
      expect(fullNameInput).toBeTruthy();

      // Trigger submit
      const spySubmit = vi.spyOn(component, 'onSubmitProfile');
      form.dispatchEvent(new Event('submit'));
      expect(spySubmit).toHaveBeenCalled();
    });

    it('should render address form component when isAddingAddress is true', () => {
      component.isAddingAddress.set(true);
      fixture.detectChanges();

      const addressFormEl = fixture.debugElement.nativeElement.querySelector('app-address-form');
      expect(addressFormEl).toBeTruthy();
    });

    it('should render loading spinner when loadingProfile is true', () => {
      component.loadingProfile.set(true);
      fixture.detectChanges();

      const spinner = fixture.debugElement.nativeElement.querySelector('.loading-state');
      expect(spinner).toBeTruthy();
      expect(spinner.textContent).toContain('Loading profile details...');
    });

    it('should display fullName required error when touched and empty', () => {
      component.isEditingProfile.set(true);
      fixture.detectChanges();
      const control = component.profileForm.get('fullName')!;
      control.setValue('');
      control.markAsTouched();
      fixture.detectChanges();

      const errText = fixture.debugElement.nativeElement.querySelector('.error-text');
      expect(errText.textContent).toContain('Full name is required.');
    });

    it('should display fullName minlength error when touched and too short', () => {
      component.isEditingProfile.set(true);
      fixture.detectChanges();
      const control = component.profileForm.get('fullName')!;
      control.setValue('abc');
      control.markAsTouched();
      fixture.detectChanges();

      const errText = fixture.debugElement.nativeElement.querySelector('.error-text');
      expect(errText.textContent).toContain('Full name must be at least 4 characters.');
    });

    it('should display email required error when touched and empty', () => {
      component.isEditingProfile.set(true);
      fixture.detectChanges();
      const control = component.profileForm.get('email')!;
      control.setValue('');
      control.markAsTouched();
      fixture.detectChanges();

      const errText = fixture.debugElement.nativeElement.querySelector('.error-text');
      expect(errText.textContent).toContain('Email address is required.');
    });

    it('should display email pattern error when touched and invalid', () => {
      component.isEditingProfile.set(true);
      fixture.detectChanges();
      const control = component.profileForm.get('email')!;
      control.setValue('invalid-email');
      control.markAsTouched();
      fixture.detectChanges();

      const errText = fixture.debugElement.nativeElement.querySelector('.error-text');
      expect(errText.textContent).toContain('Please enter a valid email address');
    });
  });

  describe('Reviews and Change Password logic', () => {
    it('should load reviews on init', () => {
      expect(mockReviewService.getMyReviews).toHaveBeenCalled();
      expect(component.reviews()).toEqual(mockReviews);
      expect(component.loadingReviews()).toBe(false);
    });

    it('should handle reviews loading failure', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      mockReviewService.getMyReviews.mockReturnValue(throwError(() => new Error('Reviews failure')));
      component.loadReviews();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load reviews.');
      expect(consoleSpy).toHaveBeenCalled();
      consoleSpy.mockRestore();
    });

    it('should initialize change password form and validate mismatch', () => {
      component.onChangePassword();
      expect(component.isChangingPassword()).toBe(true);

      const form = component.changePasswordForm;
      form.patchValue({
        oldPassword: 'old',
        newPassword: 'newPassword123',
        confirmNewPassword: 'differentPassword'
      });
      expect(form.invalid).toBe(true);
      expect(form.errors?.['mismatch']).toBe(true);

      form.patchValue({
        confirmNewPassword: 'newPassword123'
      });
      expect(form.valid).toBe(true);
    });

    it('should submit change password successfully', () => {
      component.onChangePassword();
      component.changePasswordForm.patchValue({
        oldPassword: 'old',
        newPassword: 'newPassword123',
        confirmNewPassword: 'newPassword123'
      });
      mockUserService.changePassword.mockReturnValue(of({ success: true }));

      component.onSubmitChangePassword();
      expect(mockUserService.changePassword).toHaveBeenCalledWith({
        oldPassword: 'old',
        newPassword: 'newPassword123'
      });
      expect(mockToastService.success).toHaveBeenCalledWith('Password changed successfully.');
      expect(component.isChangingPassword()).toBe(false);
    });

    it('should handle change password error', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.onChangePassword();
      component.changePasswordForm.patchValue({
        oldPassword: 'old',
        newPassword: 'newPassword123',
        confirmNewPassword: 'newPassword123'
      });
      mockUserService.changePassword.mockReturnValue(throwError(() => ({ error: { message: 'Password change failed' } })));

      component.onSubmitChangePassword();
      expect(mockToastService.error).toHaveBeenCalledWith('Password change failed');
      consoleSpy.mockRestore();
    });
  });
});
