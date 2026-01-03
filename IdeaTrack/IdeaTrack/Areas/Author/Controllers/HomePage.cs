using IdeaTrack.Areas.Author.ViewModels;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.Author.Controllers
{
    [Area("Author")]
    [Authorize(Roles = "Author,Lecturer,Admin")]
    public class HomePage : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomePage> _logger;

        public HomePage(ApplicationDbContext context, ILogger<HomePage> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET: /Author/HomePage/Files
        public async Task<IActionResult> Files()
        {
            try
            {
                var files = await _context.InitiativeFiles
                    .Include(f => f.Initiative)
                    .OrderByDescending(f => f.UploadDate)
                    .ToListAsync();

                return View(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách files");
                TempData["ErrorMessage"] = "Không thể tải danh sách files. Vui lòng thử lại sau.";
                return View(new List<InitiativeFile>());
            }
        }

        // GET: /Author/HomePage/Attachments/5
        public async Task<IActionResult> Attachments(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attachments action called with null id");
                return NotFound();
            }

            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Files)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (initiative == null)
                {
                    _logger.LogWarning("Initiative with id {InitiativeId} not found for attachments", id);
                    return NotFound();
                }

                return View(initiative);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang attachments cho sáng kiến {InitiativeId}", id);
                TempData["ErrorMessage"] = "Không thể tải trang attachments. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: /Author/HomePage/Setting
        public async Task<IActionResult> Setting()
        {
            try
            {
                // Temporarily get first user (should use authenticated user in production)
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync();

                var viewModel = new SettingViewModel
                {
                    FullName = user?.FullName ?? "Guest",
                    Email = user?.Email ?? "",
                    AvatarUrl = user?.AvatarUrl,
                    Position = user?.Position,
                    Department = user?.Department?.Name,
                    AcademicRank = user?.AcademicRank,
                    Degree = user?.Degree,
                    NotificationsEnabled = true,
                    EmailAlertsEnabled = false
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang cài đặt");
                TempData["ErrorMessage"] = "Không thể tải trang cài đặt. Vui lòng thử lại sau.";
                
                // Return empty view model to prevent crash
                var emptyViewModel = new SettingViewModel
                {
                    FullName = "Guest",
                    Email = "",
                    NotificationsEnabled = true,
                    EmailAlertsEnabled = false
                };
                
                return View(emptyViewModel);
            }
        }

        // GET: /Author/HomePage/DailyReport
        public async Task<IActionResult> DailyReport()
        {
            try
            {
                var allInitiatives = await _context.Initiatives
                    .Include(i => i.Category)
                    .Include(i => i.Creator)
                    .ToListAsync();

                var totalCount = allInitiatives.Count;
                
                // Category distribution
                var categoryColors = new[] { "#3b82f6", "#60a5fa", "#93c5fd", "#bfdbfe", "#dbeafe" };
                var categoryGroups = allInitiatives
                    .GroupBy(i => i.Category?.Name ?? "Uncategorized")
                    .Select((g, index) => new CategoryDistribution
                    {
                        CategoryName = g.Key,
                        Count = g.Count(),
                        Percentage = totalCount > 0 ? Math.Round((decimal)g.Count() / totalCount * 100, 1) : 0,
                        Color = categoryColors[index % categoryColors.Length]
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList();

                // Today's submissions
                var today = DateTime.Today;
                var todaySubmissions = allInitiatives
                    .Where(i => i.CreatedAt.Date == today)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToList();

                // Recent submissions (last 7 days)
                var weekAgo = today.AddDays(-7);
                var recentSubmissions = allInitiatives
                    .Where(i => i.CreatedAt.Date >= weekAgo)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(10)
                    .ToList();

                // Monthly stats
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthlyInitiatives = allInitiatives.Where(i => i.CreatedAt >= monthStart).ToList();

                var viewModel = new DailyReportViewModel
                {
                    TotalInitiatives = totalCount,
                    CategoryDistributions = categoryGroups,
                    TodaySubmissions = todaySubmissions,
                    RecentSubmissions = recentSubmissions,
                    SelectedDate = today,
                    MonthlyTotal = monthlyInitiatives.Count,
                    MonthlyApproved = monthlyInitiatives.Count(i => i.Status == InitiativeStatus.Approved),
                    MonthlyPending = monthlyInitiatives.Count(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Evaluating)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải báo cáo hàng ngày");
                TempData["ErrorMessage"] = "Không thể tải báo cáo. Vui lòng thử lại sau.";
                
                // Return empty view model
                var emptyViewModel = new DailyReportViewModel
                {
                    TotalInitiatives = 0,
                    CategoryDistributions = new List<CategoryDistribution>(),
                    TodaySubmissions = new List<Initiative>(),
                    RecentSubmissions = new List<Initiative>(),
                    SelectedDate = DateTime.Today,
                    MonthlyTotal = 0,
                    MonthlyApproved = 0,
                    MonthlyPending = 0
                };
                
                return View(emptyViewModel);
            }
        }

        private bool InitiativeExists(int id)
        {
            return _context.Initiatives.Any(e => e.Id == id);
        }
    }
}
