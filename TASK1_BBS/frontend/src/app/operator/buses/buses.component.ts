import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OperatorService } from '../../services/operator.service';
import { ToastService } from '../../services/toast.service';
import { BusDto, LayoutDto } from '../../models/models';

@Component({
  selector: 'app-operator-buses',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './buses.component.html',
  styleUrl: './buses.component.scss'
})
export class OperatorBusesComponent implements OnInit {
  buses: BusDto[] = [];
  layouts: LayoutDto[] = [];
  loading = true;
  showAdd = false;
  adding = false;
  downId: number | null = null;
  addForm!: FormGroup;

  constructor(private svc: OperatorService, private toast: ToastService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.addForm = this.fb.group({
      busName:            ['', Validators.required],
      busType:            ['', Validators.required],
      registrationNumber: ['', Validators.required],
      layoutId:           [null]
    });
    this.svc.getBuses().subscribe(b => { this.buses = b; this.loading = false; });
    this.svc.getLayouts().subscribe(l => this.layouts = l);
  }

  addBus(): void {
    if (this.addForm.invalid) { this.addForm.markAllAsTouched(); return; }
    this.adding = true;
    const val = { ...this.addForm.value, layoutId: this.addForm.value.layoutId || null };
    this.svc.addBus(val).subscribe({
      next: b => { this.buses = [...this.buses, b]; this.toast.success('Bus added!', 'Pending admin approval'); this.showAdd = false; this.adding = false; this.addForm.reset(); },
      error: err => { this.toast.error('Error', err.error?.message); this.adding = false; }
    });
  }

  bringDown(id: number): void {
    this.downId = id;
    this.svc.bringDownBus(id).subscribe({
      next: () => { this.toast.success('Bus taken down'); this.svc.getBuses().subscribe(b => this.buses = b); this.downId = null; },
      error: err => { this.toast.error('Error', err.error?.message); this.downId = null; }
    });
  }

  statusClass(s: string): string {
    return ({ Active: 'badge-success', Pending: 'badge-warning', Down: 'badge-danger', Removed: 'badge-muted' } as Record<string, string>)[s] ?? 'badge-muted';
  }
}
