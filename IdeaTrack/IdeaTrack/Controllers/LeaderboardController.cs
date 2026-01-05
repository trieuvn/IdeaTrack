using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Controllers
{
    /// <summary>
    /// Public controller for leaderboards and reference forms download
    /// No authentication required
    /// </summary>
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ApplicationDbContext context, ILogger<LeaderboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Main leaderboard page - shows top initiatives and top authors
        /// </summary>
        public async Task<IActionResult> Index(int? periodId)
        {
            try
            {
                // Get available periods
                var periods = await _context.InitiativePeriods
                    .OrderByDescending(p => p.IsActive)
                    .ThenByDescending(p => p.CreatedAt)
                    .Select(p => new PeriodSelectItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

                // Use selected period or default to active period
                var selectedPeriodId = periodId ?? periods.FirstOrDefault(p => p.IsActive)?.Id ?? periods.FirstOrDefault()?.Id;
                var currentPeriod = periods.FirstOrDefault(p => p.Id == selectedPeriodId);

                var vm = new LeaderboardViewModel
                {
                    Periods = periods,
                    SelectedPeriodId = selectedPeriodId,
                    CurrentPeriodName = currentPeriod?.Name
                };

                if (selectedPeriodId.HasValue)
                {
                    // Get top initiatives (approved with final results)
                    vm.TopInitiatives = await _context.Initiatives
                        .AsNoTracking()
                        .Include(i => i.FinalResult)
                        .Include(i => i.Category)
                        .Include(i => i.Creator)
                            .ThenInclude(c => c.Department)
                        .Where(i => i.PeriodId == selectedPeriodId && 
                                   (i.Status == InitiativeStatus.Approved || i.Status == InitiativeStatus.Rejected) &&
                                   i.FinalResult != null)
                        .OrderByDescending(i => i.FinalResult!.AverageScore)
                        .Take(10)
                        .Select(i => new TopInitiativeItem
                        {
                            InitiativeId = i.Id,
                            InitiativeCode = i.InitiativeCode,
                            Title = i.Title,
                            CategoryName = i.Category.Name,
                            AuthorName = i.Creator.FullName,
                            DepartmentName = i.Creator.Department != null ? i.Creator.Department.Name : "",
                            AverageScore = i.FinalResult!.AverageScore,
                            FinalDecision = i.FinalResult.ChairmanDecision,
                            Status = i.Status
                        })
                        .ToListAsync();

                    // Add rank numbers
                    for (int i = 0; i < vm.TopInitiatives.Count; i++)
                    {
                        vm.TopInitiatives[i].Rank = i + 1;
                    }

                    // Get top authors
                    var authorStats = await _context.Initiatives
                        .AsNoTracking()
                        .Include(i => i.Creator)
                            .ThenInclude(c => c.Department)
                        .Include(i => i.FinalResult)
                        .Where(i => i.PeriodId == selectedPeriodId && i.FinalResult != null)
                        .GroupBy(i => new { i.CreatorId, i.Creator.FullName, DepartmentName = i.Creator.Department != null ? i.Creator.Department.Name : null })
                        .Select(g => new TopAuthorItem
                        {
                            AuthorId = g.Key.CreatorId,
                            AuthorName = g.Key.FullName,
                            DepartmentName = g.Key.DepartmentName,
                            TotalInitiatives = g.Count(),
                            ApprovedCount = g.Count(i => i.Status == InitiativeStatus.Approved),
                            TotalScore = g.Sum(i => i.FinalResult != null ? i.FinalResult.AverageScore : 0),
                            AverageScore = g.Average(i => i.FinalResult != null ? i.FinalResult.AverageScore : 0)
                        })
                        .OrderByDescending(a => a.TotalScore)
                        .Take(10)
                        .ToListAsync();

                    // Add rank numbers
                    for (int i = 0; i < authorStats.Count; i++)
                    {
                        authorStats[i].Rank = i + 1;
                    }

                    vm.TopAuthors = authorStats;
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading leaderboard");
                return View(new LeaderboardViewModel());
            }
        }

        /// <summary>
        /// Top initiatives page with more entries
        /// </summary>
        public async Task<IActionResult> TopInitiatives(int? periodId, int page = 1)
        {
            const int pageSize = 20;

            var periods = await _context.InitiativePeriods
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.CreatedAt)
                .Select(p => new PeriodSelectItem { Id = p.Id, Name = p.Name, IsActive = p.IsActive })
                .ToListAsync();

            var selectedPeriodId = periodId ?? periods.FirstOrDefault(p => p.IsActive)?.Id;

            var query = _context.Initiatives
                .AsNoTracking()
                .Include(i => i.FinalResult)
                .Include(i => i.Category)
                .Include(i => i.Creator)
                    .ThenInclude(c => c.Department)
                .Where(i => i.PeriodId == selectedPeriodId &&
                           (i.Status == InitiativeStatus.Approved || i.Status == InitiativeStatus.Rejected) &&
                           i.FinalResult != null)
                .OrderByDescending(i => i.FinalResult!.AverageScore);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new TopInitiativeItem
                {
                    InitiativeId = i.Id,
                    InitiativeCode = i.InitiativeCode,
                    Title = i.Title,
                    CategoryName = i.Category.Name,
                    AuthorName = i.Creator.FullName,
                    DepartmentName = i.Creator.Department != null ? i.Creator.Department.Name : "",
                    AverageScore = i.FinalResult!.AverageScore,
                    FinalDecision = i.FinalResult.ChairmanDecision,
                    Status = i.Status
                })
                .ToListAsync();

            // Add rank with pagination offset
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Rank = (page - 1) * pageSize + i + 1;
            }

            ViewBag.Periods = periods;
            ViewBag.SelectedPeriodId = selectedPeriodId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(items);
        }

        /// <summary>
        /// Top authors page with more entries
        /// </summary>
        public async Task<IActionResult> TopAuthors(int? periodId)
        {
            var periods = await _context.InitiativePeriods
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.CreatedAt)
                .Select(p => new PeriodSelectItem { Id = p.Id, Name = p.Name, IsActive = p.IsActive })
                .ToListAsync();

            var selectedPeriodId = periodId ?? periods.FirstOrDefault(p => p.IsActive)?.Id;

            var authorStats = await _context.Initiatives
                .AsNoTracking()
                .Include(i => i.Creator)
                    .ThenInclude(c => c.Department)
                .Include(i => i.FinalResult)
                .Where(i => i.PeriodId == selectedPeriodId && i.FinalResult != null)
                .GroupBy(i => new { i.CreatorId, i.Creator.FullName, DepartmentName = i.Creator.Department != null ? i.Creator.Department.Name : null })
                .Select(g => new TopAuthorItem
                {
                    AuthorId = g.Key.CreatorId,
                    AuthorName = g.Key.FullName,
                    DepartmentName = g.Key.DepartmentName,
                    TotalInitiatives = g.Count(),
                    ApprovedCount = g.Count(i => i.Status == InitiativeStatus.Approved),
                    TotalScore = g.Sum(i => i.FinalResult != null ? i.FinalResult.AverageScore : 0),
                    AverageScore = g.Average(i => i.FinalResult != null ? i.FinalResult.AverageScore : 0)
                })
                .OrderByDescending(a => a.TotalScore)
                .ToListAsync();

            // Add rank numbers
            for (int i = 0; i < authorStats.Count; i++)
            {
                authorStats[i].Rank = i + 1;
            }

            ViewBag.Periods = periods;
            ViewBag.SelectedPeriodId = selectedPeriodId;

            return View(authorStats);
        }

        /// <summary>
        /// Reference forms download page
        /// </summary>
        public async Task<IActionResult> ReferenceForms(int? periodId)
        {
            try
            {
                var periods = await _context.InitiativePeriods
                    .OrderByDescending(p => p.IsActive)
                    .ThenByDescending(p => p.CreatedAt)
                    .Select(p => new PeriodSelectItem { Id = p.Id, Name = p.Name, IsActive = p.IsActive })
                    .ToListAsync();

                var selectedPeriodId = periodId ?? periods.FirstOrDefault(p => p.IsActive)?.Id;
                var currentPeriod = periods.FirstOrDefault(p => p.Id == selectedPeriodId);

                var forms = selectedPeriodId.HasValue
                    ? await _context.ReferenceForms
                        .AsNoTracking()
                        .Where(f => f.PeriodId == selectedPeriodId)
                        .OrderBy(f => f.FileName)
                        .Select(f => new ReferenceFormDownloadItem
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            Description = f.Description,
                            FileUrl = f.FileUrl,
                            FileType = f.FileUrl.Contains(".pdf") ? "PDF" :
                                       f.FileUrl.Contains(".doc") ? "Word" :
                                       f.FileUrl.Contains(".xls") ? "Excel" : "File",
                            UploadedAt = f.UploadedAt
                        })
                        .ToListAsync()
                    : new List<ReferenceFormDownloadItem>();

                var vm = new ReferenceFormsDownloadViewModel
                {
                    Periods = periods,
                    SelectedPeriodId = selectedPeriodId,
                    PeriodName = currentPeriod?.Name,
                    Forms = forms
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reference forms");
                return View(new ReferenceFormsDownloadViewModel());
            }
        }
    }
}
