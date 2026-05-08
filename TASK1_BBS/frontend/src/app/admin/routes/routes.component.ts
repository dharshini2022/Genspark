import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { RouteDto } from '../../models/models';

@Component({
  selector: 'app-admin-routes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './routes.component.html',
  styleUrl: './routes.component.scss'
})
export class AdminRoutesComponent implements OnInit {
  routes: RouteDto[] = [];
  loading = true;
  showAdd = false;
  adding = false;
  toggleId: number | null = null;
  routeForm!: FormGroup;

  constructor(private svc: AdminService, private toast: ToastService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.routeForm = this.fb.group({
      sourceCity:      ['', Validators.required],
      destinationCity: ['', Validators.required]
    });
    this.svc.getRoutes().subscribe(r => { this.routes = r; this.loading = false; });
  }

  addRoute(): void {
    if (this.routeForm.invalid) { this.routeForm.markAllAsTouched(); return; }
    this.adding = true;
    this.svc.createRoute(this.routeForm.value).subscribe({
      next: r => { this.routes = [...this.routes, r]; this.toast.success('Route added!'); this.showAdd = false; this.adding = false; this.routeForm.reset(); },
      error: err => { this.toast.error('Error', err.error?.message); this.adding = false; }
    });
  }

  toggle(id: number): void {
    this.toggleId = id;
    this.svc.toggleRoute(id).subscribe({
      next: () => {
        const r = this.routes.find(r => r.id === id);
        if (r) r.isActive = !r.isActive;
        this.toast.success('Route updated!');
        this.toggleId = null;
      },
      error: err => { this.toast.error('Error', err.error?.message); this.toggleId = null; }
    });
  }
}
