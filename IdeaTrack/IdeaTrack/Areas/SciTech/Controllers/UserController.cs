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
        public async Task<IActionResult> Index(
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

            // Filter by role (Identity role)
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
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
            int totalUsers = await query.CountAsync();

            var pageUsers = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var users = new List<UserViewModel>(pageUsers.Count);
            foreach (var u in pageUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var displayRole = roles.FirstOrDefault() ?? string.Empty;

                var lastLogin = await _context.SystemAuditLogs
                    .Where(l => l.UserId == u.Id && l.Action == "Login")
                    .OrderByDescending(l => l.Timestamp)
                    .Select(l => (DateTime?)l.Timestamp)
                    .FirstOrDefaultAsync();

                var loginCount = await _context.SystemAuditLogs
                    .CountAsync(l => l.UserId == u.Id && l.Action == "Login");

                users.Add(new UserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    Role = displayRole,
                    LastLogin = lastLogin,
                    LastActive = null,
                    LoginCount = loginCount
                });
            }

            // Dashboard stats
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            ViewBag.ActiveUsers = activeUsers;
            ViewBag.ActivePercent = ViewBag.TotalUsers == 0 ? 0 : (activeUsers * 100 / ViewBag.TotalUsers);

            var since = DateTime.Now.AddHours(-24);
            ViewBag.RecentLogins = await _context.SystemAuditLogs
                .CountAsync(l => l.Action == "Login" && l.Timestamp >= since);

            ViewBag.PendingUsers = await _context.Users.CountAsync(u => !u.IsActive);

            // Role list (Identity)
            ViewBag.Roles = await _context.Roles
                .OrderBy(r => r.Name)
                .Select(r => r.Name!)
                .ToListAsync();

            // Roles for Create User modal
            ViewBag.AllRoles = ViewBag.Roles;

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
            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");

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
                TempData["ErrorMessage"] = "Invalid input data. Please check required fields.";
                return RedirectToAction(nameof(Index));
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "This email is already in use by another account.";
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
                // Assign selected Identity role
                var selectedRole = (model.SelectedRole ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(selectedRole))
                {
                    if (await _context.Roles.AnyAsync(r => r.Name == selectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, selectedRole);
                    }
                }
                TempData["SuccessMessage"] = "Successfully created new user " + model.FullName + "!";
                return RedirectToAction(nameof(Index));
            }

            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            TempData["ErrorMessage"] = "System Error: " + errors;

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

            // Filter by role (Identity role)
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
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

            // Export uses a simple join to get a role name (first role per user)
            var users = query
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    u.FullName,
                    u.Email,
                    Role = (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ur.UserId == u.Id
                            select r.Name).FirstOrDefault(),
                    Status = u.IsActive ? "Active" : "Locked"
                })
                .ToList();

            // Create Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("User_List");

            int colCount = 4;

            // Title
            ws.Cells[1, 1, 1, colCount].Merge = true;
            ws.Cells[1, 1].Value = "USER REPORT";
            ws.Cells[1, 1].Style.Font.Size = 16;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            ws.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Headers
            var headers = new string[] { "Full Name", "Email", "Role", "Status" };
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
