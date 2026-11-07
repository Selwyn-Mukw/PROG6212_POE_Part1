using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;
using RaceDay.contracts.DTOs;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly RaceDayApiClient _api;

        public AuthController(RaceDayApiClient api)
        {
            _api = api;
        }

        // Display login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Process login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                "api/Auth/login",
                new LoginRequestDto
                {
                    Email = vm.Email,
                    Password = vm.Password
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");
                return View(vm);
            }

            var auth = await _api.ReadAsync<AuthResponseVm>(response);

            if (auth == null || string.IsNullOrWhiteSpace(auth.Token))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to complete login.");
                return View(vm);
            }

            HttpContext.Session.SetString("JwtToken", auth.Token);
            HttpContext.Session.SetInt32("UserId", auth.UserId);
            HttpContext.Session.SetString("Role", auth.Role);
            HttpContext.Session.SetString(
                "DisplayName",
                $"{auth.FirstName} {auth.LastName}");
            HttpContext.Session.SetString("Email", auth.Email);

            if (auth.Role == "Organiser")
                return RedirectToAction("Index", "Organiser");

            return RedirectToAction("Index", "Participant");
        }

        // Display registration page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Process registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            using var response = await _api.SendAsync(
                HttpMethod.Post,
                "api/Auth/register",
                new RegisterRequestDto
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.Email,
                    PhoneNumber = vm.PhoneNumber,
                    Password = vm.Password
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. Please check your details.");
                return View(vm);
            }

            TempData["SuccessMessage"] =
                "Registration successful. Please log in.";

            return RedirectToAction("Login");
        }

        // Log out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}