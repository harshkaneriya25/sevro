using System.ComponentModel.DataAnnotations;

namespace Sevro.ViewModels.Account
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required]
        public string Role { get; set; } = "Customer"; // "Customer" or "Provider"

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 digits.")]
        [RegularExpression(@"^\d{6,}$", ErrorMessage = "Password must contain only numbers (at least 6 digits).")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Provider-only business profile fields
        [StringLength(150)]
        [Display(Name = "Business Name")]
        public string? BusinessName { get; set; }

        [Range(0, 60)]
        [Display(Name = "Years of Experience")]
        public int? ExperienceYears { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(1000)]
        [Display(Name = "Business Description")]
        public string? Description { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Role != "Customer" && Role != "Provider")
            {
                yield return new ValidationResult("Please select either Customer or Service Provider.", new[] { nameof(Role) });
            }

            if (Role == "Provider")
            {
                if (string.IsNullOrWhiteSpace(BusinessName))
                {
                    yield return new ValidationResult("Business Name is required for Service Providers.", new[] { nameof(BusinessName) });
                }
                if (string.IsNullOrWhiteSpace(City))
                {
                    yield return new ValidationResult("City is required for Service Providers.", new[] { nameof(City) });
                }
                if (!ExperienceYears.HasValue)
                {
                    yield return new ValidationResult("Experience (in years) is required for Service Providers.", new[] { nameof(ExperienceYears) });
                }
            }
        }
    }
}
