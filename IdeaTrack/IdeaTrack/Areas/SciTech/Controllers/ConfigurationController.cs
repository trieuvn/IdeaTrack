using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class ConfigurationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfigurationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =================================================
        // 1. ACADEMIC YEAR MANAGEMENT
        // =================================================

        // GET: /SciTech/Configuration/AcademicYear
        public async Task<IActionResult> AcademicYear()
        {
            var items = await _context.AcademicYears.OrderByDescending(y => y.Name).ToListAsync();
            return View("~/Areas/SciTech/Views/Configuration/AcademicYear.cshtml", items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateYear(string name, bool isCurrent)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name required");

            if (isCurrent)
            {
                var currents = await _context.AcademicYears.Where(y => y.IsCurrent).ToListAsync();
                currents.ForEach(y => y.IsCurrent = false);
            }

            _context.AcademicYears.Add(new AcademicYear { Name = name, IsCurrent = isCurrent });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AcademicYear));
        }

        [HttpPost]
        public async Task<IActionResult> SetCurrentYear(int id)
        {
            var year = await _context.AcademicYears.FindAsync(id);
            if (year == null) return NotFound();

            var oldCurrents = await _context.AcademicYears.Where(y => y.IsCurrent).ToListAsync();
            oldCurrents.ForEach(y => y.IsCurrent = false);

            year.IsCurrent = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteYear(int id)
        {
            var year = await _context.AcademicYears.Include(y => y.Periods).FirstOrDefaultAsync(y => y.Id == id);
            if (year == null) return NotFound();
            
            if (year.Periods.Any()) return BadRequest("Cannot delete year containing periods.");

            _context.AcademicYears.Remove(year);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // =================================================
        // 2. INITIATIVE PERIOD MANAGEMENT
        // =================================================

        // GET: /SciTech/Configuration/InitiativePeriod?yearId=...&status=...
        public async Task<IActionResult> InitiativePeriod(int? yearId, string status = "All")
        {
            // Default to current year if not specified
            if (!yearId.HasValue)
            {
                var current = await _context.AcademicYears.FirstOrDefaultAsync(y => y.IsCurrent);
                if (current != null) yearId = current.Id;
            }

            ViewBag.Years = await _context.AcademicYears.OrderByDescending(y => y.Name).ToListAsync();
            ViewBag.SelectedYearId = yearId;
            ViewBag.SelectedStatus = status;

            IQueryable<InitiativePeriod> query = _context.InitiativePeriods
                .Include(p => p.AcademicYear)
                .Include(p => p.Categories)
                .OrderByDescending(p => p.StartDate);

            if (yearId.HasValue)
            {
                query = query.Where(p => p.AcademicYearId == yearId);
            }

            var allItems = await query.ToListAsync();
            var now = DateTime.Now;

            // Client-side/Memory filtering for date logic is safer/easier than complex LINQ to SQL date diffs
            if (status == "Open")
            {
                allItems = allItems.Where(p => now >= p.StartDate && now <= p.EndDate).ToList();
            }
            else if (status == "Closed")
            {
                allItems = allItems.Where(p => now < p.StartDate || now > p.EndDate).ToList();
            }

            return View("~/Areas/SciTech/Views/Configuration/InitiativePeriod.cshtml", allItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePeriod(int yearId, string name, DateTime startDate, DateTime endDate, string? description)
        {
            // Allow multiple open periods - just check if this specific period is active by date
            bool isActive = (DateTime.Now >= startDate && DateTime.Now <= endDate);

            _context.InitiativePeriods.Add(new InitiativePeriod
            {
                AcademicYearId = yearId,
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Description = description,
                IsActive = isActive
            });
            await _context.SaveChangesAsync();
            
            // Redirect preserving year filter
            return RedirectToAction(nameof(InitiativePeriod), new { yearId });
        }
        
        [HttpPost]
        public async Task<IActionResult> DeletePeriod(int id)
        {
            var period = await _context.InitiativePeriods.Include(p => p.Initiatives).FirstOrDefaultAsync(p => p.Id == id);
            if (period == null) return NotFound();
            if (period.Initiatives.Any()) return BadRequest("Cannot delete period containing initiatives.");

            _context.InitiativePeriods.Remove(period);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // =================================================
        // 3. CATEGORY MANAGEMENT & CLONING
        // =================================================

        // GET: /SciTech/Configuration/InitiativeCategory?periodId=...
        public async Task<IActionResult> InitiativeCategory(int? periodId)
        {
             // Default to active period if not specified
            if (!periodId.HasValue)
            {
                var active = await _context.InitiativePeriods.OrderByDescending(p => p.IsActive).ThenByDescending(p => p.Id).FirstOrDefaultAsync();
                if (active != null) periodId = active.Id;
            }
            
            // Get all periods for clone dropdown and filter dropdown
            var yearGroups = await _context.InitiativePeriods
                .Include(p => p.AcademicYear)
                .OrderByDescending(p => p.AcademicYear.Name)
                .ThenByDescending(p => p.StartDate)
                .ToListAsync();

            ViewBag.Periods = yearGroups;
            ViewBag.SelectedPeriodId = periodId;
            ViewBag.Boards = await _context.Boards.Where(b => b.IsActive).ToListAsync();
            ViewBag.Templates = await _context.EvaluationTemplates.Where(t => t.IsActive).ToListAsync();

            List<InitiativeCategory> items = new List<InitiativeCategory>();
            if (periodId.HasValue)
            {
                items = await _context.InitiativeCategories
                    .Include(c => c.Board)
                    .Include(c => c.Template)
                    .Where(c => c.PeriodId == periodId)
                    .ToListAsync();
            }

            return View("~/Areas/SciTech/Views/Configuration/InitiativeCategory.cshtml", items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(int periodId, string name, int? boardId, int? templateId)
        {
            _context.InitiativeCategories.Add(new InitiativeCategory
            {
                PeriodId = periodId,
                Name = name,
                BoardId = boardId,
                TemplateId = templateId
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(InitiativeCategory), new { periodId });
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
             var cat = await _context.InitiativeCategories.Include(c => c.Initiatives).FirstOrDefaultAsync(c => c.Id == id);
             if (cat == null) return NotFound();
             if (cat.Initiatives.Any()) return BadRequest("Cannot delete category containing initiatives.");
             
             _context.InitiativeCategories.Remove(cat);
             await _context.SaveChangesAsync();
             return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CloneCategories(int sourcePeriodId, int targetPeriodId)
        {
            if (sourcePeriodId == targetPeriodId) return BadRequest("Source and target cannot be the same.");

            var sourceCats = await _context.InitiativeCategories
                .AsNoTracking() // Important for cloning
                .Where(c => c.PeriodId == sourcePeriodId)
                .ToListAsync();

            if (!sourceCats.Any()) return BadRequest("Source period has no categories.");

            foreach (var cat in sourceCats)
            {
                // Create new instance
                _context.InitiativeCategories.Add(new InitiativeCategory
                {
                    PeriodId = targetPeriodId, // Reassignment
                    Name = cat.Name,
                    Description = cat.Description,
                    BoardId = cat.BoardId,
                    TemplateId = cat.TemplateId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(InitiativeCategory), new { periodId = targetPeriodId });
        }

        // =================================================
        // 4. CATEGORY DETAILS & SYNC
        // =================================================

        // GET: /SciTech/Configuration/CategoryDetails/5
        public async Task<IActionResult> CategoryDetails(int id)
        {
            var category = await _context.InitiativeCategories
                .Include(c => c.Board)
                .Include(c => c.Template)
                .Include(c => c.Period)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            ViewBag.Boards = await _context.Boards
                .Where(b => b.IsActive)
                .Include(b => b.Members)
                .ToListAsync();
            ViewBag.Templates = await _context.EvaluationTemplates
                .Where(t => t.IsActive)
                .ToListAsync();
            ViewBag.InitiativeCount = await _context.Initiatives
                .CountAsync(i => i.CategoryId == id);

            return View("~/Areas/SciTech/Views/Configuration/CategoryDetails.cshtml", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(int id, int? boardId, int? templateId)
        {
            var category = await _context.InitiativeCategories.FindAsync(id);
            if (category == null) return NotFound();

            category.BoardId = boardId;
            category.TemplateId = templateId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật thông tin danh mục!";
            return RedirectToAction(nameof(CategoryDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncCategoryInitiatives(int id)
        {
            var category = await _context.InitiativeCategories.FindAsync(id);
            if (category == null) return NotFound();

            // Get count of affected initiatives
            var initiativeCount = await _context.Initiatives
                .CountAsync(i => i.CategoryId == id);

            // Note: Since Initiative references Board/Template via Category,
            // updating the Category automatically affects all linked initiatives.
            // The sync here is just to confirm and provide feedback.
            
            TempData["SuccessMessage"] = $"Đã áp dụng thay đổi cho {initiativeCount} sáng kiến thuộc danh mục này!";
            return RedirectToAction(nameof(CategoryDetails), new { id });
        }

        // =================================================
        // 5. QUICK CREATE COUNCIL & TEMPLATE
        // =================================================

        [HttpPost]
        public async Task<IActionResult> QuickCreateCouncil(int categoryId, string name, int? cloneFromBoardId)
        {
            try
            {
                var category = await _context.InitiativeCategories
                    .Include(c => c.Period)
                    .FirstOrDefaultAsync(c => c.Id == categoryId);
                if (category == null)
                    return Json(new { success = false, message = "Category not found" });

                // Create new board
                var newBoard = new Board
                {
                    BoardName = name,
                    FiscalYear = DateTime.Now.Year,
                    IsActive = true
                };
                _context.Boards.Add(newBoard);
                await _context.SaveChangesAsync();

                // Clone members if specified
                if (cloneFromBoardId.HasValue)
                {
                    var sourceBoard = await _context.Boards
                        .Include(b => b.Members)
                        .FirstOrDefaultAsync(b => b.Id == cloneFromBoardId.Value);

                    if (sourceBoard?.Members != null)
                    {
                        foreach (var member in sourceBoard.Members)
                        {
                            _context.BoardMembers.Add(new BoardMember
                            {
                                BoardId = newBoard.Id,
                                UserId = member.UserId,
                                Role = member.Role
                            });
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                // Assign new board to category
                category.BoardId = newBoard.Id;
                await _context.SaveChangesAsync();

                return Json(new { success = true, boardId = newBoard.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickCreateTemplate(int categoryId, string name)
        {
            try
            {
                var category = await _context.InitiativeCategories.FindAsync(categoryId);
                if (category == null)
                    return Json(new { success = false, message = "Category not found" });

                // Create new template (empty, no criteria yet)
                var newTemplate = new EvaluationTemplate
                {
                    TemplateName = name,
                    IsActive = true
                };
                _context.EvaluationTemplates.Add(newTemplate);
                await _context.SaveChangesAsync();

                // Assign new template to category
                category.TemplateId = newTemplate.Id;
                await _context.SaveChangesAsync();

                return Json(new { success = true, templateId = newTemplate.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
