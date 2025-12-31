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

        // ... (Giữ nguyên các hàm Details, Review, Progress... của bạn)
        public IActionResult Details(int? id) => View();
        public IActionResult Review() => View();
        public IActionResult Progress() => View();
        public IActionResult Profile() => View();
    }
}