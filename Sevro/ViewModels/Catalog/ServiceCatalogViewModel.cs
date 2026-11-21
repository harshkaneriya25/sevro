using Sevro.Models;

namespace Sevro.ViewModels.Catalog
{
    public class ServiceCatalogViewModel
    {
        // Active Filter Inputs
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? City { get; set; }
        public bool AvailableOnly { get; set; }
        public List<string> PriceRanges { get; set; } = new();
        public string SortBy { get; set; } = "rating"; // rating | price_asc | price_desc

        // Reference Options
        public List<ServiceCategory> Categories { get; set; } = new();
        public List<string> Cities { get; set; } = new();

        // Results
        public List<ServiceCardViewModel> Services { get; set; } = new();

        public string? SelectedCategoryName =>
            CategoryId.HasValue
                ? Categories.FirstOrDefault(c => c.CategoryId == CategoryId.Value)?.CategoryName
                : null;

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(Search) ||
            CategoryId.HasValue ||
            !string.IsNullOrWhiteSpace(City) ||
            AvailableOnly ||
            PriceRanges.Any();
    }
}
