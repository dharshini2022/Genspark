import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { ToastService } from '../../../services/toast.service';
import { ReviewService } from '../../../services/review.service';
import { AuthService } from '../../../services/auth.service';
import { UserProfileResponse } from '../../../models/user.model';
import { UserAddress } from '../../../models/address.model';
import { ReviewResponse } from '../../../models/review.model';
import { AddressForm } from '../address-form/address-form';

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AddressForm],
  templateUrl: './customer-profile.html',
  styleUrl: './customer-profile.css'
})
export class CustomerProfile implements OnInit {
  profile = signal<UserProfileResponse | null>(null);
  addresses = signal<UserAddress[]>([]);
  reviews = signal<ReviewResponse[]>([]);
  loadingProfile = signal<boolean>(true);
  loadingAddresses = signal<boolean>(true);
  loadingReviews = signal<boolean>(true);

  profileForm: FormGroup;
  changePasswordForm: FormGroup;

  isEditingProfile = signal<boolean>(false);
  isAddingAddress = signal<boolean>(false);
  isChangingPassword = signal<boolean>(false);
  selectedAddressForEdit = signal<UserAddress | null>(null);

  emailPattern = /^[^\s@]+@[^\s@]+\.(com|in|org)$/;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private reviewService: ReviewService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.profileForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(4)]],
      email: ['', [Validators.required, Validators.pattern(this.emailPattern)]]
    });

    this.changePasswordForm = this.fb.group({
      oldPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmNewPassword')?.value
      ? null : { mismatch: true };
  }

  ngOnInit(): void {
    this.loadProfile();
    this.loadAddresses();
    this.loadReviews();
  }

  loadProfile(): void {
    this.loadingProfile.set(true);
    this.userService.getProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.profileForm.patchValue({
          fullName: data.fullName,
          email: data.email
        });
        this.loadingProfile.set(false);
      },
      error: (err) => {
        console.error('Error fetching profile:', err);
        this.toastService.error('Failed to load profile details.');
        this.loadingProfile.set(false);
      }
    });
  }

  loadAddresses(): void {
    this.loadingAddresses.set(true);
    this.userService.getMyAddresses().subscribe({
      next: (data) => {
        this.addresses.set(data || []);
        this.loadingAddresses.set(false);
      },
      error: (err) => {
        console.error('Error fetching addresses:', err);
        this.toastService.error('Failed to load addresses.');
        this.loadingAddresses.set(false);
      }
    });
  }

  onEditProfile(): void {
    if (this.profile()) {
      this.profileForm.patchValue({
        fullName: this.profile()!.fullName,
        email: this.profile()!.email
      });
      this.isEditingProfile.set(true);
    }
  }

  onCancelEditProfile(): void {
    this.isEditingProfile.set(false);
  }

  onSubmitProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.userService.updateProfile(this.profileForm.value).subscribe({
      next: (updatedProfile) => {
        this.profile.set(updatedProfile);
        this.authService.updateCurrentUser(updatedProfile.fullName);
        this.toastService.success('Profile updated successfully.');
        this.isEditingProfile.set(false);
      },
      error: (err) => {
        console.error('Error updating profile:', err);
        this.toastService.error(err.error?.message || err.error || 'Failed to update profile.');
      }
    });
  }

  onAddAddressClick(): void {
    this.selectedAddressForEdit.set(null);
    this.isAddingAddress.set(true);
  }

  onEditAddressClick(address: UserAddress): void {
    this.selectedAddressForEdit.set(address);
    this.isAddingAddress.set(true);
  }

  onCancelAddressForm(): void {
    this.isAddingAddress.set(false);
    this.selectedAddressForEdit.set(null);
  }

  onSaveAddress(addressData: UserAddress): void {
    const addressId = addressData.id;

    if (addressId !== undefined && addressId !== null) {
      this.userService.updateUserAddress(addressId, addressData).subscribe({
        next: () => {
          this.toastService.success('Address updated successfully.');
          this.loadAddresses();
          this.onCancelAddressForm();
        },
        error: (err) => {
          console.error('Error updating address:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to update address.');
        }
      });
    } else {
      this.userService.addUserAddress(addressData).subscribe({
        next: () => {
          this.toastService.success('Address added successfully.');
          this.loadAddresses();
          this.onCancelAddressForm();
        },
        error: (err) => {
          console.error('Error adding address:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to add address.');
        }
      });
    }
  }

  loadReviews(): void {
    this.loadingReviews.set(true);
    this.reviewService.getMyReviews().subscribe({
      next: (data) => {
        this.reviews.set(data || []);
        this.loadingReviews.set(false);
      },
      error: (err) => {
        console.error('Error fetching reviews:', err);
        this.toastService.error('Failed to load reviews.');
        this.loadingReviews.set(false);
      }
    });
  }

  onChangePassword(): void {
    this.changePasswordForm.reset();
    this.isChangingPassword.set(true);
  }

  onCancelChangePassword(): void {
    this.isChangingPassword.set(false);
    this.changePasswordForm.reset();
  }

  onSubmitChangePassword(): void {
    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      return;
    }

    const request = {
      oldPassword: this.changePasswordForm.value.oldPassword,
      newPassword: this.changePasswordForm.value.newPassword
    };

    this.userService.changePassword(request).subscribe({
      next: () => {
        this.toastService.success('Password changed successfully.');
        this.onCancelChangePassword();
      },
      error: (err) => {
        console.error('Error changing password:', err);
        this.toastService.error(err.error?.message || err.error || 'Failed to change password.');
      }
    });
  }

  onToggleAccountStatus(): void {
    const action = this.profile()?.isActive ? 'deactivate' : 'activate';
    if (confirm(`Are you sure you want to ${action} your account?`)) {
      this.userService.toggleAccountStatus().subscribe({
        next: (res) => {
          this.toastService.success(res.message || `Account ${action}d successfully.`);
          this.loadProfile();
        },
        error: (err) => {
          console.error('Error toggling account status:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to toggle account status.');
        }
      });
    }
  }

  onEditReview(review: ReviewResponse): void {
    this.router.navigate(['/customer-home/product-review-form'], {
      queryParams: {
        productId: review.productId,
        orderId: review.orderId,
        reviewId: review.id
      }
    });
  }

  onDeleteReview(reviewId: number): void {
    if (confirm('Are you sure you want to delete this review?')) {
      this.reviewService.deleteReview(reviewId).subscribe({
        next: (res) => {
          this.toastService.success(res.message || 'Review deleted successfully.');
          this.loadReviews();
        },
        error: (err) => {
          console.error('Error deleting review:', err);
          this.toastService.error(err.error?.message || err.error || 'Failed to delete review.');
        }
      });
    }
  }
}
