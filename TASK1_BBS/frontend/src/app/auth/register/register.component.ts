import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  form: FormGroup;
  loading = false;
  error = '';
  isOperator = false;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router, private toast: ToastService) {
    this.form = this.fb.group({
      name:          ['', Validators.required],
      email:         ['', [Validators.required, Validators.email]],
      phone:         ['', Validators.required],
      password:      ['', [Validators.required, Validators.minLength(6)]],
      acceptedTerms: [false, Validators.requiredTrue]
    });
  }

  onOperatorToggle(): void {
    if (this.isOperator) {
      // Prefill name/email/phone into query params so operator reg is pre-filled
      const { name, email, phone, password } = this.form.value;
      this.router.navigate(['/auth/operator-register'], {
        state: { prefill: { name, email, phone, password } }
      });
    }
  }

  submit(): void {
    if (this.isOperator) {
      this.router.navigate(['/auth/operator-register']);
      return;
    }
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true; this.error = '';
    this.auth.register(this.form.value).subscribe({
      next: () => { this.toast.success('Account created!', 'Welcome aboard'); this.router.navigate(['/user/home']); },
      error: err => { this.error = err.error?.message ?? 'Registration failed.'; this.loading = false; }
    });
  }
}
