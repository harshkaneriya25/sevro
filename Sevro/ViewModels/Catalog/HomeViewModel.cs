using Sevro.Models;

namespace Sevro.ViewModels.Catalog
{
    public class HomeViewModel
    {
        public List<ServiceCategory> Categories { get; set; } = new();
        public List<ServiceCardViewModel> FeaturedServices { get; set; } = new();
        public int TotalVerifiedProviders { get; set; }
        public int TotalServices { get; set; }
        public int TotalCompletedBookings { get; set; }
    }
}
