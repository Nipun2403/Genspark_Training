export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export interface AuthResponse {
  token: string;
  email: string;
  role: string;
  userId: string;
  fullName: string;
}

export interface RouteDto {
  routeId: string;
  sourceCity: string;
  destinationCity: string;
  isActive: boolean;
  busCount: number;
}

export interface BusDto {
  busId: string;
  operatorId: string;
  operatorName: string;
  routeId: string;
  sourceCity: string;
  destinationCity: string;
  plateNumber: string;
  busNumber: string;
  basePrice: number;
  totalSeats: number;
  availableSeats: number;
  status: string;
  pickupAddress: string;
  dropoffAddress: string;
  departureTime: string;
}

export interface SeatDto {
  seatId: string;
  seatNumber: number;
  status: 'Available' | 'Locked' | 'Booked';
}

export interface LockSeatResponse {
  lockIds: string[];
  expiresAt: string;
}

export interface PassengerDetail {
  seatId: string;
  name: string;
  age: number;
  gender: number;
  mobile: string;
}

export interface BookingDto {
  bookingId: string;
  busId: string;
  sourceCity: string;
  destinationCity: string;
  busNumber: string;
  operatorName: string;
  pickupAddress: string;
  dropoffAddress: string;
  totalAmount: number;
  baseFare: number;
  gst: number;
  serviceFee: number;
  bookingFee: number;
  discountPercent: number;
  status: string;
  departureTime: string;
  createdAt: string;
  seats: BookingSeatDto[];
  cancellationNote?: string;
}

export interface BookingSeatDto {
  seatNumber: number;
  passengerName: string;
  passengerAge: number;
  passengerGender: string;
  passengerMobile: string;
}

export interface SearchResultDto {
  busId: string;
  sourceCity: string;
  destinationCity: string;
  busNumber: string;
  operatorName: string;
  basePrice: number;
  availableSeats: number;
  totalSeats: number;
  pickupAddress: string;
  dropoffAddress: string;
  departureTime: string;
}

export interface CreateBusRequest {
  routeId: string;
  plateNumber: string;
  busNumber: string;
  basePrice: number;
  pickupAddress: string;
  dropoffAddress: string;
  departureTime: string;
  totalSeats?: number;
}

export interface CreateScheduleRequest {
  routeId: string;
  plateNumber: string;
  busNumber: string;
  basePrice: number;
  pickupAddress: string;
  dropoffAddress: string;
  departureTime: string; // HH:mm
  totalSeats: number;
}

export interface BusScheduleDto {
  scheduleId: string;
  routeId: string;
  sourceCity: string;
  destinationCity: string;
  plateNumber: string;
  busNumber: string;
  basePrice: number;
  totalSeats: number;
  pickupAddress: string;
  dropoffAddress: string;
  departureTime: string;
  isActive: boolean;
}

export interface AdminDashboardDto {
  totalBookings: number;
  totalRevenue: number;
  platformProfit: number;
  activeOperators: number;
  pendingOperators: number;
  totalRoutes: number;
  activeBuses: number;
}

export interface AdminRevenueAnalyticsDto {
  routes: RouteRevenueDto[];
  operators: OperatorRevenueDto[];
}

export interface RouteRevenueDto {
  routeName: string;
  revenue: number;
  bookingCount: number;
}

export interface OperatorRevenueDto {
  name: string;
  totalTurnover: number;
  platformEarnings: number;
  busCount: number;
}

export interface OperatorProfileDto {
  profileId: string;
  userId: string;
  email: string;
  businessName: string;
  contactDetails: string;
  approvalStatus: string;
  rejectionReason: string | null;
  createdAt: string;
}

export interface OperatorRevenueAnalyticsDto {
  totalRevenue: number;
  totalEarnings: number;
  perBus: BusRevenueDto[];
}

export interface BusRevenueDto {
  busId: string;
  busNumber: string;
  route: string;
  revenue: number;
  passengerCount: number;
}

export interface CouponDto {
  code: string;
  discountPercent: number;
  expiryDate: string;
  isValid: boolean;
}

export interface CancelBookingResponse {
  refundAmount: number;
  refundPercent: number;
  couponCode: string | null;
}

export interface PaymentResponse {
  success: boolean;
  paymentRef: string | null;
  message: string;
}

export interface ManifestDto {
  busNumber: string;
  route: string;
  departure: string;
  passengers: ManifestPassengerDto[];
}

export interface ManifestPassengerDto {
  seatNumber: number;
  name: string;
  age: number;
  gender: string;
  mobile: string;
}
