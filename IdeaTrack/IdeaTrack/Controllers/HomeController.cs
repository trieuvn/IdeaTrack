using System.Diagnostics;
using System.IO.Compression;
using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Services;
using IdeaTrack.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger, 
            SignInManager<ApplicationUser> signInManager,
            ILeaderboardService leaderboardService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _signInManager = signInManager;
            _leaderboardService = leaderboardService;
            _context = context;
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

        public async Task<IActionResult> Privacy(int? yearId, int? periodId)
        {
            var today = DateTime.Today;

            // Get all active/open periods (based on IsActive flag or date range)
            var activePeriods = await _context.InitiativePeriods
                .Include(p => p.AcademicYear)
                .Where(p => p.IsActive || (p.StartDate <= today && p.EndDate >= today))
                .OrderByDescending(p => p.EndDate)
                .ToListAsync();

            // Get years that have active periods
            var yearsWithActivePeriods = activePeriods
                .Select(p => p.AcademicYear)
                .DistinctBy(y => y.Id)
                .OrderByDescending(y => y.Id)
                .Select(y => new AcademicYearOption
                {
                    Id = y.Id,
                    Name = y.Name,
                    IsCurrent = y.IsCurrent
                })
                .ToList();

            // Default to first year with active periods if no year selected
            if (!yearId.HasValue && yearsWithActivePeriods.Any())
            {
                yearId = yearsWithActivePeriods.First().Id;
            }

            // Get active periods for selected year
            var periodsForYear = activePeriods
                .Where(p => p.AcademicYearId == yearId)
                .Select(p => new PeriodOption
                {
                    Id = p.Id,
                    Name = p.Name,
                    IsActive = p.IsActive,
                    EndDate = p.EndDate
                })
                .ToList();

            // Default to first period if no period selected
            if (!periodId.HasValue && periodsForYear.Any())
            {
                periodId = periodsForYear.First().Id;
            }

            // Get reference forms for selected period
            var referenceForms = new List<ReferenceForm>();
            string? selectedPeriodName = null;

            if (periodId.HasValue)
            {
                referenceForms = await _context.ReferenceForms
                    .Where(rf => rf.PeriodId == periodId.Value)
                    .OrderBy(rf => rf.FormName)
                    .ToListAsync();

                var period = activePeriods.FirstOrDefault(p => p.Id == periodId.Value);
                selectedPeriodName = period?.Name;
            }

            // Get active period info for status card
            ActivePeriodInfo? activePeriodInfo = null;
            var currentActivePeriod = activePeriods.FirstOrDefault(p => p.StartDate <= today && p.EndDate >= today);
            if (currentActivePeriod != null)
            {
                activePeriodInfo = new ActivePeriodInfo
                {
                    PeriodName = currentActivePeriod.Name,
                    EndDate = currentActivePeriod.EndDate,
                    DaysRemaining = (currentActivePeriod.EndDate - today).Days,
                    IsOpen = true
                };
            }

            var viewModel = new PrivacyViewModel
            {
                AvailableYears = yearsWithActivePeriods,
                AvailablePeriods = periodsForYear,
                ReferenceForms = referenceForms,
                SelectedYearId = yearId,
                SelectedPeriodId = periodId,
                SelectedPeriodName = selectedPeriodName,
                ActivePeriodInfo = activePeriodInfo
            };

            return View(viewModel);
        }

        /// <summary>
        /// AJAX endpoint to get periods for a selected year (Privacy page)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPrivacyPeriods(int yearId)
        {
            var today = DateTime.Today;

            var periods = await _context.InitiativePeriods
                .Where(p => p.AcademicYearId == yearId && (p.IsActive || (p.StartDate <= today && p.EndDate >= today)))
                .OrderByDescending(p => p.EndDate)
                .Select(p => new { p.Id, p.Name, p.IsActive, p.EndDate })
                .ToListAsync();

            return Json(periods);
        }

        /// <summary>
        /// Download a single reference form file
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var form = await _context.ReferenceForms.FindAsync(id);
            if (form == null || string.IsNullOrEmpty(form.FileUrl))
            {
                _logger.LogWarning("Document with id {Id} not found or has no FileUrl", id);
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction(nameof(Privacy));
            }

            _logger.LogInformation("Attempting to download document: {FormName}, FileUrl: {FileUrl}", form.FormName, form.FileUrl);

            // Handle external URLs (http/https)
            if (form.FileUrl.StartsWith("http://") || form.FileUrl.StartsWith("https://"))
            {
                return Redirect(form.FileUrl);
            }

            // For local files, construct the physical path
            string localPath;
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // Handle various path formats:
            // 1. Already relative to wwwroot: "/templates/file.docx" or "templates/file.docx"
            // 2. Full path starting with wwwroot
            var cleanUrl = form.FileUrl.TrimStart('~').TrimStart('/').Replace('\\', '/');
            localPath = Path.Combine(wwwrootPath, cleanUrl);

            _logger.LogInformation("Resolved local path: {LocalPath}", localPath);

            if (!System.IO.File.Exists(localPath))
            {
                _logger.LogWarning("File not found at path: {LocalPath}", localPath);
                return NotFound($"File not found on server at {localPath}");
            }

            try
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(localPath);
                
                // Determine content type from file extension if FileType is not set
                var extension = Path.GetExtension(localPath).TrimStart('.').ToLower();
                var contentType = GetContentType(form.FileType ?? extension);
                
                // Determine filename
                var downloadFileName = form.FileName;
                if (string.IsNullOrEmpty(downloadFileName))
                {
                    downloadFileName = Path.GetFileName(localPath);
                }
                // Ensure file has proper extension
                if (!downloadFileName.Contains('.'))
                {
                    downloadFileName += "." + extension;
                }

                _logger.LogInformation("Serving file: {FileName}, ContentType: {ContentType}, Size: {Size} bytes", 
                    downloadFileName, contentType, fileBytes.Length);

                return File(fileBytes, contentType, downloadFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {LocalPath}", localPath);
                return BadRequest($"Error reading file: {ex.Message}");
            }
        }

        /// <summary>
        /// Download all documents for a period as ZIP
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadAllDocuments(int periodId)
        {
            var forms = await _context.ReferenceForms
                .Where(rf => rf.PeriodId == periodId)
                .ToListAsync();

            if (!forms.Any())
            {
                TempData["ErrorMessage"] = "No documents available for this period.";
                return RedirectToAction(nameof(Privacy));
            }

            var period = await _context.InitiativePeriods.FindAsync(periodId);
            var zipFileName = $"Documents_{period?.Name ?? "Period"}.zip".Replace(" ", "_");

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var form in forms)
                {
                    if (string.IsNullOrEmpty(form.FileUrl)) continue;

                    // Handle local files
                    if (!form.FileUrl.StartsWith("http"))
                    {
                        var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", form.FileUrl.TrimStart('/'));
                        if (System.IO.File.Exists(localPath))
                        {
                            var entry = archive.CreateEntry(form.FileName ?? form.FormName);
                            using var entryStream = entry.Open();
                            using var fileStream = System.IO.File.OpenRead(localPath);
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                    // For external URLs, we skip adding them to ZIP (too complex)
                }
            }

            memoryStream.Position = 0;
            return File(memoryStream.ToArray(), "application/zip", zipFileName);
        }

        private string GetContentType(string fileType)
        {
            return fileType.ToLower() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "doc" => "application/msword",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xls" => "application/vnd.ms-excel",
                _ => "application/octet-stream"
            };
        }

        public IActionResult About()
        {
            return View();
        }

        /*
        /// <summary>
        /// Admin action to reset reference forms data for testing
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ResetReferenceForms()
        {
            try
            {
                // Get the first active period
                var period = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
                if (period == null)
                {
                    return Content("No active period found.");
                }

                // Delete all existing reference forms
                var existingForms = await _context.ReferenceForms.ToListAsync();
                if (existingForms.Any())
                {
                    _context.ReferenceForms.RemoveRange(existingForms);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Deleted {Count} reference forms", existingForms.Count);
                }

                // Ensure docs directory exists
                var docsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs");
                if (!Directory.Exists(docsPath))
                {
                    Directory.CreateDirectory(docsPath);
                }

                // Create sample DOCX files (simple text content)
                var docxContent1 = "Form M01 - Đơn đăng ký sáng kiến\n\nHọ và tên: ________________\nĐơn vị: ________________\nTên sáng kiến: ________________\n\nNội dung sáng kiến:\n________________\n________________\n________________\n\nChữ ký: ________________";
                var docxContent2 = "Form M02 - Mô tả chi tiết sáng kiến\n\n1. Tên sáng kiến: ________________\n\n2. Lĩnh vực áp dụng: ________________\n\n3. Mô tả chi tiết:\n________________\n________________\n________________\n\n4. Hiệu quả mang lại:\n________________\n________________";

                await System.IO.File.WriteAllTextAsync(Path.Combine(docsPath, "Form_M01_DangKy.docx"), docxContent1);
                await System.IO.File.WriteAllTextAsync(Path.Combine(docsPath, "Form_M02_MoTa.docx"), docxContent2);

                // Create simple PDF files (text content for testing)
                var pdfContent1 = "%PDF-1.4\n1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n2 0 obj << /Type /Pages /Kids [] /Count 0 >> endobj\ntrailer << /Root 1 0 R >>\n%%EOF\n\nHướng dẫn nộp sáng kiến - Tài liệu hướng dẫn quy trình nộp và đánh giá sáng kiến.";
                var pdfContent2 = "%PDF-1.4\n1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n2 0 obj << /Type /Pages /Kids [] /Count 0 >> endobj\ntrailer << /Root 1 0 R >>\n%%EOF\n\nTiêu chí đánh giá sáng kiến - Bảng tiêu chí chấm điểm và xếp loại sáng kiến.";

                await System.IO.File.WriteAllTextAsync(Path.Combine(docsPath, "Huong_Dan.pdf"), pdfContent1);
                await System.IO.File.WriteAllTextAsync(Path.Combine(docsPath, "Tieu_Chi_Danh_Gia.pdf"), pdfContent2);

                // Add new reference forms with correct paths
                _context.ReferenceForms.AddRange(
                    new ReferenceForm { PeriodId = period.Id, FormName = "Form M01 - Đơn đăng ký", FileName = "Form_M01_DangKy.docx", FileType = "docx", Description = "Đơn đăng ký sáng kiến chính thức", FileUrl = "docs/Form_M01_DangKy.docx" },
                    new ReferenceForm { PeriodId = period.Id, FormName = "Form M02 - Mô tả sáng kiến", FileName = "Form_M02_MoTa.docx", FileType = "docx", Description = "Mẫu mô tả chi tiết sáng kiến", FileUrl = "docs/Form_M02_MoTa.docx" },
                    new ReferenceForm { PeriodId = period.Id, FormName = "Hướng dẫn nộp sáng kiến", FileName = "Huong_Dan.pdf", FileType = "pdf", Description = "Hướng dẫn quy trình nộp sáng kiến", FileUrl = "docs/Huong_Dan.pdf" },
                    new ReferenceForm { PeriodId = period.Id, FormName = "Tiêu chí đánh giá", FileName = "Tieu_Chi_Danh_Gia.pdf", FileType = "pdf", Description = "Tiêu chí chấm điểm sáng kiến", FileUrl = "docs/Tieu_Chi_Danh_Gia.pdf" }
                );
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reset reference forms for period {PeriodId}: {PeriodName}", period.Id, period.Name);

                return Content($"Successfully reset reference forms for period: {period.Name} (ID: {period.Id}). Created 4 documents in wwwroot/docs/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting reference forms");
                return Content($"Error: {ex.Message}");
            }
        }
        */
    }
}
