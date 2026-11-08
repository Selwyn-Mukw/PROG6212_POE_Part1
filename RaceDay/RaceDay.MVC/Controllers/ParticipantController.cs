using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? search, string? location)
        {
            string? token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            string? role = HttpContext.Session.GetString("Role");

            if (role != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.DisplayName =
                HttpContext.Session.GetString("DisplayName");

            var model = new UpcomingEventsVm
            {
                Search = search,
                Location = location
            };

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to retrieve upcoming events.";

                return View(model);
            }

            var events = await _api.ReadAsync<List<EventVm>>(response)
                         ?? new List<EventVm>();

            // Display upcoming events only.
            events = events
                .Where(e => e.EventDate.Date >= DateTime.Today)
                .ToList();

            // Filter by event name.
            if (!string.IsNullOrWhiteSpace(search))
            {
                events = events
                    .Where(e => e.EventName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by location.
            if (!string.IsNullOrWhiteSpace(location))
            {
                events = events
                    .Where(e => e.Location.Contains(
                        location,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            model.Events = events
                .OrderBy(e => e.EventDate)
                .ToList();

            return View(model);
        }
    }
}