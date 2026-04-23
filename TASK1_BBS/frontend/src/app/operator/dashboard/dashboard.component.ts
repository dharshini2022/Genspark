import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OperatorService } from '../../services/operator.service';
import { OperatorProfile, BusDto, ScheduleDto, BookingList } from '../../models/models';

@Component({
  selector: 'app-operator-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class OperatorDashboardComponent implements OnInit {
  profile: OperatorProfile | null = null;
  buses: BusDto[] = [];
  schedules: ScheduleDto[] = [];
  bookings: BookingList[] = [];
  loading = true;

  get confirmedBookings(): number { return this.bookings.filter(b => b.status === 'Confirmed').length; }
  get revenue(): number { return this.bookings.filter(b => b.status === 'Confirmed').reduce((a, b) => a + b.totalAmount, 0); }

  constructor(private svc: OperatorService) {}

  ngOnInit(): void {
    this.svc.getProfile().subscribe(p => this.profile = p);
    this.svc.getBuses().subscribe(b => this.buses = b);
    this.svc.getSchedules().subscribe(s => this.schedules = s);
    this.svc.getBookings().subscribe(b => { this.bookings = b; this.loading = false; });
  }

  busStatusClass(s: string): string {
    return ({ Active: 'badge-success', Pending: 'badge-warning', Down: 'badge-danger', Removed: 'badge-muted' } as Record<string, string>)[s] ?? 'badge-muted';
  }
}
