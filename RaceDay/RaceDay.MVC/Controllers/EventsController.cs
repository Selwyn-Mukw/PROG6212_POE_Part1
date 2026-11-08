using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using RaceDay.contracts.DTOs;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiClient _api;

        public EventsController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsParticipant()
        {
            string? token =
                HttpContext.Session.GetString("JwtToken");

            string? role =
                HttpContext.Session.GetString("Role");

            return !string.IsNullOrEmpty(token)
                   && role == "Participant";
        }

        // EVENT DETAILS AND AVAILABLE CATEGORIES
        public async Task<IActionResult> Details(int id)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var eventResponse = await _api.SendAsync(
                HttpMethod.Get,
                $"api/Events/{id}");

            if (!eventResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var eventDetails =
                await _api.ReadAsync<EventVm>(eventResponse);

            if (eventDetails == null)
            {
                return NotFound();
            }

            using var categoryResponse = await _api.SendAsync(
                HttpMethod.Get,
                "api/Categories");

            if (!categoryResponse.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to retrieve event categories.";
            }

            var allCategories =
                await _api.ReadAsync<List<CategoryDto>>(categoryResponse)
                ?? new List<CategoryDto>();

            var model = new EventDetailsVm
            {
                Event = eventDetails,

                Categories = allCategories
                    .Where(c => c.EventId == id)
                    .ToList()
            };

            return View(model);
        }

        // PARTICIPANT ENROLMENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(
            int eventId,
            int categoryId)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (eventId <= 0 || categoryId <= 0)
            {
                TempData["Error"] = "Please select a valid category.";

                return RedirectToAction("Details", new { id = eventId });
            }

            var request = new CreateEnrolmentDto
            {
                EventId = eventId,
                CategoryId = categoryId
            };

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                "api/Enrolments",
                request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "You have successfully enrolled in this event!";

                return RedirectToAction("Details", new { id = eventId });
            }

            string responseBody =
                await response.Content.ReadAsStringAsync();

            string errorMessage =
                "Unable to complete your enrolment. Please try again.";

            try
            {
                using var document = JsonDocument.Parse(responseBody);

                if (document.RootElement.TryGetProperty(
                    "message", out var message))
                {
                    errorMessage = message.GetString() ?? errorMessage;
                }
            }
            catch (JsonException)
            {
                // Use the default error message.
            }

            TempData["Error"] = errorMessage;

            return RedirectToAction("Details", new { id = eventId });
        }
    }
}