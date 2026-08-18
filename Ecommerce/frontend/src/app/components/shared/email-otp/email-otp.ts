import { Component, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-email-otp',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './email-otp.html',
  styleUrl: './email-otp.css',
})
export class EmailOtp implements OnDestroy {
  step = 1; // 1: Send OTP, 2: Verify OTP, 3: Reset Password
  isSubmitting = false;
  resendCountdown = 0;
  private timerInterval: any;

  emailForm: FormGroup;
  otpForm: FormGroup;
  resetForm: FormGroup;

  private emailPattern = /^[^\s@]+@[^\s@]+\.(com|in|org)$/;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.pattern(this.emailPattern)]]
    });

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6), Validators.pattern(/^\d+$/)]]
    });

    this.resetForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  get ef() { return this.emailForm.controls; }
  get of() { return this.otpForm.controls; }
  get rf() { return this.resetForm.controls; }

  passwordMatchValidator(g: FormGroup) {
    const password = g.get('password')?.value;
    const confirmPassword = g.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  sendOtp() {
    if (this.emailForm.invalid) {
      this.emailForm.markAllAsTouched();
      return;
    }

    const email = this.emailForm.value.email;
    this.isSubmitting = true;
    this.cdr.detectChanges();

    this.authService.sendForgotPasswordOtp(email).subscribe({
      next: () => {
        setTimeout(() => {
          this.isSubmitting = false;
          this.step = 2;
          this.startResendTimer();
          this.cdr.detectChanges();
        });
        this.toastService.success('OTP sent successfully. Please check your inbox.');
      },
      error: (err) => {
        setTimeout(() => {
          this.isSubmitting = false;
          this.cdr.detectChanges();
        });
        this.toastService.error(err.error?.message || 'Failed to send OTP. Please ensure email is correct.');
      }
    });
  }

  verifyOtp() {
    if (this.otpForm.invalid) {
      this.otpForm.markAllAsTouched();
      return;
    }

    const email = this.emailForm.value.email;
    const otp = this.otpForm.value.otp;
    this.isSubmitting = true;
    this.cdr.detectChanges();

    this.authService.verifyForgotPasswordOtp(email, otp).subscribe({
      next: () => {
        setTimeout(() => {
          this.isSubmitting = false;
          this.step = 3;
          this.cdr.detectChanges();
        });
        this.toastService.success('OTP verified successfully. You can now reset your password.');
      },
      error: (err) => {
        setTimeout(() => {
          this.isSubmitting = false;
          this.cdr.detectChanges();
        });
        this.toastService.error(err.error?.message || 'Invalid or expired OTP.');
      }
    });
  }

  resetPassword() {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    const email = this.emailForm.value.email;
    const otp = this.otpForm.value.otp;
    const newPassword = this.resetForm.value.password;
    this.isSubmitting = true;
    this.cdr.detectChanges();

    this.authService.resetPasswordWithOtp(email, otp, newPassword).subscribe({
      next: () => {
        setTimeout(() => {
          this.isSubmitting = false;
          this.cdr.detectChanges();
          this.router.navigate(['/login']);
        });
        this.toastService.success('Password updated successfully! Please sign in with your new password.');
      },
      error: (err) => {
        setTimeout(() => {
          this.isSubmitting = false;
          this.cdr.detectChanges();
        });
        this.toastService.error(err.error?.message || 'Failed to reset password. Please try again.');
      }
    });
  }

  startResendTimer() {
    this.resendCountdown = 60;
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
    this.timerInterval = setInterval(() => {
      if (this.resendCountdown > 0) {
        this.resendCountdown--;
      } else {
        clearInterval(this.timerInterval);
      }
    }, 1000);
  }

  resendOtp() {
    if (this.resendCountdown > 0) return;
    this.sendOtp();
  }

  goBackToStep1() {
    this.step = 1;
    this.otpForm.reset();
  }

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }
}
