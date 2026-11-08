using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDay.Mvc.ViewModels
{
    public class EditEventVm
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, 9999.99)]
        public decimal Distance { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select an event type.")]
        public int EventTypeId { get; set; }

        public List<SelectListItem> EventTypes { get; set; } = new();
    }
}