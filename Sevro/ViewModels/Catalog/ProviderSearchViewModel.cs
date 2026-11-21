namespace Sevro.ViewModels.Catalog
{
    public class ProviderSearchViewModel
    {
        public string? Search { get; set; }
        public string? City { get; set; }
        public bool AvailableOnly { get; set; }
        public List<string> Cities { get; set; } = new();
        public List<ProviderCardItem> Providers { get; set; } = new();
    }

    public class ProviderCardItem
    {
        public int ProviderProfileId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string? Description { get; set; }
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public int ServiceCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<string> Categories { get; set; } = new();
    }
}
