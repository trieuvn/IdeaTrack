using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    public class PortController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Result()
        {
            return View();

        }
        public IActionResult Approve()
        {
            return View();
        }
        public IActionResult Follow()
        {
            return View();
        }
        public IActionResult Rule()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
        public IActionResult User()
        {
                        return View();

        }
        public IActionResult Councils()
        {
            return View();
        }
    }
}
