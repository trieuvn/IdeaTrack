using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Areas.Councils.Controllers
{

    [Area("Councils")]
    public class PageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AssignedInitiatives()
        {
            return View();
        }
        public IActionResult History()
        {
            return View();
        }
        public IActionResult Details()
        {
            return View();
        }

    }
}
