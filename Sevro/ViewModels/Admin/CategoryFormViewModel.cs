using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sevro.ViewModels.Admin
{
    public class CategoryFormViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(80)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClass { get; set; } = "bi-grid-fill";

        [Display(Name = "Category Icon / Image (Optional)")]
        public IFormFile? IconImageFile { get; set; }

        public string? ExistingImagePath { get; set; }

        [Display(Name = "Active in Marketplace")]
        public bool IsActive { get; set; } = true;

        [Range(0, 999)]
        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;
    }
}
