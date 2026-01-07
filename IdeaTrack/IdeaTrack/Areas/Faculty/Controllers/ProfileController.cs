using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Areas.Faculty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IdeaTrack.Areas.Faculty.Controllers
{
    /// <summary>
    /// Dedicated ProfileController for Faculty area.
    /// Handles user profile viewing, editing, and account actions.
    /// Separated from DashboardController for clear responsibility.
    /// </summary>
    [Area("Faculty")]
    [Authorize(Roles = "FacultyLeader,Faculty_Admin,Admin")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ==========================================
        // 1. VIEW PROFILE (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var initiativeCount = await _context.Initiatives
                .CountAsync(i => i.CreatorId == user.Id);

            var viewModel = new FacultyProfileVM
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Position = user.Position ?? "Trưởng khoa",
                AcademicRank = user.AcademicRank,
                Degree = user.Degree,
                DepartmentName = user.Department?.Name ?? "Chưa cập nhật",
                AvatarUrl = user.AvatarUrl ?? "https://i.pravatar.cc/150?img=32",
                InitiativeCount = initiativeCount,
                AchievementCount = 5 // Placeholder
            };

            return View(viewModel);
        }

        // ==========================================
        // 2. EDIT PROFILE (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = new ProfileEditVM
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Position = user.Position ?? "",
                AcademicRank = user.AcademicRank ?? "",
                Degree = user.Degree ?? "",
                AvatarUrl = user.AvatarUrl ?? "https://i.pravatar.cc/150?img=32"
            };

            return View(viewModel);
        }

        // ==========================================
        // 3. EDIT PROFILE (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            // Update user profile
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Position = model.Position;
            user.AcademicRank = model.AcademicRank;
            user.Degree = model.Degree;

            // Handle avatar URL if provided
            if (!string.IsNullOrWhiteSpace(model.AvatarUrl))
            {
                user.AvatarUrl = model.AvatarUrl;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật thông tin: " + ex.Message);
                return View(model);
            }
        }

        // ==========================================
        // 4. LOGOUT (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}
