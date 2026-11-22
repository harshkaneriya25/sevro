using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sevro.Models;
using Sevro.Models.Enums;

namespace Sevro.ViewModels.Admin
{
    public class AdminCatalogListItem
    {
        public CatalogService Service { get; set; } = null!;
        public int VerifiedProviderCount { get; set; }
    }

    public class AdminCatalogIndexViewModel
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? PricingType { get; set; }
        public string? StatusFilter { get; set; } // all | active | inactive | popular

        public List<ServiceCategory> Categories { get; set; } = new();
        public List<AdminCatalogListItem> Items { get; set; } = new();

        public int TotalCatalogServices { get; set; }
        public int ActiveServicesCount { get; set; }
        public int PopularServicesCount { get; set; }
        public int QuoteServicesCount { get; set; }
    }

    public class CatalogServiceFormViewModel
    {
        public int CatalogServiceId { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Service name is required.")]
        [StringLength(150)]
        [Display(Name = "Service Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Price cannot be negative.")]
        [Display(Name = "Price (₹ INR)")]
        public decimal? Price { get; set; }

        [Required]
        [Display(Name = "Pricing Type")]
        public PricingType PricingType { get; set; } = PricingType.Fixed;

        [StringLength(50)]
        [Display(Name = "Pricing Unit")]
        public string? Unit { get; set; } = "Per job";

        [StringLength(60)]
        [Display(Name = "Estimated Duration")]
        public string? EstimatedDuration { get; set; } = "30–60 mins";

        [Display(Name = "Active in Catalog")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Mark as Popular")]
        public bool IsPopular { get; set; } = false;

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
