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
        // 1. TRANG DANH SÁCH (INDEX) - CÓ PHÂN TRANG 5 DÒNG
        // ==========================================
        public async Task<IActionResult> Index(string searchString, string statusFilter, int pageNumber = 1)
        {
            int pageSize = 5; // Cố định 5 dòng mỗi trang

            // --- A. TẠO QUERY & LỌC DỮ LIỆU ---
            var query = _context.Initiatives
                .Include(i => i.Proposer) // Load thông tin giảng viên
                .Include(i => i.Category)
                .Include(i => i.Department) // Load thông tin khoa
                .AsQueryable();

            // 1. Logic Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Title.Contains(searchString) || s.InitiativeCode.Contains(searchString) || s.Proposer.FullName.Contains(searchString));
            }

            // 2. Logic Lọc trạng thái
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(typeof(InitiativeStatus), statusFilter, out var status))
            {
                query = query.Where(s => s.Status == (InitiativeStatus)status);
            }

            // 3. Sắp xếp: Ưu tiên hồ sơ "Chờ duyệt" lên đầu, sau đó đến ngày mới nhất
            query = query.OrderByDescending(i => i.Status == InitiativeStatus.Pending)
                         .ThenByDescending(i => i.CreatedAt);

            // --- B. XỬ LÝ PHÂN TRANG ---
            var totalItems = await query.CountAsync(); // Tổng số bản ghi SAU khi lọc
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Đảm bảo pageNumber nằm trong khoảng hợp lệ
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages == 0 ? 1 : totalPages));

            // Lấy dữ liệu cho trang hiện tại
            var initiatives = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- C. THỐNG KÊ (CARD VIEW - Màu xanh, vàng, tím...) ---
            // Lấy số liệu TOÀN CỤC (Không bị ảnh hưởng bởi search/filter)
            var allData = _context.Initiatives.AsQueryable();

            ViewBag.CountPending = await allData.CountAsync(i => i.Status == InitiativeStatus.Pending);
            ViewBag.CountRevision = await allData.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);
            ViewBag.CountApproved = await allData.CountAsync(i => i.Status == InitiativeStatus.Dept_Review || i.Status == InitiativeStatus.Approved);
            ViewBag.Total = await allData.CountAsync();

            // --- D. TRUYỀN DỮ LIỆU PHÂN TRANG RA VIEW ---
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Giữ lại giá trị filter để khi chuyển trang không bị mất
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;

            return View(initiatives);
        }

        public IActionResult Details(int? id) => View();

        // Tìm đến Action Review và thay thế bằng đoạn code này:
        public async Task<IActionResult> Review(int pageNumber = 1, string filterType = "All")
        {
            int pageSize = 5;

            // --- A. LẤY DỮ LIỆU CƠ BẢN ---
            var query = _context.Initiatives
                .Include(i => i.Proposer)
                .Include(i => i.RevisionRequests)
                .AsQueryable();

            // --- B. XỬ LÝ BỘ LỌC (LOGIC MỚI) ---
            switch (filterType)
            {
                case "Editing": // Chỉ lấy đang chỉnh sửa
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required);
                    break;
                case "Resubmitted": // Chỉ lấy đã nộp lại
                    query = query.Where(i => i.Status == InitiativeStatus.Reviewing);
                    break;
                default: // "All" - Lấy cả hai
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required || i.Status == InitiativeStatus.Reviewing);
                    break;
            }

            // Sắp xếp: Mới nhất lên đầu
            query = query.OrderByDescending(i => i.SubmittedDate);

            // --- C. PHÂN TRANG ---
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages == 0 ? 1 : totalPages));

            var listData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- D. THỐNG KÊ (Giữ nguyên - tính trên toàn bộ dữ liệu, không theo filter) ---
            var allData = _context.Initiatives.AsQueryable();
            ViewBag.CountEditing = await allData.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);
            ViewBag.CountResubmitted = await allData.CountAsync(i => i.Status == InitiativeStatus.Reviewing);
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.CountCompleted = await allData.CountAsync(i => i.Status == InitiativeStatus.Approved && i.FinalResult.DecisionDate >= startOfMonth);

            // --- E. DROPDOWN LIST (Giữ nguyên) ---
            ViewBag.DropdownList = await allData
                .Where(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Dept_Review)
                .Select(i => new { i.Id, i.InitiativeCode, i.Title, LecturerName = i.Proposer.FullName })
                .ToListAsync();

            // --- F. TRUYỀN DỮ LIỆU RA VIEW ---
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.FilterType = filterType; // Truyền loại lọc hiện tại để View biết nút nào đang active

            return View(listData);
        }
        // ... Các code cũ ...

        // ==========================================
        // 4. XỬ LÝ GỬI YÊU CẦU CHỈNH SỬA (POST)
        // ==========================================
        [HttpPost] // Hàm này chỉ nhận dữ liệu gửi lên từ Form
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRevision(int initiativeId, string content, DateTime? deadline)
        {
            // 1. Tìm hồ sơ trong Database
            var initiative = await _context.Initiatives.FindAsync(initiativeId);

            if (initiative == null)
            {
                return NotFound();
            }

            // 2. Cập nhật trạng thái hồ sơ -> Yêu cầu chỉnh sửa
            initiative.Status = InitiativeStatus.Revision_Required;

            // 3. Tạo bản ghi yêu cầu chỉnh sửa mới (Lưu lịch sử)
            var revisionRequest = new RevisionRequest
            {
                InitiativeId = initiativeId,
                RequestContent = content,
                RequestedDate = DateTime.Now,
                Deadline = deadline,
                IsResolved = false,
                Status = "Open",

                RequesterId = 2
            };

            _context.RevisionRequests.Add(revisionRequest);

            // 4. Lưu tất cả thay đổi vào Database
            await _context.SaveChangesAsync();

            // 5. Load lại trang Review để thấy kết quả mới
            return RedirectToAction(nameof(Review));
        }
        public IActionResult Progress() => View();
        public IActionResult Profile() => View();
    }
}