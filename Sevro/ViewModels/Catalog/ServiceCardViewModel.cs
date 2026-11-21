namespace Sevro.ViewModels.Catalog
{
    public class ServiceCardViewModel
    {
        public int ProviderServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProviderProfileId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string ThumbnailUrl => GetCategoryThumbnail(CategoryName);

        public static string GetCategoryThumbnail(string categoryName)
        {
            var key = (categoryName ?? string.Empty).Trim().ToLowerInvariant();
            return key switch
            {
                "plumbing" => "/images/categories/plumbing.svg",
                "electrical" => "/images/categories/electrical.svg",
                "cleaning" => "/images/categories/cleaning.svg",
                "painting" => "/images/categories/painting.svg",
                "mechanics" => "/images/categories/mechanics.svg",
                "repairing" => "/images/categories/repairing.svg",
                _ => "/images/categories/default.svg"
            };
        }
    }
}
