using IdeaTrack.Areas.Author.ViewModels;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.Author.Controllers
{
    [Area("Author")]
    [Authorize(Roles = "Author,Lecturer,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<DashboardController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Author/Dashboard/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? 0;

                // Get initiatives where user is creator or co-author
                var allInitiatives = await _context.Initiatives
                    .Include(i => i.Category)
                    .Include(i => i.Authorships)
                    .Where(i => i.CreatorId == userId || i.Authorships.Any(a => a.AuthorId == userId))
                    .ToListAsync();

                var recentInitiatives = allInitiatives
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(10)
                    .ToList();

                var approvedCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Approved);
                var totalCount = allInitiatives.Count;

                var viewModel = new AuthorDashboardViewModel
                {
                    UserName = currentUser?.FullName ?? "Giang vien",
                    RecentInitiatives = recentInitiatives,
                    DraftCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Draft),
                    SubmittedCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Pending),
                    ApprovedCount = approvedCount,
                    UnderReviewCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Evaluating || i.Status == InitiativeStatus.Faculty_Approved),
                    TotalScore = 845, // Demo value - will be calculated from FinalResults
                    SuccessRate = totalCount > 0 ? Math.Round((decimal)approvedCount / totalCount * 100, 0) : 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi tai trang Dashboard");
                TempData["ErrorMessage"] = "Khong the tai du lieu Dashboard. Vui long thu lai sau.";
                
                // Return empty view model to prevent crash
                var emptyViewModel = new AuthorDashboardViewModel
                {
                    UserName = "Guest",
                    RecentInitiatives = new List<Initiative>(),
                    DraftCount = 0,
                    SubmittedCount = 0,
                    ApprovedCount = 0,
                    UnderReviewCount = 0,
                    TotalScore = 0,
                    SuccessRate = 0
                };
                
                return View(emptyViewModel);
            }
        }
    }
}
