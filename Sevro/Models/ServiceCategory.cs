using System.ComponentModel.DataAnnotations;

namespace Sevro.Models
{
    public class ServiceCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    }
}
