using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class ProfileController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ProfileController(RaceDayApiClient api)
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

        // VIEW PROFILE
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Profile/me");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to retrieve your profile.";
                return View();
            }

            var profile =
                await _api.ReadAsync<ProfileVm>(response);

            return View(profile);
        }

        // EDIT PROFILE
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Profile/me");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to retrieve your profile.";
                return RedirectToAction("Index");
            }

            var profile =
                await _api.ReadAsync<ProfileVm>(response);

            if (profile == null)
            {
                return RedirectToAction("Index");
            }

            var model = new EditProfileVm
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = profile.PhoneNumber
            };

            return View(model);
        }

        // SAVE PROFILE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileVm model)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var response = await _api.SendAsync(
                HttpMethod.Put,
                "api/Profile/me",
                model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to update your profile.";
                return View(model);
            }

            TempData["Success"] =
                "Your profile was updated successfully.";

            return RedirectToAction("Index");
        }
    }
}