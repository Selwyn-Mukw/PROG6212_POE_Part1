namespace RaceDay.contracts.DTOs
{
    public class EnrolmentDto
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int EventId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string EnrolmentStatus { get; set; } = string.Empty;
    }
}