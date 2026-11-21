using System.ComponentModel.DataAnnotations;

namespace Sevro.ViewModels.Customer
{
    public class CreateReviewViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Please select a star rating between 1 and 5.")]
        public int Rating { get; set; } = 5;

        [StringLength(1000)]
        [Display(Name = "Your Feedback")]
        public string? Comment { get; set; }
    }
}
