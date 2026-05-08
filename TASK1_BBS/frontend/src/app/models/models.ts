// ── Auth ──────────────────────────────────────────────────────────────────────
export interface RegisterDto {
  name: string;
  email: string;
  phone: string;
  password: string;
  acceptedTerms: boolean;
}

export interface OperatorRegisterDto {
  name: string;
  email: string;
  phone: string;
  password: string;
  companyName: string;
  gstNumber: string;
  offices: OfficeDto[];
}

export interface OfficeDto {
  id?: number;
  district: string;
  city: string;
  address: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  role: string;
  userId: number;
  name: string;
  email: string;
}

// ── Search ────────────────────────────────────────────────────────────────────
export interface BusSearchResult {
  scheduleId: number;
  busId: number;
  busName: string;
  busType: string;
  operatorName: string;
  operatorPhone: string;
  source: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  pricePerSeat: number;
  platformFee: number;
  gst: number;
  totalPrice: number;
  boardingAddress: string;
  droppingAddress: string;
  availableSeats: number;
  totalSeats: number;
  layoutJson: string;
  bookedSeats: string[];
  blockedSeats: string[];
  femaleBookedSeats: string[];
  womenOnlySeats: string[];
  features?: string[];
  photos?: string[];
}

// ── Seat Block ────────────────────────────────────────────────────────────────
export interface SeatBlockRequest { scheduleId: number; seatNumbers: string[]; }
export interface SeatBlockResponse { success: boolean; message: string; expiresAt: string; blockedSeats: string[]; }

// ── Booking ───────────────────────────────────────────────────────────────────
export interface PassengerDto {
  seatNumber: string;
  passengerName: string;
  age: number;
  gender: string;
}
export interface CreateBookingDto { scheduleId: number; passengers: PassengerDto[]; }
export interface BookingResponse {
  bookingId: number;
  status: string;
  totalAmount: number;
  platformFee: number;
  gst: number;
  grandTotal: number;
  busName: string;
  source: string;
  destination: string;
  boardingAddress: string;
  droppingAddress: string;
  departureTime: string;
  bookedAt: string;
  passengers: PassengerDto[];
}
export interface BookingList {
  bookingId: number;
  busName: string;
  operatorName: string;
  customerName: string;
  source: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  bookedAt: string;
  totalAmount: number;
  platformFee: number;
  gst: number;
  grandTotal: number;
  status: string;
  refundAmount?: number;
  passengerCount: number;
  seatNumbers: string[];
}
export interface CancelBookingDto { reason?: string; }
export interface CancelResponse { success: boolean; message: string; refundAmount: number; refundPolicy: string; }

// ── Payment ───────────────────────────────────────────────────────────────────
export interface PaymentRequest { bookingId: number; paymentMethod: string; cardLastFour?: string; }
export interface PaymentResponse { success: boolean; transactionId: string; amount: number; message: string; }

// ── Operator ──────────────────────────────────────────────────────────────────
export interface AddBusDto {
  busName: string; registrationNumber: string; busType: string; layoutId?: number;
  features?: string[]; photos?: string[];
}
export interface BusDto {
  id: number; busName: string; registrationNumber: string;
  busType: string; status: string; layoutName?: string; layoutId?: number; 
  features?: string[]; photos?: string[];
  createdAt: string;
}
export interface CreateScheduleDto {
  busId: number; routeId: number; departureTime: string; arrivalTime: string; pricePerSeat: number;
}
export interface ScheduleDto {
  id: number; busId: number; busName: string; routeName: string;
  source: string; destination: string; departureTime: string; arrivalTime: string;
  pricePerSeat: number; isCancelled: boolean; totalBookings: number;
}
export interface UploadLayoutDto {
  name: string; description: string; totalRows: number; seatsPerRow: number;
  hasUpperDeck: boolean; layoutJson: string;
}
export interface LayoutDto {
  id: number; name: string; description: string; totalRows: number;
  seatsPerRow: number; hasUpperDeck: boolean; isGlobal: boolean; layoutJson: string;
}
export interface UpdatePriceDto { pricePerSeat: number; }
export interface OperatorProfile {
  id: number; userId: number; operatorName: string; companyName: string;
  email: string; phone: string; gstNumber: string; status: string;
  registeredAt: string; offices: OfficeDto[]; totalBuses: number;
}
export interface OperatorDetailedRevenueDto {
  totalRevenue: number; totalBookings: number; totalCancellations: number;
  byBus: BusRevenueDto[]; bySchedule: ScheduleRevenueDto[];
}
export interface BusRevenueDto {
  busId: number; busName: string; registrationNumber: string;
  totalBookings: number; revenue: number;
}
export interface ScheduleRevenueDto {
  scheduleId: number; busId: number; busName: string; routeName: string;
  departureTime: string; totalSeats: number; bookedSeats: number;
  occupancyPercentage: number; revenue: number;
}
export interface SchedulePassengerDto {
  bookingId: number; passengerName: string; seatNumber: string;
  boardingPoint: string; maskedContact: string;
}

// ── Admin ─────────────────────────────────────────────────────────────────────
export interface RouteDto { id: number; sourceCity: string; destinationCity: string; isActive: boolean; totalSchedules: number; }
export interface CreateRouteDto { sourceCity: string; destinationCity: string; }
export interface BusPendingDto {
  id: number; busName: string; registrationNumber: string;
  busType: string; operatorName: string; companyName: string; status: string; 
  features?: string[]; photos?: string[];
  createdAt: string;
}
export interface RevenueDto {
  totalRevenue: number; totalPlatformFee: number; totalBookings: number;
  totalCancellations: number; byOperator: OperatorRevenueDto[];
}
export interface OperatorRevenueDto {
  operatorId: number; operatorName: string; companyName: string;
  totalBookings: number; grossRevenue: number; platformFeeCollected: number;
}
export interface PlatformSettingDto { key: string; value: string; description: string; }

// ── Profile ───────────────────────────────────────────────────────────────────
export interface ProfileDto { id: number; name: string; email: string; phone: string; role: string; createdAt: string; }
export interface UpdateProfileDto { name: string; phone: string; }

// ── Generic ───────────────────────────────────────────────────────────────────
export interface MessageResponse { success: boolean; message: string; }
