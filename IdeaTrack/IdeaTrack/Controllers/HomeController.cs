using System.Diagnostics;
using IdeaTrack.Models;
using IdeaTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILeaderboardService _leaderboardService;

        public HomeController(
            ILogger<HomeController> logger, 
            SignInManager<ApplicationUser> signInManager,
            ILeaderboardService leaderboardService)
        {
            _logger = logger;
            _signInManager = signInManager;
            _leaderboardService = leaderboardService;
        }

        public async Task<IActionResult> Index()
        {
            // Allow ALL users (authenticated or not) to access HomePage
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _signInManager.UserManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _signInManager.UserManager.GetRolesAsync(user);
                    
                    // Pass user info to view for dynamic portal button
                    ViewBag.UserName = user.FullName ?? user.UserName;
                    ViewBag.UserAvatar = user.AvatarUrl;
                    ViewBag.IsLoggedIn = true;
                    
                    // Determine portal URL and name based on role
                    if (roles.Contains("Admin"))
                    {
                        ViewBag.PortalName = "Admin Portal";
                        ViewBag.PortalUrl = "/Admin";
                    }
                    else if (roles.Contains("SciTech") || roles.Contains("OST_Admin"))
                    {
                        ViewBag.PortalName = "SciTech Portal";
                        ViewBag.PortalUrl = "/SciTech/Port";
                    }
                    else if (roles.Contains("FacultyLeader") || roles.Contains("Faculty_Admin"))
                    {
                        ViewBag.PortalName = "Faculty Portal";
                        ViewBag.PortalUrl = "/Faculty/Dashboard";
                    }
                    else if (roles.Contains("CouncilMember") || roles.Contains("Council_Member"))
                    {
                        ViewBag.PortalName = "Council Portal";
                        ViewBag.PortalUrl = "/Councils/Page";
                    }
                    else
                    {
                        // Default: Author/Lecturer
                        ViewBag.PortalName = "Author Portal";
                        ViewBag.PortalUrl = "/Author/Dashboard";
                    }
                }
            }
            
            // Fetch leaderboard data for all users (public) - default is all-time
            var leaderboard = await _leaderboardService.GetLeaderboardDataAsync(5);
            ViewBag.Leaderboard = leaderboard;
            
            // Pass filter options
            ViewBag.AvailableYears = await _leaderboardService.GetAvailableYearsAsync();
            ViewBag.AvailablePeriods = await _leaderboardService.GetAvailablePeriodsAsync();
            
            return View();
        }

        /// <summary>
        /// AJAX endpoint to get leaderboard data with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLeaderboardData(int? yearId, int? periodId)
        {
            var data = await _leaderboardService.GetLeaderboardDataAsync(5, yearId, periodId);
            return Json(data);
        }

        /// <summary>
        /// AJAX endpoint to get available periods for a year
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPeriods(int? yearId)
        {
            var periods = await _leaderboardService.GetAvailablePeriodsAsync(yearId);
            return Json(periods);
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
