using System.ComponentModel.DataAnnotations;

namespace Sevro.Models
{
    public class CustomerAddress
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Pincode { get; set; } = string.Empty;

        // Navigation Properties
        public ApplicationUser Customer { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
