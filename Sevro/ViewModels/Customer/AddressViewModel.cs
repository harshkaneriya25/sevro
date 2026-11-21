using System.ComponentModel.DataAnnotations;

namespace Sevro.ViewModels.Customer
{
    public class AddressViewModel
    {
        public int AddressId { get; set; }

        [Required]
        [StringLength(300)]
        [Display(Name = "Street Address / Landmark")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^\d{5,10}$", ErrorMessage = "Please enter a valid numeric pincode.")]
        public string Pincode { get; set; } = string.Empty;
    }
}
