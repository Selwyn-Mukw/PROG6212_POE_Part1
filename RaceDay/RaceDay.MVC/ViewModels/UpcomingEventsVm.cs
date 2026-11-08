using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.ViewModels
{
    public class UpcomingEventsVm
    {
        public string? Search { get; set; }

        public string? Location { get; set; }

        public List<EventVm> Events { get; set; } = new();
    }
}