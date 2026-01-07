using IdeaTrack.Areas.Councils.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdeaTrack.Areas.Councils.Controllers
{

    [Area("Councils")]
    [Authorize(Roles = "CouncilMember,Council_Member,Admin")]
    public class PageController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PageController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<int> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });
            
            var userId = user.Id;
            var now = DateTime.Now;
            
            // Only show initiatives that are in Evaluating or Re_Evaluating status
            var allowedStatuses = new[] { InitiativeStatus.Evaluating, InitiativeStatus.Re_Evaluating };

            var baseQuery = _db.InitiativeAssignments
                .AsNoTracking()
                .Where(a => a.MemberId == userId && allowedStatuses.Contains(a.Initiative.Status));

            var totalAssigned = await baseQuery.CountAsync();
            var completed = await baseQuery.CountAsync(a => a.Status == AssignmentStatus.Completed);
            var pending = totalAssigned - completed;

            var vm = new DashboardVM
            {
                UserFullName = user.FullName ?? user.UserName ?? "Expert",
                TotalAssigned = totalAssigned,
                CompletedCount = completed,
                PendingCount = pending,
                ProgressPercentage = totalAssigned == 0 ? 0 : (int)Math.Round((decimal)completed / totalAssigned * 100, 0, MidpointRounding.AwayFromZero)
            };


            vm.Assignments = await baseQuery
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
                    Score = a.EvaluationDetails.Select(d => (decimal?)d.ScoreGiven).Sum(),
                    Description = $"Evaluated {a.Initiative.InitiativeCode}: {a.Initiative.Title}"
                })
                .Take(3)
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> AssignedInitiatives(string? keyword, string status = "Assigned", string sortOrder = "Deadline", int page = 1)
        {
            var userId = await GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var vm = new AssignedListVM
            {
                Keyword = keyword,
                Status = string.IsNullOrWhiteSpace(status) ? "Assigned" : status,
                SortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "Deadline" : sortOrder,
                CurrentPage = page <= 0 ? 1 : page
            };

            // Only show initiatives that are in Evaluating or Re_Evaluating status
            var allowedStatuses = new[] { InitiativeStatus.Evaluating, InitiativeStatus.Re_Evaluating };

            var query = _db.InitiativeAssignments
                .AsNoTracking()
                .Where(a => a.MemberId == userId && allowedStatuses.Contains(a.Initiative.Status))
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Category)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Period)
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
            var userId = await GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var vm = new AssignedListVM
            {
                Keyword = keyword,
                Status = "Completed", // History chi hien thi da hoan thanh
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
            var userId = await GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var assignment = await _db.InitiativeAssignments
                .AsNoTracking()
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Creator)
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
            
            // Check if there are previous rounds for this initiative
            var hasPreviousRounds = await _db.InitiativeAssignments
                .AnyAsync(a => a.InitiativeId == assignment.InitiativeId && 
                              a.RoundNumber < assignment.RoundNumber);

            var vm = new GradingVM
            {
                AssignmentId = assignment.Id,
                InitiativeTitle = assignment.Initiative.Title,
                InitiativeCode = assignment.Initiative.InitiativeCode,
                ProposerName = assignment.Initiative.Creator?.FullName ?? assignment.Initiative.Creator?.UserName ?? string.Empty,
                DepartmentName = assignment.Initiative.Department?.Name ?? string.Empty,
                DueDate = assignment.DueDate,
                Files = assignment.Initiative.Files?.ToList() ?? new(),
                GeneralComment = assignment.ReviewComment,
                Strengths = assignment.Strengths,
                Limitations = assignment.Limitations,
                Recommendations = assignment.Recommendations,
                SubmitAction = "SaveDraft",
                RoundNumber = assignment.RoundNumber,
                IsLocked = assignment.Status == AssignmentStatus.Completed,
                HasPreviousRounds = hasPreviousRounds
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
            var userId = await GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var assignment = await _db.InitiativeAssignments
                .Include(a => a.Template)
                    .ThenInclude(t => t.CriteriaList)
                .Include(a => a.EvaluationDetails)
                .FirstOrDefaultAsync(a => a.Id == vm.AssignmentId && a.MemberId == userId);

            if (assignment == null)
                return NotFound();
            
            // Check if assignment is locked (already submitted)
            if (assignment.Status == AssignmentStatus.Completed)
            {
                TempData["ErrorMessage"] = "You cannot edit this evaluation as it has already been submitted and locked.";
                return RedirectToAction(nameof(Details), new { id = vm.AssignmentId });
            }

            var templateCriteria = assignment.Template.CriteriaList
                .ToDictionary(c => c.Id, c => c);

            if (vm.CriteriaItems == null)
                vm.CriteriaItems = new();

            foreach (var item in vm.CriteriaItems)
            {
                if (!templateCriteria.TryGetValue(item.CriteriaId, out var criteria))
                {
                    ModelState.AddModelError(string.Empty, "Tieu chi khong hop le.");
                    continue;
                }

                if (item.ScoreGiven < 0 || item.ScoreGiven > criteria.MaxScore)
                {
                    ModelState.AddModelError($"CriteriaItems[{vm.CriteriaItems.IndexOf(item)}].ScoreGiven",
                        $"Diem phai tu 0 den {criteria.MaxScore}.");
                }
            }

            if (!ModelState.IsValid)
            {
                var hydrated = await _db.InitiativeAssignments
                    .AsNoTracking()
                    .Include(a => a.Initiative)
                        .ThenInclude(i => i.Creator)
                    .Include(a => a.Initiative)
                        .ThenInclude(i => i.Department)
                    .Include(a => a.Initiative)
                        .ThenInclude(i => i.Files)
                    .FirstOrDefaultAsync(a => a.Id == vm.AssignmentId && a.MemberId == userId);

                if (hydrated != null)
                {
                    vm.InitiativeTitle = hydrated.Initiative.Title;
                    vm.InitiativeCode = hydrated.Initiative.InitiativeCode;
                    vm.ProposerName = hydrated.Initiative.Creator?.FullName
                                      ?? hydrated.Initiative.Creator?.UserName
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
            assignment.Strengths = vm.Strengths;
            assignment.Limitations = vm.Limitations;
            assignment.Recommendations = vm.Recommendations;

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
            
            // If submitting, check if all members have completed and calculate average
            if (string.Equals(vm.SubmitAction, "Submit", StringComparison.OrdinalIgnoreCase))
            {
                await CheckAndCalculateAverageScore(assignment.InitiativeId, assignment.RoundNumber);
            }
            
            TempData["SuccessMessage"] = string.Equals(vm.SubmitAction, "Submit", StringComparison.OrdinalIgnoreCase)
                ? "Evaluation submitted successfully! Your scores have been locked."
                : "Draft saved successfully!";

            if (string.Equals(vm.SubmitAction, "Submit", StringComparison.OrdinalIgnoreCase))
            {
                // After submitting, return to assigned list
                return RedirectToAction(nameof(AssignedInitiatives), new { status = "Assigned" });
            }

            // Return to the details page after saving draft
            return RedirectToAction(nameof(Details), new { id = vm.AssignmentId });
        }
        
        /// <summary>
        /// Check if all council members have completed their evaluations and calculate average score
        /// </summary>
        private async Task CheckAndCalculateAverageScore(int initiativeId, int roundNumber)
        {
            // Get all assignments for this initiative in the current round
            var assignments = await _db.InitiativeAssignments
                .Include(a => a.EvaluationDetails)
                .Where(a => a.InitiativeId == initiativeId && a.RoundNumber == roundNumber)
                .ToListAsync();
            
            if (!assignments.Any())
                return;
            
            // Check if all assignments are completed
            var allCompleted = assignments.All(a => a.Status == AssignmentStatus.Completed);
            
            if (!allCompleted)
                return;
            
            // Calculate average score from all members
            var memberScores = assignments
                .Select(a => a.EvaluationDetails.Sum(d => d.ScoreGiven))
                .ToList();
            
            if (!memberScores.Any())
                return;
            
            var averageScore = memberScores.Average();
            
            // Get the initiative to update status
            var initiative = await _db.Initiatives
                .Include(i => i.FinalResult)
                .FirstOrDefaultAsync(i => i.Id == initiativeId);
            
            if (initiative == null)
                return;
            
            // Create or update FinalResult
            if (initiative.FinalResult == null)
            {
                initiative.FinalResult = new FinalResult
                {
                    InitiativeId = initiativeId,
                    AverageScore = averageScore,
                    DecisionDate = DateTime.Now,
                    ChairmanId = 1 // Will be set when Chairman makes final decision
                };
                _db.Set<FinalResult>().Add(initiative.FinalResult);
            }
            else
            {
                initiative.FinalResult.AverageScore = averageScore;
            }
            
            // Update initiative status to Pending_Final (all evaluations complete)
            initiative.Status = InitiativeStatus.Pending_Final;
            
            await _db.SaveChangesAsync();
        }

        public IActionResult CouncilChair()
        {
            return View();
        }
        
        // ============ PHASE 5: ROUND HISTORY ============
        
        /// <summary>
        /// View all rounds for a specific initiative (for council members)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RoundHistory(int initiativeId)
        {
            var userId = await GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account", new { area = "" });
            
            var initiative = await _db.Initiatives
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == initiativeId);
                
            if (initiative == null)
                return NotFound();
            
            // Get all assignments for this initiative across all rounds
            var assignments = await _db.InitiativeAssignments
                .AsNoTracking()
                .Include(a => a.Member)
                .Include(a => a.EvaluationDetails)
                    .ThenInclude(d => d.Criteria)
                .Where(a => a.InitiativeId == initiativeId)
                .OrderBy(a => a.RoundNumber)
                .ThenBy(a => a.MemberId)
                .ToListAsync();
            
            var vm = new RoundHistoryVM
            {
                InitiativeId = initiative.Id,
                InitiativeTitle = initiative.Title,
                InitiativeCode = initiative.InitiativeCode,
                CurrentRound = initiative.CurrentRound,
                Rounds = assignments.Select(a => new RoundHistoryItem
                {
                    RoundNumber = a.RoundNumber,
                    AssignmentId = a.Id,
                    MemberName = a.Member?.FullName ?? "N/A",
                    AssignedDate = a.AssignedDate,
                    CompletedDate = a.DecisionDate,
                    TotalScore = a.EvaluationDetails.Sum(d => d.ScoreGiven),
                    Status = a.Status,
                    Comment = a.ReviewComment,
                    Details = a.EvaluationDetails.Select(d => new GradingItem
                    {
                        CriteriaId = d.CriteriaId,
                        CriteriaName = d.Criteria?.CriteriaName ?? "",
                        Description = d.Criteria?.Description ?? "",
                        MaxScore = d.Criteria?.MaxScore ?? 0,
                        ScoreGiven = d.ScoreGiven,
                        Note = d.Note
                    }).ToList()
                }).ToList()
            };
            
            return View(vm);
        }
        
        /// <summary>
        /// View a specific past assignment in read-only mode
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewPastAssignment(int id)
        {
            var assignment = await _db.InitiativeAssignments
                .AsNoTracking()
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Creator)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Department)
                .Include(a => a.Initiative)
                    .ThenInclude(i => i.Files)
                .Include(a => a.Template)
                    .ThenInclude(t => t.CriteriaList)
                .Include(a => a.EvaluationDetails)
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
                return NotFound();

            var detailsLookup = assignment.EvaluationDetails
                .ToDictionary(d => d.CriteriaId, d => d);

            var vm = new GradingVM
            {
                AssignmentId = assignment.Id,
                InitiativeTitle = assignment.Initiative.Title,
                InitiativeCode = assignment.Initiative.InitiativeCode,
                ProposerName = assignment.Initiative.Creator?.FullName ?? assignment.Initiative.Creator?.UserName ?? string.Empty,
                DepartmentName = assignment.Initiative.Department?.Name ?? string.Empty,
                DueDate = assignment.DueDate,
                Files = assignment.Initiative.Files?.ToList() ?? new(),
                GeneralComment = assignment.ReviewComment,
                Strengths = assignment.Strengths,
                Limitations = assignment.Limitations,
                Recommendations = assignment.Recommendations,
                RoundNumber = assignment.RoundNumber,
                IsLocked = true, // Always locked in view mode
                HasPreviousRounds = false
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
            
            ViewData["ViewOnlyMode"] = true;
            ViewData["MemberName"] = assignment.Member?.FullName ?? "N/A";
            
            return View("Details", vm);
        }
    }
}