using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Controllers
{
    /// <summary>
    /// Public page showing initiatives in Processing/Approved status.
    /// Approver role can approve/revoke initiatives.
    /// </summary>
    public class ProcessingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProcessingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Processing
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, string? status, int page = 1)
        {
            int pageSize = 12;

            // Get latest active period
            var latestPeriod = await _context.InitiativePeriods
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.EndDate)
                .FirstOrDefaultAsync();

            if (latestPeriod == null)
            {
                ViewBag.Categories = new List<SelectListItem>();
                ViewBag.Status = status;
                ViewBag.CategoryId = categoryId;
                ViewBag.CurrentPage = 1;
                ViewBag.TotalPages = 0;
                ViewBag.TotalItems = 0;
                return View(new List<Initiative>());
            }

            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .Include(i => i.Category)
                .Include(i => i.Period)
                    .ThenInclude(p => p.AcademicYear)
                .Where(i => i.PeriodId == latestPeriod.Id &&
                           (i.Status == InitiativeStatus.Processing || i.Status == InitiativeStatus.Approved));

            // Filter by category
            if (categoryId.HasValue)
                query = query.Where(i => i.CategoryId == categoryId.Value);

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<InitiativeStatus>(status, out var parsedStatus))
                query = query.Where(i => i.Status == parsedStatus);

            query = query.OrderByDescending(i => i.SubmittedDate ?? i.CreatedAt);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Categories dropdown
            var categories = await _context.InitiativeCategories
                .Where(c => c.PeriodId == latestPeriod.Id)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PeriodName = latestPeriod.Name;

            return View(items);
        }

        // GET: /Processing/Detail/5
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.Category)
                .Include(i => i.Period)
                    .ThenInclude(p => p != null ? p.AcademicYear : null)
                .Include(i => i.Department)
                .Include(i => i.Files)
                .Include(i => i.Authorships)
                    .ThenInclude(a => a.Author)
                .Include(i => i.FinalResult)
                    .ThenInclude(f => f.Chairman)
                .FirstOrDefaultAsync(i => i.Id == id
                    && (i.Status == InitiativeStatus.Processing || i.Status == InitiativeStatus.Approved));

            if (initiative == null) return NotFound();

            ViewBag.IsApprover = User.IsInRole("Approver");
            return View(initiative);
        }

        // POST: /Processing/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Approver,Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            if (initiative.Status != InitiativeStatus.Processing)
            {
                TempData["ErrorMessage"] = "Only initiatives in Processing status can be approved.";
                return RedirectToAction("Detail", new { id });
            }

            var currentUser = await _userManager.GetUserAsync(User);

            initiative.Status = InitiativeStatus.Approved;
            await _context.SaveChangesAsync();

            // Audit log
            _context.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = currentUser?.Id ?? 0,
                Action = "Approve",
                TargetTable = "Initiatives",
                TargetId = id,
                Details = $"Approved initiative '{initiative.Title}' (Processing → Approved)",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Initiative has been approved!";
            return RedirectToAction("Detail", new { id });
        }

        // POST: /Processing/Revoke/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Approver,Admin")]
        public async Task<IActionResult> Revoke(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null) return NotFound();

            if (initiative.Status != InitiativeStatus.Approved)
            {
                TempData["ErrorMessage"] = "Only approved initiatives can be revoked.";
                return RedirectToAction("Detail", new { id });
            }

            var currentUser = await _userManager.GetUserAsync(User);

            initiative.Status = InitiativeStatus.Processing;
            await _context.SaveChangesAsync();

            // Audit log
            _context.SystemAuditLogs.Add(new SystemAuditLog
            {
                UserId = currentUser?.Id ?? 0,
                Action = "Revoke",
                TargetTable = "Initiatives",
                TargetId = id,
                Details = $"Revoked approval for initiative '{initiative.Title}' (Approved → Processing)",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Timestamp = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Approval has been revoked. Initiative is back to Processing.";
            return RedirectToAction("Detail", new { id });
        }
    }
}
