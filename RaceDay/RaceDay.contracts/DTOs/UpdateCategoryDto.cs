using System.ComponentModel.DataAnnotations;

namespace RaceDay.contracts.DTOs
{
    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string CategoryType { get; set; } = string.Empty;

        [Range(0, 120)]
        public int MinimumAge { get; set; }

        [Range(0, 120)]
        public int MaximumAge { get; set; }

        [Range(0.01, 9999.99)]
        public decimal CategoryDistance { get; set; }
    }
}