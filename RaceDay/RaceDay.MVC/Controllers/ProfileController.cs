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

        // UPLOAD PROFILE PICTURE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile file)
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (file == null || file.Length == 0)
            {
                TempData["UploadError"] =
                    "Please select an image.";

                return RedirectToAction("Edit");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["UploadError"] =
                    "Image must not exceed 5 MB.";

                return RedirectToAction("Edit");
            }

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(file.ContentType))
            {
                TempData["UploadError"] =
                    "Only JPG, PNG and WebP images are allowed.";

                return RedirectToAction("Edit");
            }

            using var response = await _api.UploadFileAsync(
                "api/Profile/upload-picture",
                file);

            if (!response.IsSuccessStatusCode)
            {
                string error =
                    await response.Content.ReadAsStringAsync();

                TempData["UploadError"] =
                    "Unable to upload your profile picture. " + error;

                return RedirectToAction("Edit");
            }

            TempData["UploadSuccess"] =
                "Your profile picture was uploaded successfully.";

            return RedirectToAction("Index");
        }
        // DISPLAY PROFILE PICTURE
        [HttpGet]
        public async Task<IActionResult> Picture()
        {
            if (!IsParticipant())
            {
                return Unauthorized();
            }

            using var response = await _api.SendAsync(
                HttpMethod.Get,
                "api/Profile/picture");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var content = await response.Content.ReadAsByteArrayAsync();

            var contentType = response.Content.Headers.ContentType?.MediaType
                              ?? "application/octet-stream";

            var allowedTypes = new[]
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            if (!allowedTypes.Contains(contentType))
            {
                return NotFound();
            }

            return File(content, contentType);
        }
    }
}