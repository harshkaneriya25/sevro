using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Sevro.Models;

namespace Sevro.ViewModels.Customer
{
    public class CreateBookingViewModel
    {
        [Required]
        public int ProviderServiceId { get; set; }

        [Required(ErrorMessage = "Please select a saved service address.")]
        [Display(Name = "Service Address")]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Please select a preferred booking date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Service Date")]
        public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please select a preferred time slot.")]
        [StringLength(40)]
        [Display(Name = "Preferred Time Slot")]
        public string PreferredTimeSlot { get; set; } = "10:00 AM – 12:00 PM";

        [StringLength(1000)]
        [Display(Name = "Describe Your Issue / Requirement")]
        public string? ProblemDescription { get; set; }

        [Display(Name = "Upload Issue Photo (Optional)")]
        public IFormFile? IssuePhoto { get; set; }

        // Display details for checkout summary
        public ProviderService? Service { get; set; }
        public double ProviderAverageRating { get; set; }
        public int ProviderReviewCount { get; set; }
        public List<CustomerAddress> SavedAddresses { get; set; } = new();

        public static readonly string[] TimeSlots =
        {
            "08:00 AM – 10:00 AM",
            "10:00 AM – 12:00 PM",
            "12:00 PM – 02:00 PM",
            "02:00 PM – 04:00 PM",
            "04:00 PM – 06:00 PM",
            "06:00 PM – 08:00 PM"
        };
    }
}
