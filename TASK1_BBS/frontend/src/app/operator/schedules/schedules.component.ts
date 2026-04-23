import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { OperatorService } from '../../services/operator.service';
import { ToastService } from '../../services/toast.service';
import { ScheduleDto, BusDto, RouteDto } from '../../models/models';

@Component({
  selector: 'app-operator-schedules',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './schedules.component.html',
  styleUrl: './schedules.component.scss'
})
export class OperatorSchedulesComponent implements OnInit {
  schedules: ScheduleDto[] = [];
  activeBuses: BusDto[] = [];
  routes: RouteDto[] = [];
  showAdd = false;
  adding = false;
  loading = true;
  editingPrice: number | null = null;
  newPrice = 0;
  addForm!: FormGroup;

  constructor(private svc: OperatorService, private toast: ToastService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.addForm = this.fb.group({
      busId:         ['', Validators.required],
      routeId:       ['', Validators.required],
      departureTime: ['', Validators.required],
      arrivalTime:   ['', Validators.required],
      pricePerSeat:  ['', [Validators.required, Validators.min(1)]]
    });
    this.svc.getSchedules().subscribe(s => { this.schedules = s; this.loading = false; });
    this.svc.getBuses().subscribe(b => this.activeBuses = b.filter(bus => bus.status === 'Active'));
    this.svc.getRoutes().subscribe(r => this.routes = r);
  }

  addSchedule(): void {
    if (this.addForm.invalid) { this.addForm.markAllAsTouched(); return; }
    this.adding = true;
    const raw = this.addForm.value;
    const toUtc = (dt: string) => dt.includes('Z') ? dt : dt + ':00Z';
    const val = {
      busId:         +raw.busId,
      routeId:       +raw.routeId,
      departureTime: toUtc(raw.departureTime),
      arrivalTime:   toUtc(raw.arrivalTime),
      pricePerSeat:  raw.pricePerSeat
    };
    this.svc.createSchedule(val).subscribe({
      next: s => { this.schedules = [s, ...this.schedules]; this.toast.success('Schedule added!'); this.showAdd = false; this.adding = false; },
      error: err => { this.toast.error('Error', err.error?.message ?? 'Failed to create schedule'); this.adding = false; }
    });
  }

  startEditPrice(s: ScheduleDto): void { this.editingPrice = s.id; this.newPrice = s.pricePerSeat; }

  savePrice(id: number): void {
    this.svc.updatePrice(id, { pricePerSeat: this.newPrice }).subscribe({
      next: () => {
        const s = this.schedules.find(s => s.id === id);
        if (s) s.pricePerSeat = this.newPrice;
        this.toast.success('Price updated!');
        this.editingPrice = null;
      },
      error: err => this.toast.error('Error', err.error?.message)
    });
  }

  formatDateTime(dt: string): string {
    return new Date(dt).toLocaleString('en-IN', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
  }
}
