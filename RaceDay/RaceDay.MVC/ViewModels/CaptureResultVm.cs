using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels
{
    public class CaptureResultVm
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Finishing position must be at least 1.")]
        public int FinishPosition { get; set; }
    }
}