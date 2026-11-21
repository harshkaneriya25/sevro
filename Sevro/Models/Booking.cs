using System.ComponentModel.DataAnnotations;
using Sevro.Models.Enums;

namespace Sevro.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public int ProviderServiceId { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ApplicationUser Customer { get; set; } = null!;
        public ProviderService ProviderService { get; set; } = null!;
        public CustomerAddress Address { get; set; } = null!;
        public Review? Review { get; set; }
    }
}
