using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Areas.Faculty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using Microsoft.AspNetCore.Hosting;

namespace IdeaTrack.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    [Authorize(Roles = "FacultyLeader,Faculty_Admin,Admin")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
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
                AvatarUrl = user.AvatarUrl ?? "",
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
                AvatarUrl = user.AvatarUrl ?? ""
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

            // Handle avatar file upload to wwwroot
            if (model.AvatarFile != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AvatarFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(fileStream);
                }
                user.AvatarUrl = "/uploads/avatars/" + uniqueFileName;
            }
            // Retain fallback URL mechanism if no file, but a URL was provided
            else if (!string.IsNullOrWhiteSpace(model.AvatarUrl))
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
