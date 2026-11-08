using RaceDay.contracts.DTOs;

namespace RaceDay.Mvc.ViewModels
{
    public class EventDetailsVm
    {
        public EventVm Event { get; set; } = new();

        public List<CategoryDto> Categories { get; set; } = new();
    }
}