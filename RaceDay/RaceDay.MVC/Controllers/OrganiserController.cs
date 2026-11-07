using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }
        private bool IsOrganiser()
        {
            string? token = HttpContext.Session.GetString("JwtToken");
            string? role = HttpContext.Session.GetString("Role");

            return !string.IsNullOrEmpty(token) && role == "Organiser";
        }

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
    }
}