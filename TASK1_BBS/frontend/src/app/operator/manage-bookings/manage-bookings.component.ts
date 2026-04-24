import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OperatorService } from '../../services/operator.service';
import { ScheduleDto, SchedulePassengerDto } from '../../models/models';

@Component({
  selector: 'app-manage-bookings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manage-bookings.component.html',
  styleUrl: './manage-bookings.component.scss'
})
export class ManageBookingsComponent implements OnInit {
  schedules: ScheduleDto[] = [];
  loadingSchedules = true;

  selectedSchedule: ScheduleDto | null = null;
  passengers: SchedulePassengerDto[] = [];
  loadingPassengers = false;

  constructor(private svc: OperatorService) {}

  ngOnInit(): void {
    this.svc.getSchedules().subscribe({
      next: s => { this.schedules = s; this.loadingSchedules = false; },
      error: () => this.loadingSchedules = false
    });
  }

  viewPassengers(s: ScheduleDto): void {
    this.selectedSchedule = s;
    this.loadingPassengers = true;
    this.svc.getSchedulePassengers(s.id).subscribe({
      next: p => { this.passengers = p; this.loadingPassengers = false; },
      error: () => { this.passengers = []; this.loadingPassengers = false; }
    });
  }

  formatDate(dt: string): string {
    return new Date(dt).toLocaleString('en-IN', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
  }
}
