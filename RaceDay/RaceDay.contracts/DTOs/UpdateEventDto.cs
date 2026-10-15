using System.ComponentModel.DataAnnotations;

namespace RaceDay.contracts.DTOs
{
    public class UpdateEventDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, 9999.99)]
        public decimal Distance { get; set; }

        [Range(1, int.MaxValue)]
        public int EventTypeId { get; set; }
    }
}