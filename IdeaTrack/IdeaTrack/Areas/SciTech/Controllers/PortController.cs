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
    }
}
