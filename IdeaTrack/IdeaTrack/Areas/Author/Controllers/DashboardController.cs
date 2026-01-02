using IdeaTrack.Areas.Author.ViewModels;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.Author.Controllers
{
    [Area("Author")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Author/Dashboard/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                // Tạm thời lấy tất cả initiatives (sau này sẽ filter theo user đăng nhập)
                var initiatives = await _context.Initiatives
                    .Include(i => i.Category)
                    .Include(i => i.AcademicYear)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                var allInitiatives = await _context.Initiatives.ToListAsync();
                var approvedCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Approved);
                var totalCount = allInitiatives.Count;

                var viewModel = new AuthorDashboardViewModel
                {
                    UserName = "John", // Tạm thời hardcode
                    RecentInitiatives = initiatives,
                    DraftCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Draft),
                    SubmittedCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Pending),
                    ApprovedCount = approvedCount,
                    UnderReviewCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Reviewing || i.Status == InitiativeStatus.Dept_Review),
                    TotalScore = 845, // Demo value
                    SuccessRate = totalCount > 0 ? Math.Round((decimal)approvedCount / totalCount * 100, 0) : 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang Dashboard");
                TempData["ErrorMessage"] = "Không thể tải dữ liệu Dashboard. Vui lòng thử lại sau.";
                
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
