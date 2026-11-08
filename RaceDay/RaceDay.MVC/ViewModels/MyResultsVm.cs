using RaceDay.contracts.DTOs;

namespace RaceDay.Mvc.ViewModels
{
    public class MyResultsVm
    {
        public List<ResultDto> Results { get; set; } = new();

        public List<EnrolmentDto> Enrolments { get; set; } = new();

        public List<EventVm> Events { get; set; } = new();
    }
}