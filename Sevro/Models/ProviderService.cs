using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 99999999.99)]
        public decimal Price { get; set; }

        // Navigation Properties
        public ProviderProfile ProviderProfile { get; set; } = null!;
        public ServiceCategory Category { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
