using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sevro.ViewModels.Provider
{
    public class ProviderServiceFormViewModel
    {
        public int ProviderServiceId { get; set; }

        [Required(ErrorMessage = "Please select a service category.")]
        [Display(Name = "Service Category")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Service Title")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Service Description")]
        public string? Description { get; set; }

        [Required]
        [Range(1, 999999.99, ErrorMessage = "Price must be greater than ₹0.")]
        [Display(Name = "Fixed Price (₹)")]
        public decimal Price { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
