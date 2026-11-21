using Sevro.Models;

namespace Sevro.ViewModels.Customer
{
    public class CustomerDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int SavedAddresses { get; set; }
        public List<Booking> RecentBookings { get; set; } = new();
    }
}
