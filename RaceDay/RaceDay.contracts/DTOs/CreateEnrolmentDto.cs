using System.ComponentModel.DataAnnotations;

namespace RaceDay.contracts.DTOs
{
    public class CreateEnrolmentDto
    {
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}