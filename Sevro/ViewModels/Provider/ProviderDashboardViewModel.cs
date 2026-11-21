using Sevro.Models;

namespace Sevro.ViewModels.Provider
{
    public class ProviderDashboardViewModel
    {
        public ProviderProfile Profile { get; set; } = null!;
        public int PendingRequests { get; set; }
        public int ActiveJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int ListedServices { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<Booking> RecentBookings { get; set; } = new();
    }
}
