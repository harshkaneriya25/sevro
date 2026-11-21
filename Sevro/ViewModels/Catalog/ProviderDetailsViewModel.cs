using Sevro.Models;

namespace Sevro.ViewModels.Catalog
{
    public class ProviderDetailsViewModel
    {
        public ProviderProfile Profile { get; set; } = null!;
        public List<ServiceCardViewModel> Services { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int CompletedJobsCount { get; set; }
    }
}
