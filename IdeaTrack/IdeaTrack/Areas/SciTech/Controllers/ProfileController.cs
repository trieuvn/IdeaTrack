using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Areas.SciTech.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Port", new { area = "SciTech" });
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return RedirectToAction("Index", "Port", new { area = "SciTech" });
            }

            var viewModel = new SciTechProfileVM
            {
                UserId = user.Id,
                FullName = user.FullName ?? user.UserName ?? "Chưa cập nhật",
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Position = user.Position ?? "Chuyên viên quản lý khoa học",
                AcademicRank = user.AcademicRank ?? "Không có",
                Degree = user.Degree ?? "Chưa cập nhật",
                DepartmentName = user.Department?.Name ?? "Phòng Quản lý Khoa học",
                AvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(user.FullName ?? user.UserName ?? "X") + "&background=be123c&color=fff&size=150" : user.AvatarUrl,
                IsActive = user.IsActive,
                
                // Static / Placeholder mappings for properties missing from DB to match mock design
                StartDate = "01/09/2018", // Placeholder
                TenureStatus = "Hợp đồng không xác định thời hạn", // Placeholder
                TeacherTitle = "Không", // Placeholder
                DateOfBirth = "19/02/1995", // Placeholder
                BirthPlace = "Bình Dương", // Placeholder
                Gender = "Nam", // Placeholder
                EmployeeCode = user.UserName ?? ("EMP" + user.Id.ToString()),
                Nationality = "Việt Nam", // Placeholder
                Religion = "Không", // Placeholder
                Ethnicity = "Kinh", // Placeholder
                ContactAddress = "KP3A - P. THỚI HÒA - TX BẾN CÁT - TỈNH BÌNH DƯƠNG" // Placeholder
            };

            return View(viewModel);
        }
    }
}
