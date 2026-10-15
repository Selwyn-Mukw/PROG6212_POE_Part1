using System.ComponentModel.DataAnnotations;

namespace RaceDay.contracts.DTOs
{
    public class CreateResultDto
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishPosition { get; set; }
    }
}