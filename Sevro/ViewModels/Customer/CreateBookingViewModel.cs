using System.ComponentModel.DataAnnotations;
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

        // Display details for checkout summary
        public ProviderService? Service { get; set; }
        public double ProviderAverageRating { get; set; }
        public int ProviderReviewCount { get; set; }
        public List<CustomerAddress> SavedAddresses { get; set; } = new();
    }
}
