using IdeaTrack.Areas.Councils.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdeaTrack.Areas.Councils.Controllers
{

    [Area("Councils")]
    //[Authorize]
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PageController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int GetCurrentUserIdForTest() => 1;

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserIdForTest();
            var now = DateTime.Now;

            var baseQuery = _db.InitiativeAssignments
                .AsNoTracking()
                .Where(a => a.MemberId == userId);

            var totalAssigned = await baseQuery.CountAsync();
            var completed = await baseQuery.CountAsync(a => a.Status == AssignmentStatus.Completed);
            var pending = totalAssigned - completed;

            var vm = new DashboardVM
            {
                TotalAssigned = totalAssigned,
                CompletedCount = completed,
                PendingCount = pending,
                ProgressPercentage = totalAssigned == 0 ? 0 : (int)Math.Round((decimal)completed / totalAssigned * 100, 0, MidpointRounding.AwayFromZero)
            };

            vm.UpNextList = await baseQuery
                .Where(a => a.Status != AssignmentStatus.Completed)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Category)
                .OrderBy(a => a.DueDate == null)
                .ThenBy(a => a.DueDate)
                .ThenBy(a => a.AssignedDate)
                .Select(a => new DashboardItem
                {
                    AssignmentId = a.Id,
                    InitiativeCode = a.Initiative.InitiativeCode,
                    Title = a.Initiative.Title,
                    CategoryName = a.Initiative.Category.Name,
                    Timestamp = a.AssignedDate,
                    DueDate = a.DueDate,
                    Status = a.Status,
                    Score = a.EvaluationDetails.Select(d => (decimal?)d.ScoreGiven).Sum()
                })
                .Take(3)
                .ToListAsync();

            vm.RecentActivityList = await baseQuery
                .Where(a => a.Status == AssignmentStatus.Completed)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Category)
                .Include(a => a.EvaluationDetails)
                .OrderByDescending(a => a.DecisionDate ?? a.AssignedDate)
                .Select(a => new DashboardItem
                {
                    AssignmentId = a.Id,
                    InitiativeCode = a.Initiative.InitiativeCode,
                    Title = a.Initiative.Title,
                    CategoryName = a.Initiative.Category.Name,
                    Timestamp = a.DecisionDate ?? a.AssignedDate,
                    DueDate = a.DueDate,
                    Status = a.Status,
                    Score = a.EvaluationDetails.Select(d => (decimal?)d.ScoreGiven).Sum()
                })
                .Take(3)
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> AssignedInitiatives(string? keyword, string status = "Assigned", string sortOrder = "Deadline", int page = 1)
        {
            var userId = GetCurrentUserIdForTest();

            var vm = new AssignedListVM
            {
                Keyword = keyword,
                Status = string.IsNullOrWhiteSpace(status) ? "Assigned" : status,
                SortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "Deadline" : sortOrder,
                CurrentPage = page <= 0 ? 1 : page
            };

            var query = _db.InitiativeAssignments
                .AsNoTracking()
                .Where(a => a.MemberId == userId)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.Keyword))
            {
                var kw = vm.Keyword.Trim();
                query = query.Where(a => a.Initiative.Title.Contains(kw) || a.Initiative.InitiativeCode.Contains(kw));
            }

            switch (vm.Status)
            {
                case "All":
                    break;
                case "Completed":
                    query = query.Where(a => a.Status == AssignmentStatus.Completed);
                    break;
                case "Assigned":
                default:
                    query = query.Where(a => a.Status != AssignmentStatus.Completed);
                    vm.Status = "Assigned";
                    break;
            }

            query = vm.SortOrder switch
            {
                "Newest" => query.OrderByDescending(a => a.AssignedDate),
                _ => query
                    .OrderBy(a => a.DueDate == null)
                    .ThenBy(a => a.DueDate)
                    .ThenByDescending(a => a.AssignedDate)
            };

            vm.TotalCount = await query.CountAsync();
            vm.TotalPages = (int)Math.Ceiling(vm.TotalCount / (double)vm.PageSize);
            if (vm.TotalPages == 0) vm.TotalPages = 1;
            if (vm.CurrentPage > vm.TotalPages) vm.CurrentPage = vm.TotalPages;

            vm.Items = await query
                .Skip((vm.CurrentPage - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .Select(a => new AssignedListItem
                {
                    AssignmentId = a.Id,
                    InitiativeId = a.InitiativeId,
                    InitiativeCode = a.Initiative.InitiativeCode,
                    Title = a.Initiative.Title,
                    CategoryName = a.Initiative.Category.Name,
                    AssignedDate = a.AssignedDate,
                    DueDate = a.DueDate,
                    Status = a.Status
                })
                .ToListAsync();

            return View(vm);
        }


        public async Task<IActionResult> History(string? keyword, string sortOrder = "Newest", int page = 1)
        {
            var userId = GetCurrentUserIdForTest();

            var vm = new AssignedListVM
            {
                Keyword = keyword,
                Status = "Completed", // History chỉ hiển thị đã hoàn thành
                SortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "Newest" : sortOrder,
                CurrentPage = page <= 0 ? 1 : page
            };

            var query = _db.InitiativeAssignments
                .AsNoTracking()
                .Where(a => a.MemberId == userId && a.Status == AssignmentStatus.Completed)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Category)
                .Include(a => a.EvaluationDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.Keyword))
            {
                var kw = vm.Keyword.Trim();
                query = query.Where(a => a.Initiative.Title.Contains(kw) || a.Initiative.InitiativeCode.Contains(kw));
            }

            query = vm.SortOrder switch
            {
                "Oldest" => query.OrderBy(a => a.DecisionDate ?? a.AssignedDate),
                _ => query.OrderByDescending(a => a.DecisionDate ?? a.AssignedDate)
            };

            vm.TotalCount = await query.CountAsync();
            vm.TotalPages = (int)Math.Ceiling(vm.TotalCount / (double)vm.PageSize);
            if (vm.TotalPages == 0) vm.TotalPages = 1;
            if (vm.CurrentPage > vm.TotalPages) vm.CurrentPage = vm.TotalPages;

            vm.Items = await query
                .Skip((vm.CurrentPage - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .Select(a => new AssignedListItem
                {
                    AssignmentId = a.Id,
                    InitiativeId = a.InitiativeId,
                    InitiativeCode = a.Initiative.InitiativeCode,
                    Title = a.Initiative.Title,
                    CategoryName = a.Initiative.Category.Name,
                    AssignedDate = a.AssignedDate,
                    DueDate = a.DueDate,
                    Status = a.Status
                })
                .ToListAsync();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserIdForTest();

            var assignment = await _db.InitiativeAssignments
                .AsNoTracking()
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Proposer)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Department)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Files)
                .Include(a => a.Template)
                    .ThenInclude(t => t.CriteriaList)
                .Include(a => a.EvaluationDetails)
                .FirstOrDefaultAsync(a => a.Id == id && a.MemberId == userId);

            if (assignment == null)
                return NotFound();

            var detailsLookup = assignment.EvaluationDetails
                .ToDictionary(d => d.CriteriaId, d => d);

            var vm = new GradingVM
            {
                AssignmentId = assignment.Id,
                InitiativeTitle = assignment.Initiative.Title,
                InitiativeCode = assignment.Initiative.InitiativeCode,
                ProposerName = assignment.Initiative.Proposer?.FullName ?? assignment.Initiative.Proposer?.UserName ?? string.Empty,
                DepartmentName = assignment.Initiative.Department?.Name ?? string.Empty,
                DueDate = assignment.DueDate,
                Files = assignment.Initiative.Files?.ToList() ?? new(),
                GeneralComment = assignment.ReviewComment,
                SubmitAction = "SaveDraft"
            };

            vm.CriteriaItems = assignment.Template.CriteriaList
                .OrderBy(c => c.SortOrder)
                .Select(c =>
                {
                    detailsLookup.TryGetValue(c.Id, out var existing);
                    return new GradingItem
                    {
                        CriteriaId = c.Id,
                        CriteriaName = c.CriteriaName,
                        Description = c.Description,
                        MaxScore = c.MaxScore,
                        ScoreGiven = existing?.ScoreGiven ?? 0,
                        Note = existing?.Note
                    };
                })
                .ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitGrading(GradingVM vm)
        {
            var userId = GetCurrentUserIdForTest();

            var assignment = await _db.InitiativeAssignments
                .Include(a => a.Template)
                    .ThenInclude(t => t.CriteriaList)
                .Include(a => a.EvaluationDetails)
                .FirstOrDefaultAsync(a => a.Id == vm.AssignmentId && a.MemberId == userId);

            if (assignment == null)
                return NotFound();

            var templateCriteria = assignment.Template.CriteriaList
                .ToDictionary(c => c.Id, c => c);

            if (vm.CriteriaItems == null)
                vm.CriteriaItems = new();

            foreach (var item in vm.CriteriaItems)
            {
                if (!templateCriteria.TryGetValue(item.CriteriaId, out var criteria))
                {
                    ModelState.AddModelError(string.Empty, "Tiêu chí không hợp lệ.");
                    continue;
                }

                if (item.ScoreGiven < 0 || item.ScoreGiven > criteria.MaxScore)
                {
                    ModelState.AddModelError($"CriteriaItems[{vm.CriteriaItems.IndexOf(item)}].ScoreGiven",
                        $"Điểm phải từ 0 đến {criteria.MaxScore}.");
                }
            }

            if (!ModelState.IsValid)
            {
                var hydrated = await _db.InitiativeAssignments
                    .AsNoTracking()
                    .Include(a => a.Initiative)
                        .ThenInclude(i => i.Proposer)
                    .Include(a => a.Initiative)
                        .ThenInclude(i => i.Department)
                    .Include(a => a.Initiative)
                        .ThenInclude(i => i.Files)
                    .FirstOrDefaultAsync(a => a.Id == vm.AssignmentId && a.MemberId == userId);

                if (hydrated != null)
                {
                    vm.InitiativeTitle = hydrated.Initiative.Title;
                    vm.InitiativeCode = hydrated.Initiative.InitiativeCode;
                    vm.ProposerName = hydrated.Initiative.Proposer?.FullName
                                      ?? hydrated.Initiative.Proposer?.UserName
                                      ?? string.Empty;
                    vm.DepartmentName = hydrated.Initiative.Department?.Name ?? string.Empty;
                    vm.DueDate = hydrated.DueDate;
                    vm.Files = hydrated.Initiative.Files?.ToList() ?? new();
                }

                return View("Details", vm);
            }

            var existing = assignment.EvaluationDetails.ToDictionary(d => d.CriteriaId, d => d);

            foreach (var item in vm.CriteriaItems)
            {
                if (!templateCriteria.TryGetValue(item.CriteriaId, out var criteria))
                    continue;

                if (existing.TryGetValue(item.CriteriaId, out var detail))
                {
                    detail.ScoreGiven = item.ScoreGiven;
                    detail.Note = item.Note;
                }
                else
                {
                    assignment.EvaluationDetails.Add(new EvaluationDetail
                    {
                        AssignmentId = assignment.Id,
                        CriteriaId = item.CriteriaId,
                        ScoreGiven = item.ScoreGiven,
                        Note = item.Note
                    });
                }
            }

            assignment.ReviewComment = vm.GeneralComment;

            if (string.Equals(vm.SubmitAction, "Submit", StringComparison.OrdinalIgnoreCase))
            {
                assignment.Status = AssignmentStatus.Completed;
                assignment.DecisionDate = DateTime.Now;
            }
            else
            {
                assignment.Status = AssignmentStatus.InProgress;
            }

            await _db.SaveChangesAsync();

            if (string.Equals(vm.SubmitAction, "Submit", StringComparison.OrdinalIgnoreCase))
            {
                // Sau khi nộp kết quả, quay về danh sách
                return RedirectToAction(nameof(AssignedInitiatives), new { status = "Assigned" });
            }

            // Return to the details page after saving draft
            return RedirectToAction(nameof(Details), new { id = vm.AssignmentId });
        }

        public IActionResult CouncilChair()
        {
            return View();
        }


    }
}