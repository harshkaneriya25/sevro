using System.ComponentModel.DataAnnotations;

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
    }
}
