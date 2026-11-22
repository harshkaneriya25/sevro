using System.ComponentModel.DataAnnotations;

namespace Sevro.Models
{
    public class ServiceCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(120)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(80)]
        public string IconClass { get; set; } = "bi-grid-fill";

        [StringLength(300)]
        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        // Navigation Properties
        public ICollection<CatalogService> CatalogServices { get; set; } = new List<CatalogService>();
        public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    }
}
