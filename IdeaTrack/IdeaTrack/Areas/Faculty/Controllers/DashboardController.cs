using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Areas.Faculty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace IdeaTrack.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    [Authorize(Roles = "FacultyLeader,Faculty_Admin,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DASHBOARD LIST (INDEX)
        // ==========================================
        // ==========================================
        // 1. DASHBOARD LIST (INDEX)
        // ==========================================
        public async Task<IActionResult> Index(string searchString, string statusFilter, int pageNumber = 1)
        {
            int pageSize = 5;

            // 1. Base Query - Filter explicitly for Faculty Workflow statuses
            var allowedStatuses = new[] 
            { 
                InitiativeStatus.Pending, 
                InitiativeStatus.Faculty_Approved, 
                InitiativeStatus.Rejected, 
                InitiativeStatus.Revision_Required 
            };

            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Category)
                .Include(i => i.Department)
                .Where(i => allowedStatuses.Contains(i.Status))
                .AsQueryable();

            // 2. Apply Search
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Title.Contains(searchString) || s.InitiativeCode.Contains(searchString) || s.Creator.FullName.Contains(searchString));
            }

            // 3. Apply Status Filter (if specific one selected)
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(typeof(InitiativeStatus), statusFilter, out var status))
            {
                query = query.Where(s => s.Status == (InitiativeStatus)status);
            }

            // 4. Sorting: Latest updates first (SubmittedDate or CreatedAt)
            query = query.OrderByDescending(i => i.SubmittedDate ?? i.CreatedAt);

            // 5. Pagination
            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var initiatives = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Dynamic Statistics (Count directly from DB based on allowed workflow)
            // Note: We query the base 'allowed' set for stats, ignoring the search filter to show overall dashboard health
            var baseStatsQuery = _context.Initiatives.Where(i => allowedStatuses.Contains(i.Status));
            
            var viewModel = new FacultyDashboardVM
            {
                PendingCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Pending),
                ApprovedCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Faculty_Approved),
                RejectedCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Rejected),
                TotalInitiatives = await baseStatsQuery.CountAsync(), // Total of the 4 relevant statuses
                Initiatives = initiatives.Select(i => new FacultyInitiativeItem
                {
                    Id = i.Id,
                    Title = i.Title,
                    InitiativeCode = i.InitiativeCode,
                    ProposerName = i.Creator?.FullName ?? "Unknown",
                    Category = i.Category,
                    Status = i.Status,
                    SubmittedDate = i.SubmittedDate ?? i.CreatedAt
                }).ToList()
            };

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;

            return View(viewModel);
        }

        // ==========================================
        // 2. DETAILS PAGE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var initiative = await _context.Initiatives
                .Include(i => i.Creator).ThenInclude(u => u.Department)
                .Include(i => i.Category)
                .Include(i => i.Files)
                .Include(i => i.RevisionRequests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (initiative == null) return NotFound();

            var viewModel = new FacultyInitiativeDetailVM
            {
                Id = initiative.Id,
                Title = initiative.Title,
                InitiativeCode = initiative.InitiativeCode,
                Description = initiative.Description ?? "",
                ProposerName = initiative.Creator?.FullName ?? "Unknown",
                Budget = initiative.Budget,
                SubmittedDate = initiative.SubmittedDate ?? DateTime.MinValue,
                Status = initiative.Status.ToString(),
                Category = initiative.Category,
                Files = initiative.Files?.Select(f => new InitiativeFileVM 
                { 
                    FileName = f.FileName, 
                    FileUrl = f.FilePath // Assuming FilePath or similar exists, might need adjustment
                }).ToList()
            };

            return View(viewModel);
        }

        // ==========================================
        // 3. REVIEW PAGE (Tracking Revisions)
        // ==========================================
        public async Task<IActionResult> Review(int pageNumber = 1, string filterType = "All")
        {
            int pageSize = 5;

            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.RevisionRequests)
                .AsQueryable();

            switch (filterType)
            {
                case "Editing":
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required);
                    break;
                case "Resubmitted":
                    query = query.Where(i => i.Status == InitiativeStatus.Pending);
                    break;
                default: // "All"
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required || i.Status == InitiativeStatus.Pending);
                    break;
            }

            query = query.OrderByDescending(i => i.SubmittedDate);

            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var listData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allData = _context.Initiatives.AsQueryable();
            ViewBag.CountEditing = await allData.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);
            ViewBag.CountResubmitted = await allData.CountAsync(i => i.Status == InitiativeStatus.Pending);

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.CountCompleted = await allData.CountAsync(i => i.Status == InitiativeStatus.Approved && i.FinalResult != null && i.FinalResult.DecisionDate >= startOfMonth);

            ViewBag.DropdownList = await allData
                .Where(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Faculty_Approved)
                .Select(i => new { i.Id, i.InitiativeCode, i.Title, LecturerName = i.Creator.FullName })
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.FilterType = filterType;

            return View(listData);
        }

        // ==========================================


        // ==========================================
        // 4. SUBMIT REVISION REQUEST (POST)
        // ==========================================
        // ==========================================
        // 4. REQUEST REVISION (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRevision(int InitiativeId, string Comments)
        {
            var initiative = await _context.Initiatives.FindAsync(InitiativeId);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Revision_Required;
            
            if (!string.IsNullOrEmpty(Comments))
            {
                var revisionRequest = new RevisionRequest
                {
                    InitiativeId = InitiativeId,
                    RequestContent = Comments,
                    RequestedDate = DateTime.Now,
                    IsResolved = false,
                    Status = "Open",
                    RequesterId = 2 // TODO: Get real user ID
                };
                _context.RevisionRequests.Add(revisionRequest);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Revision request sent to the author!";
            return RedirectToAction(nameof(Details), new { id = InitiativeId });
        }

        // ==========================================
        // 5. APPROVE INITIATIVE (POST) - Faculty approves to OST
        // ==========================================
        // ==========================================
        // 5. ACCEPT INITIATIVE (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int InitiativeId, string Comments)
        {
            var initiative = await _context.Initiatives.FindAsync(InitiativeId);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Faculty_Approved;
            // You can log comments if needed

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Initiative accepted and forwarded to R&D Department!";
            return RedirectToAction(nameof(Details), new { id = InitiativeId });
        }

        // ==========================================
        // 6. REJECT INITIATIVE (POST)
        // ==========================================
        // ==========================================
        // 6. REJECT INITIATIVE (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int InitiativeId, string Comments)
        {
            var initiative = await _context.Initiatives.FindAsync(InitiativeId);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Rejected;
            // You can log comments if needed

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Initiative has been rejected.";
            return RedirectToAction(nameof(Details), new { id = InitiativeId });
        }

        // ==========================================
        // 7. STATISTICS PAGE (PROGRESS)
        // ==========================================
        public async Task<IActionResult> Progress(string searchString)
        {
            var query = _context.Initiatives
                .Include(i => i.Creator)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i => i.Creator.FullName.Contains(searchString)
                                      || i.Title.Contains(searchString)
                                      || i.InitiativeCode.Contains(searchString));
            }

            var recentInitiatives = await query
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.CurrentSearch = searchString;

            var allData = _context.Initiatives.AsQueryable();

            int countPending = await allData.CountAsync(i => i.Status == InitiativeStatus.Pending);
            int countApproved = await allData.CountAsync(i => i.Status == InitiativeStatus.Faculty_Approved || i.Status == InitiativeStatus.Approved);
            int countRevision = await allData.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);

            ViewBag.PieData = new List<int> { countPending, countApproved, countRevision };

            var today = DateTime.Today;
            var trendData = new List<int>();
            var trendLabels = new List<string>();

            for (int i = 4; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                int count = await allData.CountAsync(x => x.CreatedAt.Date == date);
                trendData.Add(count);
                trendLabels.Add(date.ToString("dd/MM"));
            }

            ViewBag.TrendData = trendData;
            ViewBag.TrendLabels = trendLabels;

            return View(recentInitiatives);
        }

        // ==========================================
        // 8. EXPORT TO EXCEL
        // ==========================================
        public async Task<IActionResult> ExportToExcel()
        {
            var data = await _context.Initiatives
                .Include(i => i.Creator)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Code,Title,Lecturer,Submitted Date,Status");

            foreach (var item in data)
            {
                string title = item.Title?.Replace(",", ";") ?? "";
                string creator = item.Creator?.FullName?.Replace(",", ";") ?? "";
                string status = item.Status switch
                {
                    InitiativeStatus.Pending => "Pending Review",
                    InitiativeStatus.Faculty_Approved => "Forwarded to R&D Dept",
                    InitiativeStatus.Revision_Required => "Revision Required",
                    InitiativeStatus.Evaluating => "Under Evaluation",
                    InitiativeStatus.Approved => "Approved",
                    InitiativeStatus.Rejected => "Rejected",
                    _ => item.Status.ToString()
                };

                builder.AppendLine($"{item.InitiativeCode},{title},{creator},{item.CreatedAt:dd/MM/yyyy},{status}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var result = new byte[bom.Length + buffer.Length];
            Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            Buffer.BlockCopy(buffer, 0, result, bom.Length, buffer.Length);

            return File(result, "text/csv", $"Initiative_Stats_{DateTime.Now:ddMMyyyy}.csv");
        }

        public IActionResult Profile() => View();
    }
}