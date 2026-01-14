using IdeaTrack.Data;
using IdeaTrack.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Services
{
    /// <summary>
    /// Service for retrieving leaderboard data for HomePage display.
    /// Rankings are calculated based on:
    /// - Initiatives: Percentage score (FinalScore / MaxScore)
    /// - Lecturers: Count of approved initiatives
    /// </summary>
    public interface ILeaderboardService
    {
        Task<LeaderboardVM> GetLeaderboardDataAsync(int topCount = 5, int? yearId = null, int? periodId = null);
        Task<List<FilterOption>> GetAvailableYearsAsync();
        Task<List<FilterOption>> GetAvailablePeriodsAsync(int? yearId = null);
    }

    /// <summary>
    /// Filter option for dropdowns
    /// </summary>
    public class FilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(ApplicationDbContext context, ILogger<LeaderboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get available academic years for filter dropdown.
        /// </summary>
        public async Task<List<FilterOption>> GetAvailableYearsAsync()
        {
            return await _context.AcademicYears
                .OrderByDescending(y => y.Id)
                .Select(y => new FilterOption
                {
                    Id = y.Id,
                    Name = y.Name,
                    IsActive = y.IsCurrent
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get available periods for filter dropdown.
        /// </summary>
        public async Task<List<FilterOption>> GetAvailablePeriodsAsync(int? yearId = null)
        {
            var query = _context.InitiativePeriods.AsQueryable();
            
            if (yearId.HasValue)
            {
                query = query.Where(p => p.AcademicYearId == yearId.Value);
            }

            return await query
                .OrderByDescending(p => p.StartDate)
                .Select(p => new FilterOption
                {
                    Id = p.Id,
                    Name = p.Name,
                    IsActive = p.IsActive
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get combined leaderboard data for HomePage with optional filters.
        /// </summary>
        public async Task<LeaderboardVM> GetLeaderboardDataAsync(int topCount = 5, int? yearId = null, int? periodId = null)
        {
            var vm = new LeaderboardVM();

            try
            {
                // Get current period info for display
                var currentPeriod = await _context.InitiativePeriods
                    .Include(p => p.AcademicYear)
                    .FirstOrDefaultAsync(p => p.IsActive);

                if (periodId.HasValue)
                {
                    var selectedPeriod = await _context.InitiativePeriods
                        .Include(p => p.AcademicYear)
                        .FirstOrDefaultAsync(p => p.Id == periodId.Value);
                    if (selectedPeriod != null)
                    {
                        vm.CurrentPeriodName = selectedPeriod.Name;
                        vm.CurrentAcademicYear = selectedPeriod.AcademicYear?.Name;
                    }
                }
                else if (yearId.HasValue)
                {
                    var selectedYear = await _context.AcademicYears.FindAsync(yearId.Value);
                    if (selectedYear != null)
                    {
                        vm.CurrentPeriodName = "All Periods";
                        vm.CurrentAcademicYear = selectedYear.Name;
                    }
                }
                else if (yearId == null && periodId == null)
                {
                    // All-time mode
                    vm.CurrentPeriodName = "All-time";
                    vm.CurrentAcademicYear = "";
                }
                else if (currentPeriod != null)
                {
                    vm.CurrentPeriodName = currentPeriod.Name;
                    vm.CurrentAcademicYear = currentPeriod.AcademicYear?.Name;
                }

                // Get top initiatives by % score
                vm.TopInitiatives = await GetTopInitiativesAsync(topCount, yearId, periodId);

                // Get top lecturers by approved count
                vm.TopLecturers = await GetTopLecturersAsync(topCount, yearId, periodId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaderboard data");
            }

            return vm;
        }

        /// <summary>
        /// Get top initiatives ranked by percentage score (FinalScore / MaxScore).
        /// </summary>
        private async Task<List<LeaderboardInitiativeItem>> GetTopInitiativesAsync(int topCount, int? yearId = null, int? periodId = null)
        {
            // Build query with filters
            var query = _context.FinalResults
                .Include(fr => fr.Initiative)
                    .ThenInclude(i => i.Creator)
                .Include(fr => fr.Initiative)
                    .ThenInclude(i => i.Category)
                        .ThenInclude(c => c.Period)
                .Include(fr => fr.Initiative)
                    .ThenInclude(i => i.Category)
                        .ThenInclude(c => c.Template)
                            .ThenInclude(t => t.CriteriaList)
                .Where(fr => fr.ChairmanDecision == "Approved");

            // Apply period filter
            if (periodId.HasValue)
            {
                query = query.Where(fr => fr.Initiative.Category.PeriodId == periodId.Value);
            }
            // Apply year filter (if no period specified)
            else if (yearId.HasValue)
            {
                query = query.Where(fr => fr.Initiative.Category.Period.AcademicYearId == yearId.Value);
            }

            var initiatives = await query
                .OrderByDescending(fr => fr.FinalScore ?? fr.AverageScore)
                .Take(topCount)
                .Select(fr => new
                {
                    fr.Initiative.InitiativeCode,
                    fr.Initiative.Title,
                    AuthorName = fr.Initiative.Creator.FullName ?? fr.Initiative.Creator.UserName,
                    CategoryName = fr.Initiative.Category.Name,
                    Score = fr.FinalScore ?? fr.AverageScore,
                    MaxScore = fr.Initiative.Category.Template != null
                        ? fr.Initiative.Category.Template.CriteriaList.Sum(c => c.MaxScore)
                        : 100m,
                    fr.Rank
                })
                .ToListAsync();

            var result = new List<LeaderboardInitiativeItem>();
            int rank = 1;

            foreach (var item in initiatives)
            {
                var maxScore = item.MaxScore > 0 ? item.MaxScore : 100m;
                var scorePercent = (item.Score / maxScore) * 100;

                result.Add(new LeaderboardInitiativeItem
                {
                    Rank = rank,
                    InitiativeCode = item.InitiativeCode,
                    Title = item.Title,
                    AuthorName = item.AuthorName ?? "N/A",
                    CategoryName = item.CategoryName,
                    ScorePercent = Math.Round(scorePercent, 1),
                    RankLabel = item.Rank ?? GetRankLabel(scorePercent)
                });
                rank++;
            }

            return result;
        }

        /// <summary>
        /// Get top lecturers ranked by number of approved initiatives.
        /// </summary>
        private async Task<List<LeaderboardLecturerItem>> GetTopLecturersAsync(int topCount, int? yearId = null, int? periodId = null)
        {
            // Build query with filters
            var query = _context.Initiatives
                .Include(i => i.Creator)
                    .ThenInclude(u => u.Department)
                .Include(i => i.FinalResult)
                .Include(i => i.Category)
                    .ThenInclude(c => c.Period)
                .Where(i => i.FinalResult != null && i.FinalResult.ChairmanDecision == "Approved");

            // Apply period filter
            if (periodId.HasValue)
            {
                query = query.Where(i => i.Category.PeriodId == periodId.Value);
            }
            // Apply year filter (if no period specified)
            else if (yearId.HasValue)
            {
                query = query.Where(i => i.Category.Period.AcademicYearId == yearId.Value);
            }

            var lecturers = await query
                .GroupBy(i => new
                {
                    i.CreatorId,
                    i.Creator.FullName,
                    i.Creator.UserName,
                    i.Creator.AvatarUrl,
                    DepartmentName = i.Creator.Department != null ? i.Creator.Department.Name : "N/A"
                })
                .Select(g => new
                {
                    g.Key.CreatorId,
                    g.Key.FullName,
                    g.Key.UserName,
                    g.Key.AvatarUrl,
                    g.Key.DepartmentName,
                    ApprovedCount = g.Count()
                })
                .OrderByDescending(x => x.ApprovedCount)
                .Take(topCount)
                .ToListAsync();

            var result = new List<LeaderboardLecturerItem>();
            int rank = 1;

            foreach (var item in lecturers)
            {
                result.Add(new LeaderboardLecturerItem
                {
                    Rank = rank,
                    FullName = item.FullName ?? item.UserName ?? "N/A",
                    DepartmentName = item.DepartmentName,
                    ApprovedCount = item.ApprovedCount,
                    AvatarUrl = item.AvatarUrl
                });
                rank++;
            }

            return result;
        }

        /// <summary>
        /// Get rank label based on percentage score.
        /// </summary>
        private static string GetRankLabel(decimal scorePercent)
        {
            return scorePercent switch
            {
                >= 90 => "Xuất sắc",
                >= 80 => "Tốt",
                >= 70 => "Khá",
                >= 50 => "Đạt",
                _ => "Không đạt"
            };
        }
    }
}
