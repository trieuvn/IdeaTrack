using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace IdeaTrack.Areas.Faculty.Controllers
{
    [Area("Faculty")]
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
        public async Task<IActionResult> Index(string searchString, string statusFilter, int pageNumber = 1)
        {
            int pageSize = 5;

            var query = _context.Initiatives
                .Include(i => i.Proposer)
                .Include(i => i.Category)
                .Include(i => i.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Title.Contains(searchString) || s.InitiativeCode.Contains(searchString) || s.Proposer.FullName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(typeof(InitiativeStatus), statusFilter, out var status))
            {
                query = query.Where(s => s.Status == (InitiativeStatus)status);
            }

            query = query.OrderByDescending(i => i.Status == InitiativeStatus.Pending)
                         .ThenByDescending(i => i.CreatedAt);

            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var initiatives = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allData = _context.Initiatives.AsQueryable();
            ViewBag.CountPending = await allData.CountAsync(i => i.Status == InitiativeStatus.Pending);
            ViewBag.CountRevision = await allData.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);
            ViewBag.CountApproved = await allData.CountAsync(i => i.Status == InitiativeStatus.Dept_Review || i.Status == InitiativeStatus.Approved);
            ViewBag.Total = await allData.CountAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;

            return View(initiatives);
        }

        // ==========================================
        // 2. DETAILS PAGE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var initiative = await _context.Initiatives
                .Include(i => i.Proposer).ThenInclude(u => u.Department)
                .Include(i => i.Category)
                .Include(i => i.Files)
                .Include(i => i.RevisionRequests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (initiative == null) return NotFound();
            return View(initiative);
        }

        // ==========================================
        // 3. REVIEW PAGE (Tracking Revisions)
        // ==========================================
        public async Task<IActionResult> Review(int pageNumber = 1, string filterType = "All")
        {
            int pageSize = 5;

            var query = _context.Initiatives
                .Include(i => i.Proposer)
                .Include(i => i.RevisionRequests)
                .AsQueryable();

            switch (filterType)
            {
                case "Editing":
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required);
                    break;
                case "Resubmitted":
                    query = query.Where(i => i.Status == InitiativeStatus.Reviewing);
                    break;
                default: // "All"
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required || i.Status == InitiativeStatus.Reviewing);
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
            ViewBag.CountResubmitted = await allData.CountAsync(i => i.Status == InitiativeStatus.Reviewing);

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.CountCompleted = await allData.CountAsync(i => i.Status == InitiativeStatus.Approved && i.FinalResult.DecisionDate >= startOfMonth);

            ViewBag.DropdownList = await allData
                .Where(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Dept_Review)
                .Select(i => new { i.Id, i.InitiativeCode, i.Title, LecturerName = i.Proposer.FullName })
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.FilterType = filterType;

            return View(listData);
        }

        // ==========================================
        // 4. SUBMIT REVISION REQUEST (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRevision(int initiativeId, string content, DateTime? deadline)
        {
            var initiative = await _context.Initiatives.FindAsync(initiativeId);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Revision_Required;

            var revisionRequest = new RevisionRequest
            {
                InitiativeId = initiativeId,
                RequestContent = content,
                RequestedDate = DateTime.Now,
                Deadline = deadline,
                IsResolved = false,
                Status = "Open",
                RequesterId = 2 // TODO: Replace with real User.Identity
            };

            _context.RevisionRequests.Add(revisionRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Review));
        }

        // ==========================================
        // 5. APPROVE INITIATIVE (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Dept_Review;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // ==========================================
        // 6. REJECT INITIATIVE (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            initiative.Status = InitiativeStatus.Rejected;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // ==========================================
        // 7. STATISTICS PAGE (PROGRESS)
        // ==========================================
        public async Task<IActionResult> Progress(string searchString)
        {
            var query = _context.Initiatives
                .Include(i => i.Proposer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i => i.Proposer.FullName.Contains(searchString)
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
            int countApproved = await allData.CountAsync(i => i.Status == InitiativeStatus.Dept_Review || i.Status == InitiativeStatus.Approved);
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
                .Include(i => i.Proposer)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var builder = new StringBuilder();
            // Translate CSV Headers to English
            builder.AppendLine("Code,Title,Lecturer,Submitted Date,Status");

            foreach (var item in data)
            {
                string title = item.Title?.Replace(",", ";") ?? "";
                string proposer = item.Proposer?.FullName?.Replace(",", ";") ?? "";
                string status = item.Status switch
                {
                    InitiativeStatus.Pending => "Pending Review",
                    InitiativeStatus.Dept_Review => "Forwarded to R&D Dept",
                    InitiativeStatus.Revision_Required => "Revision Required",
                    _ => item.Status.ToString()
                };

                builder.AppendLine($"{item.InitiativeCode},{title},{proposer},{item.CreatedAt:dd/MM/yyyy},{status}");
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