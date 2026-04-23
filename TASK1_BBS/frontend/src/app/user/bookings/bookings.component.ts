import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { BookingList, BookingResponse } from '../../models/models';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bookings.component.html',
  styleUrl: './bookings.component.scss'
})
export class BookingsComponent implements OnInit {
  bookings: BookingList[] = [];
  loading = true;
  cancelling: number | null = null;

  /** Tracks which booking IDs are expanded */
  expandedIds = new Set<number>();
  /** Cache of loaded booking details, keyed by bookingId */
  details: Record<number, BookingResponse> = {};
  /** Currently loading detail for bookingId */
  detailLoading: number | null = null;

  constructor(private userSvc: UserService, private toast: ToastService, private router: Router) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.userSvc.getMyBookings().subscribe({
      next: b => { this.bookings = b; this.loading = false; },
      error: () => this.loading = false
    });
  }

  toggleExpand(id: number): void {
    if (this.expandedIds.has(id)) {
      this.expandedIds.delete(id);
    } else {
      this.expandedIds.add(id);
      if (!this.details[id]) {
        this.detailLoading = id;
        this.userSvc.getBooking(id).subscribe({
          next: detail => { this.details[id] = detail; this.detailLoading = null; },
          error: () => { this.toast.error('Error', 'Could not load booking details'); this.detailLoading = null; }
        });
      }
    }
  }

  isExpanded(id: number): boolean { return this.expandedIds.has(id); }

  cancelBooking(id: number): void {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    this.cancelling = id;
    this.userSvc.cancelBooking(id, { reason: 'Customer request' }).subscribe({
      next: res => {
        this.toast.success('Booking cancelled', `Refund: ₹${res.refundAmount} — ${res.refundPolicy}`);
        this.load();
        this.cancelling = null;
      },
      error: err => { this.toast.error('Error', err.error?.message); this.cancelling = null; }
    });
  }

  goHome(): void { this.router.navigate(['/user/home']); }

  statusClass(s: string): string {
    return s === 'Confirmed' ? 'badge-success' : 'badge-danger';
  }

  formatTime(dt: string): string {
    return new Date(dt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
  }

  formatDate(dt: string): string {
    return new Date(dt).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });
  }

  formatDateTime(dt: string): string {
    return new Date(dt).toLocaleString('en-IN', {
      day: 'numeric', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: true
    });
  }
}
