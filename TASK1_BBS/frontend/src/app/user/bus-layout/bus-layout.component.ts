import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { ToastService } from '../../services/toast.service';
import { TicketService } from '../../services/ticket.service';
import { BusSearchResult, BookingResponse, PassengerDto } from '../../models/models';

interface LayoutConfig { rows: number; cols: number; aisle: number; seats: (string | null)[][]; }

@Component({
  selector: 'app-bus-layout',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './bus-layout.component.html',
  styleUrl: './bus-layout.component.scss'
})
export class BusLayoutComponent implements OnInit, OnDestroy {
  bus: BusSearchResult | null = null;
  grid: (string | null)[][] = [];
  selectedSeats: string[] = [];
  blockedUntil: string | null = null;
  blocking = false;
  booking = false;
  passengerForm!: FormGroup;
  private timer: any;

  constructor(
    private router: Router,
    private userSvc: UserService,
    private toast: ToastService,
    private fb: FormBuilder,
    private ticketSvc: TicketService
  ) {
    const nav = this.router.getCurrentNavigation();
    this.bus = nav?.extras?.state?.['bus'] ?? history.state?.bus ?? null;
  }

  ngOnInit(): void {
    if (this.bus?.layoutJson) this.buildGrid(JSON.parse(this.bus.layoutJson));
    this.passengerForm = this.fb.group({ passengers: this.fb.array([]) });
  }

  buildGrid(cfg: LayoutConfig): void {
    this.grid = cfg.seats.map(row => row.map(s => s));
  }

  isAvailable(seat: string): boolean { return !this.isBooked(seat) && !this.isBlocked(seat) && !this.isSelected(seat); }
  isBooked(seat: string): boolean   { return !!this.bus?.bookedSeats.includes(seat); }
  isBlocked(seat: string): boolean  { return !!this.bus?.blockedSeats.includes(seat); }
  isSelected(seat: string): boolean { return this.selectedSeats.includes(seat); }

  toggleSeat(seat: string): void {
    if (this.isBooked(seat) || this.isBlocked(seat)) return;
    if (this.blockedUntil) return;
    if (this.isSelected(seat)) {
      this.selectedSeats = this.selectedSeats.filter(s => s !== seat);
    } else {
      this.selectedSeats = [...this.selectedSeats, seat];
    }
  }

  blockSeats(): void {
    if (!this.selectedSeats.length || !this.bus) return;
    this.blocking = true;
    this.userSvc.blockSeats({ scheduleId: this.bus.scheduleId, seatNumbers: this.selectedSeats }).subscribe({
      next: res => {
        if (res.success) {
          this.blockedUntil = new Date(res.expiresAt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
          this.buildPassengerForm();
          this.startTimer(new Date(res.expiresAt).getTime() - Date.now());
          this.toast.success('Seats reserved!', 'You have 5 minutes to complete booking');
        } else {
          this.toast.error('Seat conflict', res.message);
        }
        this.blocking = false;
      },
      error: () => { this.toast.error('Error', 'Could not reserve seats'); this.blocking = false; }
    });
  }

  buildPassengerForm(): void {
    const arr = this.fb.array(this.selectedSeats.map(s => this.fb.group({
      seatNumber:    [s],
      passengerName: ['', Validators.required],
      age:           ['', [Validators.required, Validators.min(1)]],
      gender:        ['', Validators.required]
    })));
    this.passengerForm = this.fb.group({ passengers: arr });
  }

  get passengers(): FormArray { return this.passengerForm.get('passengers') as FormArray; }

  startTimer(ms: number): void {
    this.timer = setTimeout(() => {
      this.toast.warning('Time expired', 'Your seat reservation has expired.');
      this.blockedUntil = null;
      this.selectedSeats = [];
    }, ms);
  }

  confirmBooking(): void {
    if (this.passengerForm.invalid) { this.passengerForm.markAllAsTouched(); return; }
    if (!this.bus) return;
    this.booking = true;
    const dto = { scheduleId: this.bus.scheduleId, passengers: this.passengers.value as PassengerDto[] };

    this.userSvc.createBooking(dto).subscribe({
      next: (bookingResult: BookingResponse) => {
        this.userSvc.pay({ bookingId: bookingResult.bookingId, paymentMethod: 'Dummy' }).subscribe({
          next: () => {
            clearTimeout(this.timer);

            this.toast.success('Booking Confirmed! 🎉', `Booking #${bookingResult.bookingId} — check your email`);

            // ─── Auto-download ticket ────────────────────────
            this.ticketSvc.downloadTicket(bookingResult, {
              busType:      this.bus?.busType ?? '',
              operatorName: this.bus?.operatorName ?? '',
              arrivalTime:  this.bus?.arrivalTime ?? ''
            });
            // ────────────────────────────────────────────────

            this.router.navigate(['/user/bookings']);
          },
          error: () => { this.booking = false; this.toast.error('Payment failed', 'Please try again'); }
        });
      },
      error: err => { this.toast.error('Booking failed', err.error?.message); this.booking = false; }
    });
  }

  formatDate(dt: string): string {
    return new Date(dt).toLocaleDateString('en-IN', { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' });
  }

  ngOnDestroy(): void { clearTimeout(this.timer); }
}
