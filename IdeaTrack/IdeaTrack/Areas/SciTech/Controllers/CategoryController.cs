using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /SciTech/Category
        public async Task<IActionResult> Index(int? periodId)
        {
            var periods = await GetPeriodSelectList();
            
            // Default to active period if none specified
            if (!periodId.HasValue)
            {
                var activePeriod = await _context.InitiativePeriods
                    .FirstOrDefaultAsync(p => p.IsActive);
                periodId = activePeriod?.Id;
            }

            var selectedPeriod = periodId.HasValue
                ? await _context.InitiativePeriods.FindAsync(periodId.Value)
                : null;

            var query = _context.InitiativeCategories
                .Include(c => c.Period)
                .Include(c => c.Board)
                .Include(c => c.Template)
                .Include(c => c.Initiatives)
                .AsQueryable();

            if (periodId.HasValue)
            {
                query = query.Where(c => c.PeriodId == periodId.Value);
            }

            var items = await query
                .OrderBy(c => c.Name)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    PeriodId = c.PeriodId,
                    PeriodName = c.Period.Name,
                    BoardId = c.BoardId,
                    BoardName = c.Board != null ? c.Board.BoardName : null,
                    TemplateId = c.TemplateId,
                    TemplateName = c.Template != null ? c.Template.TemplateName : null,
                    InitiativeCount = c.Initiatives.Count
                })
                .ToListAsync();

            var vm = new CategoryListVM
            {
                SelectedPeriodId = periodId,
                SelectedPeriodName = selectedPeriod?.Name,
                Items = items,
                Periods = periods
            };

            return View(vm);
        }

        // GET: /SciTech/Category/Create
        public async Task<IActionResult> Create(int? periodId)
        {
            var vm = new CategoryCreateEditVM
            {
                PeriodId = periodId ?? 0,
                Periods = await GetPeriodSelectList(),
                Boards = await GetBoardSelectList(),
                Templates = await GetTemplateSelectList()
            };

            return View(vm);
        }

        // POST: /SciTech/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Periods = await GetPeriodSelectList();
                vm.Boards = await GetBoardSelectList();
                vm.Templates = await GetTemplateSelectList();
                return View(vm);
            }

            try
            {
                var category = new InitiativeCategory
                {
                    Name = vm.Name,
                    Description = vm.Description,
                    PeriodId = vm.PeriodId,
                    BoardId = vm.BoardId,
                    TemplateId = vm.TemplateId
                };

                _context.InitiativeCategories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created Category {Id}: {Name} with Board={BoardId}, Template={TemplateId}",
                    category.Id, category.Name, category.BoardId, category.TemplateId);

                TempData["SuccessMessage"] = $"Đã tạo danh mục \"{category.Name}\" thành công!";
                return RedirectToAction(nameof(Index), new { periodId = vm.PeriodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Category");
                TempData["ErrorMessage"] = "Lỗi khi tạo danh mục. Vui lòng thử lại.";
                vm.Periods = await GetPeriodSelectList();
                vm.Boards = await GetBoardSelectList();
                vm.Templates = await GetTemplateSelectList();
                return View(vm);
            }
        }

        // GET: /SciTech/Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.InitiativeCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var vm = new CategoryCreateEditVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                PeriodId = category.PeriodId,
                BoardId = category.BoardId,
                TemplateId = category.TemplateId,
                Periods = await GetPeriodSelectList(),
                Boards = await GetBoardSelectList(),
                Templates = await GetTemplateSelectList()
            };

            return View(vm);
        }

        // POST: /SciTech/Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryCreateEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Periods = await GetPeriodSelectList();
                vm.Boards = await GetBoardSelectList();
                vm.Templates = await GetTemplateSelectList();
                return View(vm);
            }

            try
            {
                var category = await _context.InitiativeCategories.FindAsync(vm.Id);
                if (category == null)
                {
                    return NotFound();
                }

                category.Name = vm.Name;
                category.Description = vm.Description;
                category.PeriodId = vm.PeriodId;
                category.BoardId = vm.BoardId;
                category.TemplateId = vm.TemplateId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated Category {Id}: {Name} with Board={BoardId}, Template={TemplateId}",
                    category.Id, category.Name, category.BoardId, category.TemplateId);

                TempData["SuccessMessage"] = $"Đã cập nhật danh mục \"{category.Name}\" thành công!";
                return RedirectToAction(nameof(Index), new { periodId = vm.PeriodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Category {Id}", vm.Id);
                TempData["ErrorMessage"] = "Lỗi khi cập nhật danh mục. Vui lòng thử lại.";
                vm.Periods = await GetPeriodSelectList();
                vm.Boards = await GetBoardSelectList();
                vm.Templates = await GetTemplateSelectList();
                return View(vm);
            }
        }

        // POST: /SciTech/Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.InitiativeCategories
                    .Include(c => c.Initiatives)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound();
                }

                if (category.Initiatives.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa danh mục \"{category.Name}\" vì có sáng kiến liên quan.";
                    return RedirectToAction(nameof(Index), new { periodId = category.PeriodId });
                }

                var periodId = category.PeriodId;
                _context.InitiativeCategories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted Category {Id}: {Name}", category.Id, category.Name);
                TempData["SuccessMessage"] = $"Đã xóa danh mục \"{category.Name}\" thành công!";

                return RedirectToAction(nameof(Index), new { periodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Category {Id}", id);
                TempData["ErrorMessage"] = "Lỗi khi xóa danh mục. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private async Task<List<SelectListItem>> GetPeriodSelectList()
        {
            return await _context.InitiativePeriods
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name + (p.IsActive ? " (Đang mở)" : "")
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetBoardSelectList()
        {
            return await _context.Boards
                .Where(b => b.IsActive)
                .OrderBy(b => b.BoardName)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.BoardName
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetTemplateSelectList()
        {
            return await _context.EvaluationTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.TemplateName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.TemplateName
                })
                .ToListAsync();
        }

        #endregion
    }
}
