using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
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

        public IActionResult Events()
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }
    }
}