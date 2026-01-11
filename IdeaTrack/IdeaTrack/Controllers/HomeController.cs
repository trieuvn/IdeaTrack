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
                var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                foreach (var form in forms)
                {
                    if (string.IsNullOrEmpty(form.FileUrl)) continue;

                    // Handle local files
                    if (!form.FileUrl.StartsWith("http://") && !form.FileUrl.StartsWith("https://"))
                    {
                        // Use consistent path resolution logic
                        var cleanUrl = form.FileUrl.TrimStart('~').TrimStart('/').Replace('\\', '/');
                        var localPath = Path.Combine(wwwrootPath, cleanUrl);

                        if (System.IO.File.Exists(localPath))
                        {
                            // Ensure unique entry names in zip
                            var entryName = form.FileName ?? form.FormName;
                            if (string.IsNullOrEmpty(Path.GetExtension(entryName)))
                            {
                                entryName += Path.GetExtension(localPath);
                            }

                            // Check if entry already exists
                            if (archive.Entries.Any(e => e.FullName == entryName))
                            {
                                entryName = $"{Path.GetFileNameWithoutExtension(entryName)}_{Guid.NewGuid().ToString().Substring(0, 4)}{Path.GetExtension(entryName)}";
                            }

                            var entry = archive.CreateEntry(entryName);
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


    }
}
