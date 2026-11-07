namespace RaceDay.Mvc.ViewModels
{
    public class EventVm
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;
    }
}