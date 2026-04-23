import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-operator-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './operator-register.component.html',
  styleUrl: './operator-register.component.scss'
})
export class OperatorRegisterComponent {
  form: FormGroup;
  loading = false;
  error = '';

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router, private toast: ToastService) {
    this.form = this.fb.group({
      name:        ['', Validators.required],
      email:       ['', [Validators.required, Validators.email]],
      phone:       ['', Validators.required],
      password:    ['', [Validators.required, Validators.minLength(6)]],
      companyName: ['', Validators.required],
      gstNumber:   ['', Validators.required],
      offices:     this.fb.array([this.newOffice()])
    });
  }

  get offices(): FormArray { return this.form.get('offices') as FormArray; }

  newOffice(): FormGroup {
    return this.fb.group({
      city:     ['', Validators.required],
      district: ['', Validators.required],
      address:  ['', Validators.required]
    });
  }

  addOffice(): void    { this.offices.push(this.newOffice()); }
  removeOffice(i: number): void { this.offices.removeAt(i); }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true; this.error = '';
    this.auth.registerOperator(this.form.value).subscribe({
      next: () => { this.toast.success('Registration submitted!', 'Await admin approval to login.'); this.router.navigate(['/auth/login']); },
      error: err => { this.error = err.error?.message ?? 'Registration failed.'; this.loading = false; }
    });
  }
}
