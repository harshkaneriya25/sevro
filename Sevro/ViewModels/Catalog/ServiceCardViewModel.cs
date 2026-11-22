using Sevro.Helpers;
using Sevro.Models.Enums;

namespace Sevro.ViewModels.Catalog
{
    public class ServiceCardViewModel
    {
        public int ProviderServiceId { get; set; }
        public int? CatalogServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public PricingType PricingType { get; set; } = PricingType.Fixed;
        public string? Unit { get; set; }
        public string? EstimatedDuration { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIconClass { get; set; } = "bi-grid-fill";
        public string? CategoryImagePath { get; set; }
        public int ProviderProfileId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public string FormattedPrice => PricingFormatter.FormatPrice(
            PricingType == PricingType.QuoteRequired ? null : Price, PricingType, Unit);
        public string PricingTypeLabel => PricingFormatter.GetPricingTypeLabel(PricingType);
        public string PricingBadgeClass => PricingFormatter.GetPricingBadgeClass(PricingType);
        public string ThumbnailUrl => GetCategoryThumbnail(CategoryName, CategoryImagePath);

        public static string GetCategoryThumbnail(string categoryName, string? customImagePath = null)
        {
            if (!string.IsNullOrWhiteSpace(customImagePath))
                return customImagePath;

            var key = (categoryName ?? string.Empty).Trim().ToLowerInvariant();
            if (key.Contains("electr")) return "/images/categories/electrical.svg";
            if (key.Contains("plumb") || key.Contains("water")) return "/images/categories/plumbing.svg";
            if (key.Contains("ac") || key.Contains("appliance")) return "/images/categories/ac-appliances.svg";
            if (key.Contains("laundry") || key.Contains("dry clean")) return "/images/categories/laundry.svg";
            if (key.Contains("clean")) return "/images/categories/cleaning.svg";
            if (key.Contains("carpent")) return "/images/categories/carpentry.svg";
            if (key.Contains("paint")) return "/images/categories/painting.svg";
            if (key.Contains("vehicle") || key.Contains("mechanic")) return "/images/categories/mechanics.svg";
            if (key.Contains("computer") || key.Contains("tech")) return "/images/categories/computer.svg";
            if (key.Contains("pest")) return "/images/categories/pest-control.svg";
            if (key.Contains("beauty") || key.Contains("groom")) return "/images/categories/beauty.svg";
            if (key.Contains("mov") || key.Contains("shift")) return "/images/categories/moving.svg";
            if (key.Contains("repair")) return "/images/categories/repairing.svg";
            return "/images/categories/default.svg";
        }
    }
}
