using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /SciTech/User
        public IActionResult Index(
            string keyword,
            string role,
            string status,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u =>
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword));
            }

            // Filter by role
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Position == role);
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "active":
                        query = query.Where(u => u.IsActive);
                        break;
                    case "locked":
                    case "pending":
                        query = query.Where(u => !u.IsActive);
                        break;
                }
            }

            // Pagination
            int totalUsers = query.Count();

            var users = query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    Role = u.Position,
                    LastLogin = _context.SystemAuditLogs
                        .Where(l => l.UserId == u.Id && l.Action == "Login")
                        .OrderByDescending(l => l.Timestamp)
                        .Select(l => l.Timestamp)
                        .FirstOrDefault(),
                    LoginCount = _context.SystemAuditLogs
                        .Count(l => l.UserId == u.Id && l.Action == "Login")
                })
                .ToList();

            // Dashboard stats
            ViewBag.TotalUsers = _context.Users.Count();

            var activeUsers = _context.Users.Count(u => u.IsActive);
            ViewBag.ActiveUsers = activeUsers;
            ViewBag.ActivePercent = ViewBag.TotalUsers == 0 ? 0 : (activeUsers * 100 / ViewBag.TotalUsers);

            var since = DateTime.Now.AddHours(-24);
            ViewBag.RecentLogins = _context.SystemAuditLogs
                .Count(l => l.Action == "Login" && l.Timestamp >= since);

            ViewBag.PendingUsers = _context.Users.Count(u => !u.IsActive);

            // Role list
            ViewBag.Roles = _context.Users
                .Select(u => u.Position)
                .Distinct()
                .ToList();

            // Pagination info
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
            ViewBag.From = totalUsers == 0 ? 0 : (page - 1) * pageSize + 1;
            ViewBag.To = Math.Min(page * pageSize, totalUsers);

            // Keep filter state
            ViewBag.Keyword = keyword;
            ViewBag.Role = role;
            ViewBag.Status = status;
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Name).ToList(), "Id", "Name");

            return View("~/Areas/SciTech/Views/Port/User.cshtml", users);
        }

        // GET: /SciTech/User/GetDetail/5
        [HttpGet]
        public IActionResult GetDetail(int id)
        {
            var user = _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    fullName = u.FullName,
                    avatarUrl = u.AvatarUrl,
                    academicRank = u.AcademicRank,
                    degree = u.Degree,
                    departmentName = _context.Departments
                                        .Where(d => d.Id == u.DepartmentId)
                                        .Select(d => d.Name)
                                        .FirstOrDefault() ?? "N/A",
                    position = u.Position,
                    isActive = u.IsActive
                })
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return Json(user);
        }

        // POST: /SciTech/User/Lock/5
        [HttpPost]
        public async Task<IActionResult> Lock(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = false;
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(new DateTime(2099, 1, 1)));

            return RedirectToAction(nameof(Index));
        }

        // POST: /SciTech/User/Unlock/5
        [HttpPost]
        public async Task<IActionResult> Unlock(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.SetLockoutEnabledAsync(user, false);
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }

        // POST: /SciTech/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu nhập vào không hợp lệ. Vui lòng kiểm tra các trường bắt buộc.";
                return RedirectToAction(nameof(Index));
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Email này đã được sử dụng bởi một tài khoản khác.";
                return RedirectToAction(nameof(Index));
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                DepartmentId = model.DepartmentId,
                IsActive = true,
                EmailConfirmed = true,
                AcademicRank = model.AcademicRank,
                Degree = model.Degree,
                Position = model.Position
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                TempData["SuccessMessage"] = "Thêm người dùng " + model.FullName + " thành công!";
                return RedirectToAction(nameof(Index));
            }

            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            TempData["ErrorMessage"] = "Lỗi từ hệ thống: " + errors;

            return RedirectToAction(nameof(Index));
        }

        // GET: /SciTech/User/ExportExcel
        public IActionResult ExportExcel(string keyword, string role, string status)
        {
            var query = _context.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u =>
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword));
            }

            // Filter by role
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Position == role);
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "active":
                        query = query.Where(u => u.IsActive);
                        break;
                    case "locked":
                    case "pending":
                        query = query.Where(u => !u.IsActive);
                        break;
                }
            }

            var users = query
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    u.FullName,
                    u.Email,
                    Role = u.Position,
                    Status = u.IsActive ? "Hoạt động" : "Đã khóa"
                })
                .ToList();

            // Create Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Danh sách người dùng");

            int colCount = 4;

            // Title
            ws.Cells[1, 1, 1, colCount].Merge = true;
            ws.Cells[1, 1].Value = "BÁO CÁO DANH SÁCH NGƯỜI DÙNG";
            ws.Cells[1, 1].Style.Font.Size = 16;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            ws.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Headers
            var headers = new string[] { "Họ tên", "Email", "Vai trò", "Trạng thái" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[2, i + 1].Value = headers[i];
                ws.Cells[2, i + 1].Style.Font.Bold = true;
                ws.Cells[2, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                ws.Cells[2, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[2, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 112, 192));
                ws.Cells[2, i + 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells[2, i + 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                ws.Cells[2, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.Black);
            }

            // Data rows
            int row = 3;
            foreach (var u in users)
            {
                ws.Cells[row, 1].Value = u.FullName;
                ws.Cells[row, 2].Value = u.Email;
                ws.Cells[row, 3].Value = u.Role;
                ws.Cells[row, 4].Value = u.Status;

                for (int col = 1; col <= colCount; col++)
                {
                    ws.Cells[row, col].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                }

                row++;
            }

            ws.Cells.AutoFitColumns();

            var fileName = $"User_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var fileBytes = package.GetAsByteArray();

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}
