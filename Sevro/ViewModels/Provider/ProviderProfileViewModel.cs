using System.ComponentModel.DataAnnotations;

namespace Sevro.ViewModels.Provider
{
    public class ProviderProfileViewModel
    {
        public int ProviderProfileId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Owner Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        [Display(Name = "Contact Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [Range(0, 60)]
        [Display(Name = "Experience (Years)")]
        public int ExperienceYears { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "About Your Business")]
        public string? Description { get; set; }

        public bool IsVerified { get; set; }
        public bool IsAvailable { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
