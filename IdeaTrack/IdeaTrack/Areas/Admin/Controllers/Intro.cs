using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class Intro : Controller
    {
        public IActionResult AuditLog()
        {
            return View();
        }
        public IActionResult User()
        {
            return View();

        }
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
