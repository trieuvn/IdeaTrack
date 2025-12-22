using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeDashboard : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
