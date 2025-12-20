using IdeaTrack.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IdeaTrack.Areas.Author.Controllers
{
    [Area("Author")]
    public class HomePage : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Attachments()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult History()
        {
            return View();
        }
        public IActionResult Setting()
        {
            return View();
        }
        public IActionResult Detail()
        {
            return View();
        }
        public IActionResult Files()
        {
            return View();
        }
        public IActionResult DailyReport()
        {
            return View();
        }
    }
}
