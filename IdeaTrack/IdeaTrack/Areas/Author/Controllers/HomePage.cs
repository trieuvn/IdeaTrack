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
    public class HomePage : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomePage> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomePage(ApplicationDbContext context, ILogger<HomePage> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
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
                _logger.LogError(ex, "Error loading files list");
                TempData["ErrorMessage"] = "Cannot load files list. Please try again later.";
                return View(new List<InitiativeFile>());
            }
        }

        // GET: /Author/HomePage/DownloadGuideline
        public IActionResult DownloadGuideline()
        {
            try
            {
                var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var filePath = Path.Combine(webRoot, "templates", "guideline.docx");

                if (!System.IO.File.Exists(filePath))
                {
                    // Create directory if not exists
                    var templateDir = Path.Combine(webRoot, "templates");
                    if (!Directory.Exists(templateDir))
                    {
                        Directory.CreateDirectory(templateDir);
                    }
                    
                    // In a real app we'd have the file. For now, create a dummy file to prevent crash
                    // or just return not found with message
                    _logger.LogWarning("Guideline file not found at {Path}", filePath);
                    TempData["ErrorMessage"] = "Guideline file is currently unavailable.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "IdeaTrack_Guideline.docx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading guideline");
                TempData["ErrorMessage"] = "Error downloading file.";
                return RedirectToAction("Index", "Dashboard");
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
                _logger.LogError(ex, "Error khi tai trang attachments cho sang kien {InitiativeId}", id);
                TempData["ErrorMessage"] = "Khong the tai trang attachments. Vui long thu lai sau.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: /Author/HomePage/Setting
        public async Task<IActionResult> Setting()
        {
            try
            {
                // Get the authenticated user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                // Load department info
                await _context.Entry(user).Reference(u => u.Department).LoadAsync();

                var viewModel = new SettingViewModel
                {
                    FullName = user.FullName ?? "Guest",
                    Email = user.Email ?? "",
                    AvatarUrl = user.AvatarUrl,
                    Position = user.Position,
                    Department = user.Department?.Name,
                    AcademicRank = user.AcademicRank,
                    Degree = user.Degree,
                    NotificationsEnabled = true,
                    EmailAlertsEnabled = false
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi tai trang cai dat");
                TempData["ErrorMessage"] = "Khong the tai trang cai dat. Vui long thu lai sau.";
                
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

        // POST: /Author/HomePage/UpdateProfile (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Update user properties
                user.FullName = request.FullName;
                user.Position = request.Position;
                user.AcademicRank = request.AcademicRank;
                user.Degree = request.Degree;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} updated profile successfully", user.Id);
                    return Json(new { success = true, message = "Profile updated successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to update profile for user {UserId}: {Errors}", user.Id, errors);
                    return Json(new { success = false, message = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return Json(new { success = false, message = "An error occurred while updating profile" });
            }
        }

        // POST: /Author/HomePage/ChangePassword (AJAX)
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} changed password successfully", user.Id);
                    return Json(new { success = true, message = "Password changed successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to change password for user {UserId}: {Errors}", user.Id, errors);
                    return Json(new { success = false, message = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return Json(new { success = false, message = "An error occurred while changing password" });
            }
        }

        // GET: /Author/HomePage/DailyReport
        public async Task<IActionResult> DailyReport()
        {
            try
            {
                // Get current user ID for data isolation (security fix)
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
                {
                    TempData["ErrorMessage"] = "Please log in to view your report.";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Only show initiatives belonging to current user (creator or co-author)
                var allInitiatives = await _context.Initiatives
                    .Include(i => i.Category)
                    .Include(i => i.Creator)
                    .Include(i => i.Authorships)
                    .Where(i => i.CreatorId == currentUserId || i.Authorships.Any(a => a.AuthorId == currentUserId))
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
                _logger.LogError(ex, "Error khi tai bao cao hang ngay");
                TempData["ErrorMessage"] = "Khong the tai bao cao. Vui long thu lai sau.";
                
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
