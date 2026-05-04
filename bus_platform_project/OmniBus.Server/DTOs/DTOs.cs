using OmniBus.Server.Models.Enums;

namespace OmniBus.Server.DTOs
{
    // ─── Auth ───
    public record SendOtpRequest(string Email);
    public record VerifyOtpRequest(string Email, string Code);
    public record AuthResponse(string Token, string Email, string Role, Guid UserId, string FullName);

    // ─── Route ───
    public record CreateRouteRequest(string SourceCity, string DestinationCity);
    public record RouteDto(Guid RouteId, string SourceCity, string DestinationCity, bool IsActive, int BusCount);

    // ─── Bus ───
    public class CreateBusRequest
    {
        public Guid RouteId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string BusNumber { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string DropoffAddress { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("totalSeats")]
        public int? TotalSeats { get; set; }
    }
    public record BusDto(
        Guid BusId, Guid OperatorId, string OperatorName, Guid RouteId,
        string SourceCity, string DestinationCity, string PlateNumber,
        string BusNumber, decimal BasePrice, int TotalSeats, int AvailableSeats,
        string Status, string PickupAddress, string DropoffAddress, DateTime DepartureTime);
    public record BusStatusToggleRequest(BusStatus Status);
    public record CreateScheduleRequest(
        Guid RouteId, string PlateNumber, string BusNumber, decimal BasePrice,
        string PickupAddress, string DropoffAddress, string DepartureTime, int TotalSeats);
    public record BusScheduleDto(
        Guid ScheduleId, Guid RouteId, string SourceCity, string DestinationCity,
        string PlateNumber, string BusNumber, decimal BasePrice, int TotalSeats,
        string PickupAddress, string DropoffAddress, string DepartureTime, bool IsActive);

    // ─── Seat ───
    public record SeatDto(Guid SeatId, int SeatNumber, string Status); // Available, Locked, Booked
    public record LockSeatRequest(Guid BusId, List<int> SeatNumbers);
    public record LockSeatResponse(List<Guid> LockIds, DateTime ExpiresAt);

    // ─── Booking ───
    public record PassengerDetail(Guid SeatId, string Name, int Age, Gender Gender, string Mobile);
    public record CreateBookingRequest(Guid BusId, List<PassengerDetail> Passengers, string? CouponCode);
    public record BookingDto(
        Guid BookingId, Guid BusId, string SourceCity, string DestinationCity,
        string BusNumber, string OperatorName, string PickupAddress, string DropoffAddress,
        decimal TotalAmount, decimal BaseFare, decimal Gst,
        decimal ServiceFee, decimal BookingFee, decimal DiscountPercent, string Status,
        DateTime DepartureTime, DateTime CreatedAt, List<BookingSeatDto> Seats, string? CancellationNote = null);
    public record BookingSeatDto(
        int SeatNumber, string PassengerName, int PassengerAge, string PassengerGender, string PassengerMobile);
    public record CancelBookingResponse(decimal RefundAmount, decimal RefundPercent, string? CouponCode);

    // ─── Payment ───
    public record PaymentRequest(Guid BookingId, bool IsSuccess);
    public record PaymentResponse(bool Success, string? PaymentRef, string Message);

    // ─── Search ───
    public record SearchRequest(string? Source, string? Destination, DateTime? Date);
    public record SearchResultDto(
        Guid BusId, string SourceCity, string DestinationCity, string BusNumber,
        string OperatorName, decimal BasePrice, int AvailableSeats, int TotalSeats,
        string PickupAddress, string DropoffAddress, DateTime DepartureTime);

    // ─── Operator ───
    public record OperatorRegisterRequest(string FullName, string Phone, string BusinessName, string ContactDetails);
    public record OperatorProfileDto(
        Guid ProfileId, Guid UserId, string Email, string BusinessName,
        string ContactDetails, string ApprovalStatus, string? RejectionReason, DateTime CreatedAt);
    public record ManifestDto(string BusNumber, string Route, DateTime Departure, List<ManifestPassengerDto> Passengers);
    public record ManifestPassengerDto(int SeatNumber, string Name, int Age, string Gender, string Mobile);
    public record OperatorRevenueAnalyticsDto(decimal TotalRevenue, decimal TotalEarnings, List<BusRevenueDto> PerBus);
    public record BusRevenueDto(Guid BusId, string BusNumber, string Route, decimal Revenue, int PassengerCount);

    // ─── Admin ───
    public record AdminDashboardDto(
        int TotalBookings, decimal TotalRevenue, decimal PlatformProfit,
        int ActiveOperators, int PendingOperators, int TotalRoutes, int ActiveBuses);
    public record AdminRevenueAnalyticsDto(List<RouteRevenueDto> Routes, List<OperatorRevenueDto> Operators);
    public record RouteRevenueDto(string RouteName, decimal Revenue, int BookingCount);
    public record OperatorRevenueDto(string Name, decimal TotalTurnover, decimal PlatformEarnings, int BusCount);
    public record ApproveRejectRequest(bool Approve, string? Reason);
    public record ToggleOperatorResponse(int AffectedBookings, int CouponsGenerated, string Message);

    // ─── Coupon ───
    public record ValidateCouponRequest(string Code);
    public record CouponDto(string Code, decimal DiscountPercent, DateTime ExpiryDate, bool IsValid);

    // ─── Common ───
    public record ApiResponse<T>(bool Success, string Message, T? Data);
    public record ErrorResponse(bool Success, string Message, Dictionary<string, string[]>? Errors = null);
}
