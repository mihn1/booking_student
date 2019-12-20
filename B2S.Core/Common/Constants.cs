namespace B2S.Core.Common.Constants
{
    public static class DefaultLoginUrl
    {
        public const string Booking = "/Booking";
        public const string VendorDashBoard = "/Vendor/Dashboard";
        public const string AdminDashBoard = "/Dashboard";
    }

    public static class BookingNotification
    {
        public const string NotYourBooking = "Sorry, you cannot view information as this is not your booking.";
        public const string AvailableBooking = "Bed: {0} - Room: {1} - Type: {2} - Date: {3}";
        public const string BookingDetails = "Bed: {0} - Room: {1} - Type: {2} - Name: {3} - Booking: {4} -> {5}";
    }

    public static class EventColorHex
    {
        public const string CheckInColorHex = "#00a65a";
        public const string CheckOutColorHex = "#f39c12";
    }

    public static class EmailTemplateName
    {
        public const string EmailAddressVerification = "Email Address Verification";
        public const string LostPassword = "Lost Password";
        public const string CustomerInvoice = "Customer Invoice";
        public const string OverdueInvoice = "Overdue Invoice";
        public const string Welcome = "Student Welcome Letter";
        public const string ArrivalInstruction = "Arrival Instruction";
        public const string BookingNotification = "Booking Notification";
        public const string DepartureLetter = "Departure Letter";
    }

    public static class UserProfile
    {
        public const string Agent = "Agent";
        public const string Customer = "Customer";
        public const string Vendor = "Vendor";
        public const string Admin = "Admin";
    }
}
