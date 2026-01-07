using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class PortController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInitiativeService _initiativeService;

        public PortController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IInitiativeService initiativeService)
        {
            _context = context;
            _userManager = userManager;
            _initiativeService = initiativeService;
        }

        // GET: /SciTech/Port
        public IActionResult Index(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? keyword,
            int page = 1)
        {
            const int PAGE_SIZE = 5;

            var allowedStatuses = new[] { 
                InitiativeStatus.Faculty_Approved, 
                InitiativeStatus.Evaluating, 
                InitiativeStatus.Re_Evaluating, 
                InitiativeStatus.Pending_Final, 
                InitiativeStatus.Approved, 
                InitiativeStatus.Rejected 
            };

            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .Include(i => i.Period)
                .ThenInclude(p => p.AcademicYear)
                .Where(i => allowedStatuses.Contains(i.Status))
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(i =>
                    (i.SubmittedDate ?? i.CreatedAt) >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i =>
                    (i.SubmittedDate ?? i.CreatedAt) <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(status)
                && Enum.TryParse<InitiativeStatus>(status, out var parsedStatus))
            {
                query = query.Where(i => i.Status == parsedStatus);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(i =>
                    i.Title.Contains(keyword) ||
                    i.InitiativeCode.Contains(keyword));
            }

            var totalItems = query.Count();

            var items = query
                .OrderByDescending(i => i.SubmittedDate ?? i.CreatedAt)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .Select(i => new InitiativeListVM
                {
                    Id = i.Id,
                    InitiativeCode = i.InitiativeCode,
                    Title = i.Title,
                    ProposerName = i.Creator.FullName,
                    DepartmentName = i.Department.Name,
                    SubmittedDate = i.SubmittedDate ?? i.CreatedAt,
                    Status = i.Status.ToString(),
                    PeriodName = i.Period != null ? i.Period.Name : "N/A",
                    AcademicYear = i.Period != null && i.Period.AcademicYear != null ? i.Period.AcademicYear.Name : "N/A"
                })
                .ToList();

            var vm = new InitiativeReportVM
            {
                Items = items,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                Keyword = keyword,
                CurrentPage = page,
                TotalItems = totalItems
            };

            return View(vm);
        }

        // POST: /SciTech/Port/ExportExcel
        [HttpPost]
        public IActionResult ExportExcel(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? keyword)
        {
            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) <= toDate.Value);

            if (!string.IsNullOrEmpty(status)
                && Enum.TryParse<InitiativeStatus>(status, out var parsedStatus))
                query = query.Where(i => i.Status == parsedStatus);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(i =>
                    i.Title.Contains(keyword) ||
                    i.InitiativeCode.Contains(keyword));

            var data = query.Select(i => new
            {
                i.InitiativeCode,
                i.Title,
                Creator = i.Creator.FullName,
                Department = i.Department.Name,
                SubmittedDate = i.SubmittedDate ?? i.CreatedAt,
                Status = i.Status.ToString()
            }).ToList();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Initiatives");
            ws.Cells.LoadFromCollection(data, true);
            ws.Cells.AutoFitColumns();

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BaoCaoHoSo.xlsx");
        }

        // POST: /SciTech/Port/ExportPdf
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportPdf(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? keyword)
        {
            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) <= toDate.Value);

            if (!string.IsNullOrEmpty(status)
                && Enum.TryParse<InitiativeStatus>(status, out var parsedStatus))
                query = query.Where(i => i.Status == parsedStatus);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(i =>
                    i.Title.Contains(keyword) ||
                    i.InitiativeCode.Contains(keyword));

            var data = query.Select(i => new InitiativePdfVM
            {
                InitiativeCode = i.InitiativeCode,
                Title = i.Title,
                Proposer = i.Creator.FullName,
                Department = i.Department.Name,
                Status = i.Status.ToString()
            }).ToList();

            var document = new InitiativeReportPdf(data);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "BaoCaoHoSo.pdf");
        }

        // GET: /SciTech/Port/Result/5
        public async Task<IActionResult> Result(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .Include(i => i.Category)
                .Include(i => i.FinalResult)
                .Include(i => i.Assignments)
                    .ThenInclude(a => a.Member)
                .Include(i => i.Assignments)
                    .ThenInclude(a => a.EvaluationDetails)
                        .ThenInclude(ed => ed.Criteria)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null) return NotFound();

            // Calculate Stats
            var assignments = initiative.Assignments.Where(a => a.RoundNumber == initiative.CurrentRound).ToList();
            var completed = assignments.Count(a => a.Status == AssignmentStatus.Completed);
            var totalMembers = assignments.Count;
            var consensusRate = totalMembers > 0 ? (double)completed / totalMembers * 100 : 0;

            decimal averageScore = 0;
            if (completed > 0)
            {
                averageScore = assignments
                    .Where(a => a.Status == AssignmentStatus.Completed)
                    .SelectMany(a => a.EvaluationDetails)
                    .Sum(d => d.ScoreGiven) / completed; 
            }
            
            var memberScores = assignments.Select(a => new MemberScoreVM
            {
                MemberName = a.Member.FullName,
                Role = "Member", 
                Scores = a.EvaluationDetails.ToDictionary(d => d.Criteria.CriteriaName, d => d.ScoreGiven),
                TotalScore = a.EvaluationDetails.Sum(d => d.ScoreGiven),
                IsCompleted = a.Status == AssignmentStatus.Completed
            }).ToList();

            if (completed > 0)
            {
                 averageScore = memberScores.Where(ms => ms.IsCompleted).Average(ms => ms.TotalScore);
            }

            var vm = new InitiativeResultVM
            {
                Id = initiative.Id,
                Title = initiative.Title,
                InitiativeCode = initiative.InitiativeCode,
                ProposerName = initiative.Creator.FullName,
                AverageScore = averageScore,
                ConsensusRate = consensusRate,
                CompletedCount = completed,
                TotalMembers = totalMembers,
                FinalStatus = initiative.FinalResult?.ChairmanDecision ?? "Pending",
                Rank = initiative.FinalResult?.Rank ?? "N/A",
                MemberScores = memberScores,
                Status = initiative.Status.ToString()
            };

            return View(vm);
        }

        // GET: /SciTech/Port/Approve/5
        [HttpGet]
        public IActionResult Approve(int id)
        {
            var initiative = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .Include(i => i.Category)
                .Include(i => i.Files)
                .FirstOrDefault(i => i.Id == id);

            if (initiative == null) return NotFound();

            // Auto-redirect to Result view for evaluating/finished initiatives
            var resultStatuses = new[] { 
                InitiativeStatus.Evaluating, 
                InitiativeStatus.Re_Evaluating, 
                InitiativeStatus.Pending_Final, 
                InitiativeStatus.Approved,
                InitiativeStatus.Rejected
            };

            if (resultStatuses.Contains(initiative.Status))
            {
                return RedirectToAction("Result", new { id = initiative.Id });
            }

            var vm = new InitiativeDetailVM
            {
                Id = initiative.Id,
                InitiativeCode = initiative.InitiativeCode,
                Title = initiative.Title,
                ProposerName = initiative.Creator.FullName,
                DepartmentName = initiative.Department.Name,
                SubmittedDate = initiative.SubmittedDate ?? initiative.CreatedAt,
                Status = initiative.Status,
                Budget = initiative.Budget,
                Code = initiative.Department.Code,
                Description = initiative.Description,
                Category = initiative.Category.Name,
                Files = initiative.Files.Select(f => new InitiativeFileVM
                {
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType
                }).ToList()
            };

            return View(vm);
        }

        // POST: /SciTech/Port/ApproveInitiative
        // When OST approves, use IInitiativeService for auto-assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInitiative(int id, string? requestContent)
        {
            var initiative = await _context.Initiatives.FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
                return NotFound();

            // Log the approval request
            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1, // TODO: Replace with current user ID
                RequestContent = requestContent ?? "Council approval and grading assignment initiated.",
                RequestedDate = DateTime.Now,
                Status = "Approved",
                IsResolved = true
            };

            _context.RevisionRequests.Add(revision);
            await _context.SaveChangesAsync();

            // Use service for auto-assignment (delegates to IInitiativeService)
            var assigned = await _initiativeService.AutoAssignToBoardAsync(id);

            if (!assigned)
            {
                // If no board/template configured, approve directly
                initiative.Status = InitiativeStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Approved, but no Council or Grading Criteria configured for this category.";
            }

            TempData["SuccessMessage"] = "Successfully approved and assigned to the Council for grading!";
            return RedirectToAction("Result", new { id });
        }

        // POST: /SciTech/Port/RejectInitiative
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInitiative(int id, DateTime? deadline, string requestContent)
        {
            var initiative = await _context.Initiatives.FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
                return NotFound();

            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1, // TODO: Replace with current user ID
                RequestContent = requestContent,
                Deadline = deadline,
                RequestedDate = DateTime.Now,
                Status = "Open"
            };

            initiative.Status = InitiativeStatus.Rejected;

            _context.RevisionRequests.Add(revision);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Initiative has been rejected.";
            return RedirectToAction("Result", new { id });
        }

        // POST: /SciTech/Port/RequestReEvaluation
        // When PKHCN requests a new evaluation round
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReEvaluation(int id, string? reason)
        {
            var success = await _initiativeService.CreateNewRoundAsync(id);

            if (!success)
            {
                TempData["ErrorMessage"] = "Could not create a new evaluation round. Please try again.";
                return RedirectToAction("Approve", new { id });
            }

            // Log the re-evaluation request
            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1,
                RequestContent = reason ?? "Requesting re-evaluation for next round",
                RequestedDate = DateTime.Now,
                Status = "Re-evaluation",
                IsResolved = false
            };

            _context.RevisionRequests.Add(revision);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Re-evaluation round requested successfully!";
            return RedirectToAction("Index");
        }

        // POST: /SciTech/Port/MakeFinalDecision
        // When PKHCN makes final decision (Approve/Reject)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeFinalDecision(int id, string decision)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var decidedByUserId = currentUser?.Id ?? 1;

            var finalStatus = decision == "Approve" 
                ? InitiativeStatus.Approved 
                : InitiativeStatus.Rejected;

            var success = await _initiativeService.CreateFinalResultAsync(id, finalStatus, decidedByUserId);

            if (!success)
            {
                TempData["ErrorMessage"] = "Could not make the final decision. Please try again.";
                return RedirectToAction("Approve", new { id });
            }

            TempData["SuccessMessage"] = $"Successfully {(decision == "Approve" ? "Approved" : "Rejected")} this initiative!";
            return RedirectToAction("Index");
        }

        // GET: /SciTech/Port/Follow
        public IActionResult Follow() => View();

        // GET: /SciTech/Port/Profile
        public IActionResult Profile() => View();
    }
}
