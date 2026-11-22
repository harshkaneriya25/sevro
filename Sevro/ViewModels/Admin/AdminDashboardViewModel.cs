using Sevro.Models;

namespace Sevro.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProviders { get; set; }
        public int PendingVerifications { get; set; }
        public int TotalBookings { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCatalogServices { get; set; }
        public List<ProviderProfile> PendingProviders { get; set; } = new();
        public List<Booking> RecentBookings { get; set; } = new();
    }
}
