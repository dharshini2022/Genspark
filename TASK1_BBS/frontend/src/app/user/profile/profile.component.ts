import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { ProfileDto } from '../../models/models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  profile: ProfileDto | null = null;
  loading = true;
  saving = false;
  editForm!: FormGroup;

  constructor(private svc: UserService, private toast: ToastService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.svc.getProfile().subscribe(p => {
      this.profile = p;
      this.loading = false;
      this.editForm = this.fb.group({
        name:  [p.name,  Validators.required],
        phone: [p.phone, Validators.required]
      });
    });
  }

  save(): void {
    if (this.editForm.invalid) return;
    this.saving = true;
    this.svc.updateProfile(this.editForm.value).subscribe({
      next: p => { this.profile = p; this.toast.success('Profile updated!'); this.saving = false; },
      error: () => { this.toast.error('Error', 'Could not update profile'); this.saving = false; }
    });
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-IN', { year: 'numeric', month: 'long', day: 'numeric' });
  }
}
