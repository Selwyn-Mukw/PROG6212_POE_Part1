
using RaceDay.contracts.DTOs;

namespace RaceDay.Mvc.ViewModels
{
    public class MyEnrolmentsVm
    {
        public List<EnrolmentDto> Enrolments { get; set; } = new();

        public List<EventVm> Events { get; set; } = new();

        public List<CategoryDto> Categories { get; set; } = new();
    }
}