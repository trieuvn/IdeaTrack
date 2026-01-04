using System.Diagnostics;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            // Redirect authenticated users to their role-specific dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _signInManager.UserManager.GetRolesAsync(user);
                    
                    if (roles.Contains("Admin"))
                        return Redirect("/Admin");
                    if (roles.Contains("SciTech") || roles.Contains("OST_Admin"))
                        return Redirect("/SciTech/Port");
                    if (roles.Contains("FacultyLeader") || roles.Contains("Faculty_Admin"))
                        return Redirect("/Faculty/Dashboard");
                    if (roles.Contains("CouncilMember") || roles.Contains("Council_Member"))
                        return Redirect("/Councils/Page");
                    
                    // Default: Author/Lecturer
                    return Redirect("/Author/Dashboard");
                }
            }
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
