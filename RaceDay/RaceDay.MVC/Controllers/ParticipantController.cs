using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            // Check whether the user is logged in
            string? token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Only allow participants
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Participant")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.DisplayName =
                HttpContext.Session.GetString("DisplayName");

            return View();
        }
    }
}