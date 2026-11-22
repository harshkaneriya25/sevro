using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sevro.Models.Enums;

namespace Sevro.Models
{
    public class CatalogService
    {
        [Key]
        public int CatalogServiceId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(180)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal? Price { get; set; }

        [Required]
        public PricingType PricingType { get; set; } = PricingType.Fixed;

        [StringLength(50)]
        public string? Unit { get; set; } = "Per job";

        [StringLength(60)]
        public string? EstimatedDuration { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPopular { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ServiceCategory Category { get; set; } = null!;
        public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    }
}
