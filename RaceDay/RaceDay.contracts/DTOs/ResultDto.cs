namespace RaceDay.contracts.DTOs
{
    public class ResultDto
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int FinishPosition { get; set; }

        public string ResultStatus { get; set; } = "Pending";
    }
}