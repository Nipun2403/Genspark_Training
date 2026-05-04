namespace OmniBus.Server.Models.Enums
{
    public enum UserRole
    {
        Customer = 0,
        Operator = 1,
        Admin = 2
    }

    public enum BusStatus
    {
        PendingApproval = 0,
        Active = 1,
        Unavailable = 2,
        Disabled = 3,
        Rejected = 4,
        EmergencyOff = 5
    }

    public enum BookingStatus
    {
        Pending = 0,
        Paid = 1,
        Cancelled = 2,
        Refunded = 3
    }

    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Disabled = 3
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2
    }
}
