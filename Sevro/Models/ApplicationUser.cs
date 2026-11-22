using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Sevro.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ProviderProfile? ProviderProfile { get; set; }
        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
