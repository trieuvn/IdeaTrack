using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Areas.FacultyLeader.Controllers
{
    public class DashboardController : Controller
    {
        [Area("FacultyLeader")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
