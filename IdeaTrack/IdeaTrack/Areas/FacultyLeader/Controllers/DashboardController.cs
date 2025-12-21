using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Areas.FacultyLeader.Controllers
{
    [Area("FacultyLeader")]
    public class DashboardController : Controller
    {

        
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Review()
        {
            return View();
        }
        public IActionResult Details()
        {
            return View();
        }
        public IActionResult Progress()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }
    }
}
