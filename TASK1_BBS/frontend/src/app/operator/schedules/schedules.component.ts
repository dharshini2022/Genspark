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

  offices: any[] = [];
  sourceOffice: any = null;
  destinationOffice: any = null;
  sourceAddress = '';
  destinationAddress = '';
  saveSourceOffice = false;
  saveDestinationOffice = false;
  selectedRoute: RouteDto | null = null;

  ngOnInit(): void {
    this.addForm = this.fb.group({
      busId:         ['', Validators.required],
      routeId:       ['', Validators.required],
      departureTime: ['', Validators.required],
      arrivalTime:   ['', Validators.required],
      pricePerSeat:  ['', [Validators.required, Validators.min(1)]]
    });

    this.addForm.get('routeId')?.valueChanges.subscribe(rId => {
      if (!rId) {
        this.selectedRoute = null;
        this.sourceOffice = null;
        this.destinationOffice = null;
        return;
      }
      this.selectedRoute = this.routes.find(r => r.id === +rId) || null;
      if (this.selectedRoute) {
        this.sourceOffice = this.offices.find(o => o.city.toLowerCase() === this.selectedRoute!.sourceCity.toLowerCase()) || null;
        this.destinationOffice = this.offices.find(o => o.city.toLowerCase() === this.selectedRoute!.destinationCity.toLowerCase()) || null;
        this.sourceAddress = this.sourceOffice?.address || '';
        this.destinationAddress = this.destinationOffice?.address || '';
      }
    });

    this.svc.getSchedules().subscribe(s => { this.schedules = s; this.loading = false; });
    this.svc.getBuses().subscribe(b => this.activeBuses = b.filter(bus => bus.status === 'Active'));
    this.svc.getRoutes().subscribe(r => this.routes = r);
    this.svc.getOffices().subscribe(o => this.offices = o);
  }

  updateOffice(type: 'source' | 'destination'): void {
    if (type === 'source' && this.sourceOffice) {
      this.svc.updateOffice(this.sourceOffice.id, { district: '', city: this.sourceOffice.city, address: this.sourceAddress }).subscribe({
        next: () => { this.toast.success('Source office updated'); this.sourceOffice.address = this.sourceAddress; },
        error: err => this.toast.error('Error', err.error?.message)
      });
    } else if (type === 'destination' && this.destinationOffice) {
      this.svc.updateOffice(this.destinationOffice.id, { district: '', city: this.destinationOffice.city, address: this.destinationAddress }).subscribe({
        next: () => { this.toast.success('Destination office updated'); this.destinationOffice.address = this.destinationAddress; },
        error: err => this.toast.error('Error', err.error?.message)
      });
    }
  }

  addSchedule(): void {
    if (this.addForm.invalid) { this.addForm.markAllAsTouched(); return; }

    if (this.saveSourceOffice && this.selectedRoute && !this.sourceOffice) {
      this.svc.addOffice({ district: '', city: this.selectedRoute.sourceCity, address: this.sourceAddress }).subscribe(
        () => this.svc.getOffices().subscribe(o => { this.offices = o; this.addForm.get('routeId')?.updateValueAndValidity(); })
      );
    }
    if (this.saveDestinationOffice && this.selectedRoute && !this.destinationOffice) {
      this.svc.addOffice({ district: '', city: this.selectedRoute.destinationCity, address: this.destinationAddress }).subscribe(
        () => this.svc.getOffices().subscribe(o => { this.offices = o; this.addForm.get('routeId')?.updateValueAndValidity(); })
      );
    }

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

  cancelling: number | null = null;

  cancelSchedule(id: number): void {
    if (!confirm('Are you sure you want to cancel this trip? All confirmed bookings will be refunded automatically.')) return;
    this.cancelling = id;
    this.svc.cancelSchedule(id).subscribe({
      next: res => {
        this.toast.success('Trip Cancelled', res.message);
        const s = this.schedules.find(x => x.id === id);
        if (s) s.isCancelled = true;
        this.cancelling = null;
      },
      error: err => {
        this.toast.error('Cancellation Failed', err.error?.message);
        this.cancelling = null;
      }
    });
  }

  formatDateTime(dt: string): string {
    return new Date(dt).toLocaleString('en-IN', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
  }
}
