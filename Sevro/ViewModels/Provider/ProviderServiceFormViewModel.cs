using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sevro.Models.Enums;

namespace Sevro.ViewModels.Provider
{
    public class ProviderServiceFormViewModel
    {
        public int ProviderServiceId { get; set; }

        [Required(ErrorMessage = "Please select a service category.")]
        [Display(Name = "Service Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Catalog Template (Optional)")]
        public int? CatalogServiceId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Service Title")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Service Description")]
        public string? Description { get; set; }

        [Required]
        [Range(0, 999999.99, ErrorMessage = "Price cannot be negative.")]
        [Display(Name = "Price (₹ INR)")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Pricing Type")]
        public PricingType PricingType { get; set; } = PricingType.Fixed;

        [StringLength(50)]
        [Display(Name = "Pricing Unit")]
        public string? Unit { get; set; } = "Per job";

        [StringLength(60)]
        [Display(Name = "Estimated Duration")]
        public string? EstimatedDuration { get; set; } = "30–60 mins";

        [Display(Name = "Active & Bookable")]
        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
