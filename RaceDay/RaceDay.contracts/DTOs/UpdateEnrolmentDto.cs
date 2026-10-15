using System.ComponentModel.DataAnnotations;

namespace RaceDay.contracts.DTOs
{
    public class UpdateEnrolmentDto
    {
        [Required]
        [RegularExpression(
            "^(Pending|Confirmed|Cancelled)$",
            ErrorMessage = "Status must be Pending, Confirmed, or Cancelled.")]
        public string EnrolmentStatus { get; set; } = string.Empty;
    }
}