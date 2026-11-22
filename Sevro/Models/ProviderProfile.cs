using System.ComponentModel.DataAnnotations;

namespace Sevro.Models
{
    public class ProviderProfile
    {
        [Key]
        public int ProviderProfileId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string BusinessName { get; set; } = string.Empty;

        [Range(0, 60)]
        public int ExperienceYears { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(300)]
        public string? ServiceAreas { get; set; }

        [StringLength(1000)]
        public string? PortfolioPhotos { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsAvailable { get; set; } = true;

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public ICollection<ProviderService> Services { get; set; } = new List<ProviderService>();
    }
}
