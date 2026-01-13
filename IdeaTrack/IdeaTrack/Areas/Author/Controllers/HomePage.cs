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
        public async Task<IActionResult> DailyReport(
            InitiativeStatus? status,
            int? categoryId,
            int? yearId,
            int? periodId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                // Get current user ID for data isolation
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
                {
                    TempData["ErrorMessage"] = "Please log in to view your report.";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Base query: initiatives where user is creator or co-author
                var query = _context.Initiatives
                    .Include(i => i.Category)
                        .ThenInclude(c => c.Period)
                            .ThenInclude(p => p.AcademicYear)
                    .Include(i => i.Creator)
                    .Include(i => i.Authorships)
                    .Where(i => i.CreatorId == currentUserId || i.Authorships.Any(a => a.AuthorId == currentUserId));

                // --- Apply Filters ---
                if (status.HasValue)
                {
                    query = query.Where(i => i.Status == status.Value);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(i => i.CategoryId == categoryId.Value);
                }

                if (yearId.HasValue)
                {
                    query = query.Where(i => i.Category.Period.AcademicYearId == yearId.Value);
                }

                if (periodId.HasValue)
                {
                    query = query.Where(i => i.Category.PeriodId == periodId.Value);
                }

                // Date Range (default: all time if not specified)
                // Note: user requested Date Range filter. If provided, filter by CreatedAt
                if (fromDate.HasValue)
                {
                    var start = fromDate.Value.Date;
                    query = query.Where(i => i.CreatedAt >= start);
                }
                
                if (toDate.HasValue)
                {
                    var end = toDate.Value.Date.AddDays(1).AddTicks(-1); // Enc day
                    query = query.Where(i => i.CreatedAt <= end);
                }

                var filteredInitiatives = await query.ToListAsync();
                var totalCount = filteredInitiatives.Count;

                // Category distribution
                var categoryColors = new[] { "#3b82f6", "#60a5fa", "#93c5fd", "#bfdbfe", "#dbeafe" };
                var categoryGroups = filteredInitiatives
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

                // Today's submissions (filtered subset)
                var today = DateTime.Today;
                var todaySubmissions = filteredInitiatives
                    .Where(i => i.CreatedAt.Date == today)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToList();

                // Recent submissions (last 7 days - or filtered range)
                // If date filter is active, show what's in the list. Otherwise default to 7 days
                var recentSubmissions = filteredInitiatives
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(fromDate.HasValue || toDate.HasValue ? 50 : 10) // Show more if filtering by date
                    .ToList();

                // Monthly stats (based on filtered data)
                // If filtering by date, these stats reflect the filtered period.
                // If not filtering by date, reflect "current month" of the filtered set?
                // Request implies filters apply to ALL stats. So we just count from filteredInitiatives directly
                // but usually "Monthly" means "This Month". 
                // However, user said "Filters will apply to all statistics".
                // If I select "Year 2024", "Monthly" stats for "Current Month (Jan 2026)" might be 0.
                // To keep it simple and consistent with "Monthly" label, I will keep "This Month" logic 
                // BUT applied to the filtered dataset.
                var monthStart = new DateTime(today.Year, today.Month, 1);
                var monthlyInitiatives = filteredInitiatives
                    .Where(i => i.CreatedAt >= monthStart)
                    .ToList();

                // Initializing Dropdowns
                var statuses = Enum.GetValues(typeof(InitiativeStatus))
                    .Cast<InitiativeStatus>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() })
                    .ToList();
                
                var categoryQuery = _context.InitiativeCategories.AsQueryable();
                if (yearId.HasValue)
                {
                    categoryQuery = categoryQuery.Where(c => c.Period.AcademicYearId == yearId.Value);
                }
                if (periodId.HasValue)
                {
                    categoryQuery = categoryQuery.Where(c => c.PeriodId == periodId.Value);
                }
                var categories = await categoryQuery.OrderBy(c => c.Name).ToListAsync();
                var years = await _context.AcademicYears.OrderByDescending(y => y.Name).ToListAsync();
                // Get periods based on selected Year if any, else all
                var periodQuery = _context.InitiativePeriods.AsQueryable();
                if (yearId.HasValue)
                {
                    periodQuery = periodQuery.Where(p => p.AcademicYearId == yearId.Value);
                }
                var periods = await periodQuery.OrderByDescending(p => p.StartDate).ToListAsync();

                var viewModel = new DailyReportViewModel
                {
                    TotalInitiatives = totalCount,
                    CategoryDistributions = categoryGroups,
                    TodaySubmissions = todaySubmissions,
                    RecentSubmissions = recentSubmissions,
                    SelectedDate = today,
                    MonthlyTotal = monthlyInitiatives.Count,
                    MonthlyApproved = monthlyInitiatives.Count(i => i.Status == InitiativeStatus.Approved),
                    MonthlyPending = monthlyInitiatives.Count(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Evaluating),
                    
                    // Set Filter Values
                    SelectedStatus = status,
                    SelectedCategoryId = categoryId,
                    SelectedYearId = yearId,
                    SelectedPeriodId = periodId,
                    FromDate = fromDate,
                    ToDate = toDate,

                    // Set Dropdowns
                    Statuses = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(statuses, "Id", "Name", status),
                    Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name", categoryId),
                    Years = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(years, "Id", "Name", yearId),
                    Periods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(periods, "Id", "Name", periodId)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading daily report");
                TempData["ErrorMessage"] = "Cannot load report. Please try again later.";
                
                // Return empty view model
                return View(new DailyReportViewModel());
            }
        }

        // GET: /Author/HomePage/ExportDailyReport
        public async Task<IActionResult> ExportDailyReport(
            InitiativeStatus? status,
            int? categoryId,
            int? yearId,
            int? periodId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                // Re-apply same filters
                var query = _context.Initiatives
                    .Include(i => i.Category)
                        .ThenInclude(c => c.Period)
                            .ThenInclude(p => p.AcademicYear)
                    .Include(i => i.Creator)
                    .Include(i => i.Authorships)
                    .Where(i => i.CreatorId == currentUserId || i.Authorships.Any(a => a.AuthorId == currentUserId));

                if (status.HasValue) query = query.Where(i => i.Status == status.Value);
                if (categoryId.HasValue) query = query.Where(i => i.CategoryId == categoryId.Value);
                if (yearId.HasValue) query = query.Where(i => i.Category.Period.AcademicYearId == yearId.Value);
                if (periodId.HasValue) query = query.Where(i => i.Category.PeriodId == periodId.Value);
                if (fromDate.HasValue) query = query.Where(i => i.CreatedAt >= fromDate.Value.Date);
                if (toDate.HasValue) query = query.Where(i => i.CreatedAt <= toDate.Value.Date.AddDays(1).AddTicks(-1));

                var initiatives = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

                // Export to Excel using EPPlus
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Daily Report");

                    // Headers
                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Code";
                    worksheet.Cells[1, 3].Value = "Title";
                    worksheet.Cells[1, 4].Value = "Category";
                    worksheet.Cells[1, 5].Value = "Status";
                    worksheet.Cells[1, 6].Value = "Created Date";
                    worksheet.Cells[1, 7].Value = "Creator";

                    // Style Header
                    using (var range = worksheet.Cells[1, 1, 1, 7])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Data
                    for (int i = 0; i < initiatives.Count; i++)
                    {
                        var item = initiatives[i];
                        worksheet.Cells[i + 2, 1].Value = item.Id;
                        worksheet.Cells[i + 2, 2].Value = item.InitiativeCode;
                        worksheet.Cells[i + 2, 3].Value = item.Title;
                        worksheet.Cells[i + 2, 4].Value = item.Category?.Name ?? "N/A";
                        worksheet.Cells[i + 2, 5].Value = item.Status.ToString();
                        worksheet.Cells[i + 2, 6].Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                        worksheet.Cells[i + 2, 7].Value = item.Creator?.FullName ?? "N/A";
                    }

                    worksheet.Cells.AutoFitColumns();

                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var fileName = $"DailyReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    var content = package.GetAsByteArray();

                    return File(content, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting daily report");
                TempData["ErrorMessage"] = "Could not export report.";
                return RedirectToAction("DailyReport");
            }
        }

        private bool InitiativeExists(int id)
        {
            return _context.Initiatives.Any(e => e.Id == id);
        }
    }
}
