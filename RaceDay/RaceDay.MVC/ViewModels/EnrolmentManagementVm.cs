using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDay.contracts.DTOs;

namespace RaceDay.Mvc.ViewModels
{
    public class EnrolmentManagementVm
    {
        public int EventId { get; set; }

        public List<SelectListItem> Events { get; set; } = new();

        public List<EnrolmentDto> Enrolments { get; set; } = new();
    }
}