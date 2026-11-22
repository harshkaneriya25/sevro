using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sevro.Models.Enums;

namespace Sevro.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int ProviderServiceId { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [StringLength(40)]
        public string? PreferredTimeSlot { get; set; }

        [StringLength(1000)]
        public string? ProblemDescription { get; set; }

        [StringLength(300)]
        public string? IssuePhotoPath { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? AgreedPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? QuotedAmount { get; set; }

        [StringLength(500)]
        public string? QuoteNotes { get; set; }

        public bool? IsQuoteApproved { get; set; }

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
