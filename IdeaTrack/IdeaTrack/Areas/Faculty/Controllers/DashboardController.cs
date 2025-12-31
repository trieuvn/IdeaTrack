using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // 1. TRANG DANH SÁCH (INDEX)
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
            // Fix lỗi chia cho 0 hoặc null
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
        // 2. TRANG CHI TIẾT (DETAILS)
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
        // 3. TRANG REVIEW (ĐÃ KHÔI PHỤC LOGIC ĐẦY ĐỦ)
        // ==========================================
        public async Task<IActionResult> Review(int pageNumber = 1, string filterType = "All")
        {
            int pageSize = 5;

            var query = _context.Initiatives
                .Include(i => i.Proposer)
                .Include(i => i.RevisionRequests)
                .AsQueryable();

            // Bộ lọc
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

            // Phân trang
            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var listData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thống kê (ViewBag quan trọng để tránh lỗi RuntimeBinder)
            var allData = _context.Initiatives.AsQueryable();
            ViewBag.CountEditing = await allData.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);
            ViewBag.CountResubmitted = await allData.CountAsync(i => i.Status == InitiativeStatus.Reviewing);

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.CountCompleted = await allData.CountAsync(i => i.Status == InitiativeStatus.Approved && i.FinalResult.DecisionDate >= startOfMonth);

            // Dropdown List
            ViewBag.DropdownList = await allData
                .Where(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Dept_Review)
                .Select(i => new { i.Id, i.InitiativeCode, i.Title, LecturerName = i.Proposer.FullName })
                .ToListAsync();

            // Truyền biến sang View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.FilterType = filterType;

            return View(listData);
        }

        // ==========================================
        // 4. XỬ LÝ: GỬI YÊU CẦU CHỈNH SỬA (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRevision(int initiativeId, string content, DateTime? deadline)
        {
            var initiative = await _context.Initiatives.FindAsync(initiativeId);
            if (initiative == null) return NotFound();

            // Cập nhật trạng thái
            initiative.Status = InitiativeStatus.Revision_Required;

            // Tạo yêu cầu chỉnh sửa
            var revisionRequest = new RevisionRequest
            {
                InitiativeId = initiativeId,
                RequestContent = content,
                RequestedDate = DateTime.Now,
                Deadline = deadline, // Đã thêm lại tham số deadline
                IsResolved = false,
                Status = "Open",
                RequesterId = 2 // TODO: Thay bằng User.Identity
            };

            _context.RevisionRequests.Add(revisionRequest);
            await _context.SaveChangesAsync();

            // QUAN TRỌNG: Load lại trang Review (thay vì Details) để đúng luồng làm việc
            return RedirectToAction(nameof(Review));
        }

        // ==========================================
        // 5. XỬ LÝ: DUYỆT HỒ SƠ (POST)
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
        // 6. XỬ LÝ: TỪ CHỐI (POST)
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

        public IActionResult Progress() => View();
        public IActionResult Profile() => View();
    }
}