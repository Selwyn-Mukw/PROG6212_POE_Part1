using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
