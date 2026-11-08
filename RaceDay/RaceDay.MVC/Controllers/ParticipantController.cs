using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using RaceDay.contracts.DTOs;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        // PARTICIPANT DASHBOARD
        public IActionResult Index()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.DisplayName =
                HttpContext.Session.GetString("DisplayName");

            return View();
        }

        // CHECK PARTICIPANT LOGIN
        private bool IsParticipant()
        {
            string? token =
                HttpContext.Session.GetString("JwtToken");

            string? role =
                HttpContext.Session.GetString("Role");

            return !string.IsNullOrEmpty(token)
                   && role == "Participant";
        }

        // UPCOMING EVENTS
        public async Task<IActionResult> BrowseEvents(
            string? search,
            string? location)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

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
                ViewBag.Error =
                    "Unable to retrieve upcoming events.";

                return View(model);
            }

            var events =
                await _api.ReadAsync<List<EventVm>>(response)
                ?? new List<EventVm>();

            events = events
                .Where(e => e.EventDate.Date >= DateTime.Today)
                .ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                events = events
                    .Where(e => e.EventName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

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
        // MY ENROLMENTS
        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new MyEnrolmentsVm();

            // Retrieve the participant's enrolments.
            using var enrolmentResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/Enrolments/mine");

            if (!enrolmentResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to retrieve your enrolments.";
                return View(model);
            }

            model.Enrolments =
                await _api.ReadAsync<List<EnrolmentDto>>(enrolmentResponse)
                ?? new List<EnrolmentDto>();

            // Retrieve event information.
            using var eventResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/Events");

            if (eventResponse.IsSuccessStatusCode)
            {
                model.Events =
                    await _api.ReadAsync<List<EventVm>>(eventResponse)
                    ?? new List<EventVm>();
            }

            // Retrieve category information.
            using var categoryResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/Categories");

            if (categoryResponse.IsSuccessStatusCode)
            {
                model.Categories =
                    await _api.ReadAsync<List<CategoryDto>>(categoryResponse)
                    ?? new List<CategoryDto>();
            }

            return View(model);
        }

        // CANCEL ENROLMENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Delete,
                $"api/Enrolments/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Your enrolment has been cancelled successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Unable to cancel your enrolment. Please try again.";
            }

            return RedirectToAction("MyEvents");
        }
        // MY RACE RESULTS
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new MyResultsVm();

            // Retrieve the participant's results.
            using var resultsResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/Results/mine");

            if (!resultsResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to retrieve your race results.";
                return View(model);
            }

            model.Results =
                await _api.ReadAsync<List<ResultDto>>(resultsResponse)
                ?? new List<ResultDto>();

            // Retrieve the participant's enrolments.
            using var enrolmentsResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/Enrolments/mine");

            if (enrolmentsResponse.IsSuccessStatusCode)
            {
                model.Enrolments =
                    await _api.ReadAsync<List<EnrolmentDto>>(enrolmentsResponse)
                    ?? new List<EnrolmentDto>();
            }

            // Retrieve event information.
            using var eventsResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/Events");

            if (eventsResponse.IsSuccessStatusCode)
            {
                model.Events =
                    await _api.ReadAsync<List<EventVm>>(eventsResponse)
                    ?? new List<EventVm>();
            }

            return View(model);
        }
    }
}