import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, SearchResultDto, BusDto, SeatDto, LockSeatResponse, BookingDto, PassengerDetail, PaymentResponse, CancelBookingResponse, CouponDto, RouteDto, AdminDashboardDto, OperatorProfileDto, ManifestDto, AdminRevenueAnalyticsDto, OperatorRevenueAnalyticsDto, BusScheduleDto, CreateScheduleRequest } from '../../models/api.models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly api = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // ── Search ──
  searchBuses(source?: string, destination?: string, date?: string): Observable<ApiResponse<SearchResultDto[]>> {
    let params = new HttpParams();
    if (source) params = params.set('source', source);
    if (destination) params = params.set('destination', destination);
    if (date) params = params.set('date', date);
    return this.http.get<ApiResponse<SearchResultDto[]>>(`${this.api}/search`, { params });
  }
  getCitySuggestions(q: string, fromCity?: string, toCity?: string): Observable<ApiResponse<string[]>> {
    let params = new HttpParams().set('q', q);
    if (fromCity) params = params.set('fromCity', fromCity);
    if (toCity) params = params.set('toCity', toCity);
    return this.http.get<ApiResponse<string[]>>(`${this.api}/routes/suggestions`, { params });
  }

  // ── Routes ──
  getRoutes(): Observable<ApiResponse<RouteDto[]>> {
    return this.http.get<ApiResponse<RouteDto[]>>(`${this.api}/routes`);
  }
  createRoute(sourceCity: string, destinationCity: string): Observable<ApiResponse<RouteDto>> {
    return this.http.post<ApiResponse<RouteDto>>(`${this.api}/routes`, { sourceCity, destinationCity });
  }
  deleteRoute(routeId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.api}/routes/${routeId}`);
  }

  // ── Seats ──
  getSeatMap(busId: string): Observable<ApiResponse<SeatDto[]>> {
    return this.http.get<ApiResponse<SeatDto[]>>(`${this.api}/seats/${busId}`);
  }
  lockSeats(busId: string, seatNumbers: number[]): Observable<ApiResponse<LockSeatResponse>> {
    return this.http.post<ApiResponse<LockSeatResponse>>(`${this.api}/seats/lock`, { busId, seatNumbers });
  }
  releaseLock(lockId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.api}/seats/lock/${lockId}`);
  }

  // ── Bookings ──
  createBooking(busId: string, passengers: PassengerDetail[], couponCode?: string): Observable<ApiResponse<BookingDto>> {
    return this.http.post<ApiResponse<BookingDto>>(`${this.api}/bookings`, { busId, passengers, couponCode });
  }
  getMyBookings(): Observable<ApiResponse<BookingDto[]>> {
    return this.http.get<ApiResponse<BookingDto[]>>(`${this.api}/bookings/my`);
  }
  getBooking(bookingId: string): Observable<ApiResponse<BookingDto>> {
    return this.http.get<ApiResponse<BookingDto>>(`${this.api}/bookings/${bookingId}`);
  }
  cancelBooking(bookingId: string): Observable<ApiResponse<CancelBookingResponse>> {
    return this.http.post<ApiResponse<CancelBookingResponse>>(`${this.api}/bookings/${bookingId}/cancel`, {});
  }

  // ── Payment ──
  processPayment(bookingId: string, isSuccess: boolean): Observable<ApiResponse<PaymentResponse>> {
    return this.http.post<ApiResponse<PaymentResponse>>(`${this.api}/payment/process`, { bookingId, isSuccess });
  }

  // ── Coupons ──
  validateCoupon(code: string): Observable<ApiResponse<CouponDto>> {
    return this.http.get<ApiResponse<CouponDto>>(`${this.api}/coupons/validate/${code}`);
  }

  // ── Buses (Operator) ──
  getMyBuses(): Observable<ApiResponse<BusDto[]>> {
    return this.http.get<ApiResponse<BusDto[]>>(`${this.api}/buses/my`);
  }
  createBus(data: any): Observable<ApiResponse<BusDto>> {
    return this.http.post<ApiResponse<BusDto>>(`${this.api}/buses`, data);
  }
  createSchedule(data: CreateScheduleRequest): Observable<ApiResponse<BusScheduleDto>> {
    return this.http.post<ApiResponse<BusScheduleDto>>(`${this.api}/schedules`, data);
  }
  getMySchedules(): Observable<ApiResponse<BusScheduleDto[]>> {
    return this.http.get<ApiResponse<BusScheduleDto[]>>(`${this.api}/schedules/my`);
  }
  toggleSchedule(id: string, active: boolean): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.api}/schedules/${id}/toggle?isActive=${active}`, {});
  }
  deleteSchedule(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.api}/schedules/${id}`);
  }
  toggleBusStatus(busId: string, status: number): Observable<ApiResponse<BusDto>> {
    return this.http.put<ApiResponse<BusDto>>(`${this.api}/buses/${busId}/toggle-status`, { status });
  }
  updateBus(busId: string, data: any): Observable<ApiResponse<string>> {
    return this.http.patch<ApiResponse<string>>(`${this.api}/operator/bus/${busId}`, data);
  }
  getManifest(busId: string): Observable<ApiResponse<ManifestDto>> {
    return this.http.get<ApiResponse<ManifestDto>>(`${this.api}/operator/manifest/${busId}`);
  }
  getOperatorRevenue(): Observable<ApiResponse<OperatorRevenueAnalyticsDto>> {
    return this.http.get<ApiResponse<OperatorRevenueAnalyticsDto>>(`${this.api}/operator/revenue`);
  }

  // ── Operator Registration ──
  registerOperator(data: any): Observable<ApiResponse<OperatorProfileDto>> {
    return this.http.post<ApiResponse<OperatorProfileDto>>(`${this.api}/operator/register`, data);
  }
  getOperatorProfile(): Observable<ApiResponse<OperatorProfileDto>> {
    return this.http.get<ApiResponse<OperatorProfileDto>>(`${this.api}/operator/profile`);
  }

  // ── Admin ──
  getAdminDashboard(): Observable<ApiResponse<AdminDashboardDto>> {
    return this.http.get<ApiResponse<AdminDashboardDto>>(`${this.api}/admin/dashboard`);
  }
  getAllBuses(): Observable<ApiResponse<BusDto[]>> {
    return this.http.get<ApiResponse<BusDto[]>>(`${this.api}/buses`);
  }
  approveBus(busId: string): Observable<ApiResponse<BusDto>> {
    return this.http.put<ApiResponse<BusDto>>(`${this.api}/buses/${busId}/approve`, {});
  }
  getOperators(): Observable<ApiResponse<OperatorProfileDto[]>> {
    return this.http.get<ApiResponse<OperatorProfileDto[]>>(`${this.api}/admin/operators`);
  }
  reviewOperator(userId: string, approve: boolean, reason?: string): Observable<ApiResponse<OperatorProfileDto>> {
    return this.http.put<ApiResponse<OperatorProfileDto>>(`${this.api}/admin/operators/${userId}/review`, { approve, reason });
  }
  toggleOperator(userId: string, enable: boolean): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${this.api}/admin/operators/${userId}/toggle?enable=${enable}`, {});
  }
  getAdminRevenueAnalytics(): Observable<ApiResponse<AdminRevenueAnalyticsDto>> {
    return this.http.get<ApiResponse<AdminRevenueAnalyticsDto>>(`${this.api}/admin/revenue-analytics`);
  }
  rejectBus(busId: string): Observable<ApiResponse<BusDto>> {
    return this.http.put<ApiResponse<BusDto>>(`${this.api}/buses/${busId}/reject`, {});
  }
}
