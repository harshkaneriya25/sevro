using Sevro.Helpers;
using Sevro.Models;
using Sevro.Models.Enums;

namespace Sevro.ViewModels.Catalog
{
    public class CatalogRateItemViewModel
    {
        public int CatalogServiceId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIconClass { get; set; } = "bi-grid-fill";
        public string? CategoryImagePath { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public PricingType PricingType { get; set; } = PricingType.Fixed;
        public string? Unit { get; set; }
        public string? EstimatedDuration { get; set; }
        public bool IsPopular { get; set; }
        public int AvailableProvidersCount { get; set; }
        public int? DefaultProviderServiceId { get; set; }

        public string FormattedPrice => PricingFormatter.FormatPrice(Price, PricingType, Unit);
        public string PricingTypeLabel => PricingFormatter.GetPricingTypeLabel(PricingType);
        public string PricingBadgeClass => PricingFormatter.GetPricingBadgeClass(PricingType);
        public string ThumbnailUrl => ServiceCardViewModel.GetCategoryThumbnail(CategoryName, CategoryImagePath);
    }

    public class ServiceCatalogViewModel
    {
        // Active Filter Inputs
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? City { get; set; }
        public bool AvailableOnly { get; set; }
        public List<string> PriceRanges { get; set; } = new();
        public string? PricingTypeFilter { get; set; }
        public string SortBy { get; set; } = "rating"; // rating | price_asc | price_desc
        public string ViewTab { get; set; } = "providers"; // providers | ratecard

        // Reference Options
        public List<ServiceCategory> Categories { get; set; } = new();
        public List<string> Cities { get; set; } = new();

        // Results
        public List<ServiceCardViewModel> Services { get; set; } = new();
        public List<CatalogRateItemViewModel> CatalogServices { get; set; } = new();

        public string? SelectedCategoryName =>
            CategoryId.HasValue
                ? Categories.FirstOrDefault(c => c.CategoryId == CategoryId.Value)?.CategoryName
                : null;

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(Search) ||
            CategoryId.HasValue ||
            !string.IsNullOrWhiteSpace(City) ||
            AvailableOnly ||
            PriceRanges.Any() ||
            !string.IsNullOrWhiteSpace(PricingTypeFilter);
    }
}
