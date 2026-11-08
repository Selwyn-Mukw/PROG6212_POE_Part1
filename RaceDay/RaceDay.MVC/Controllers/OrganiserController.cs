using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using RaceDay.contracts.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private async Task<List<SelectListItem>> GetEventTypesAsync()
        {
            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Events/types");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SelectListItem>();
            }

            var eventTypes = await _api.ReadAsync<List<EventTypeVm>>(response);

            return eventTypes?.Select(type => new SelectListItem
            {
                Value = type.EventTypeId.ToString(),
                Text = type.EventTypeName
            }).ToList() ?? new List<SelectListItem>();
        }
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        // Check whether the logged-in user is an organiser
        private bool IsOrganiser()
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("Role");

            return !string.IsNullOrEmpty(token) && role == "Organiser";
        }

        // Organiser dashboard
        public IActionResult Index()
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.DisplayName =
                HttpContext.Session.GetString("DisplayName");

            return View();
        }

        // Display all events
        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Get, "api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to retrieve events.";
                return View(new List<EventVm>());
            }

            var events = await _api.ReadAsync<List<EventVm>>(response);

            return View(events ?? new List<EventVm>());
        }

        // Display the Create Event form
        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Auth");

            var model = new CreateEventVm
            {
                EventTypes = await GetEventTypesAsync()
            };

            return View(model);
        }

        // Submit a new event to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventVm model)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                model.EventTypes = await GetEventTypesAsync();
                return View(model);
            }

            var eventDto = new CreateEventDto
            {
                EventName = model.EventName,
                EventDate = model.EventDate,
                Location = model.Location,
                Description = model.Description,
                Distance = model.Distance,
                EventTypeId = model.EventTypeId
            };

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                "api/Events",
                eventDto);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                ViewBag.Error = string.IsNullOrWhiteSpace(error)
                    ? "Unable to create the event. Please try again."
                    : error;

                model.EventTypes = await GetEventTypesAsync();
                return View(model);
            }

            TempData["Success"] = "Event created successfully!";

            return RedirectToAction("Events");
        }
    }
}