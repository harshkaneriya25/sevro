using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sevro.Models.Enums;

namespace Sevro.Models
{
    public class ProviderService
    {
        [Key]
        public int ProviderServiceId { get; set; }

        [Required]
        public int ProviderProfileId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? CatalogServiceId { get; set; }

        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal Price { get; set; }

        [Required]
        public PricingType PricingType { get; set; } = PricingType.Fixed;

        [StringLength(50)]
        public string? Unit { get; set; } = "Per job";

        [StringLength(60)]
        public string? EstimatedDuration { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public ProviderProfile ProviderProfile { get; set; } = null!;
        public ServiceCategory Category { get; set; } = null!;
        public CatalogService? CatalogService { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
