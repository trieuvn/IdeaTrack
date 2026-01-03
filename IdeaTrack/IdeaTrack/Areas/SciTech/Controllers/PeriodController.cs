using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class PeriodController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInitiativeService _initiativeService;
        private readonly ILogger<PeriodController> _logger;

        public PeriodController(
            ApplicationDbContext context,
            IInitiativeService initiativeService,
            ILogger<PeriodController> logger)
        {
            _context = context;
            _initiativeService = initiativeService;
            _logger = logger;
        }

        // GET: /SciTech/Period
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;

            var query = _context.InitiativePeriods
                .Include(p => p.AcademicYear)
                .Include(p => p.Categories)
                .Include(p => p.Initiatives)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PeriodViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    AcademicYearName = p.AcademicYear.Name,
                    AcademicYearId = p.AcademicYearId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    CategoryCount = p.Categories.Count,
                    InitiativeCount = p.Initiatives.Count
                })
                .ToListAsync();

            var vm = new PeriodListVM
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(vm);
        }

        // GET: /SciTech/Period/Create
        public async Task<IActionResult> Create()
        {
            var vm = new PeriodCreateEditVM
            {
                AcademicYears = await GetAcademicYearSelectList(),
                ExistingPeriods = await GetPeriodSelectList()
            };

            return View(vm);
        }

        // POST: /SciTech/Period/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PeriodCreateEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AcademicYears = await GetAcademicYearSelectList();
                vm.ExistingPeriods = await GetPeriodSelectList();
                return View(vm);
            }

            try
            {
                // If setting as active, deactivate others
                if (vm.IsActive)
                {
                    var activePeriods = await _context.InitiativePeriods
                        .Where(p => p.IsActive)
                        .ToListAsync();

                    foreach (var p in activePeriods)
                    {
                        p.IsActive = false;
                    }
                }

                var period = new InitiativePeriod
                {
                    Name = vm.Name,
                    AcademicYearId = vm.AcademicYearId,
                    StartDate = vm.StartDate,
                    EndDate = vm.EndDate,
                    IsActive = vm.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.InitiativePeriods.Add(period);
                await _context.SaveChangesAsync();

                // Clone categories if requested
                if (vm.CloneFromPeriodId.HasValue && vm.CloneFromPeriodId.Value > 0)
                {
                    await _initiativeService.ClonePeriodDataAsync(vm.CloneFromPeriodId.Value, period.Id);
                    TempData["SuccessMessage"] = $"Đã tạo đợt \"{period.Name}\" và sao chép danh mục thành công!";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Đã tạo đợt \"{period.Name}\" thành công!";
                }

                _logger.LogInformation("Created InitiativePeriod {Id}: {Name}", period.Id, period.Name);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating InitiativePeriod");
                TempData["ErrorMessage"] = "Lỗi khi tạo đợt sáng kiến. Vui lòng thử lại.";
                vm.AcademicYears = await GetAcademicYearSelectList();
                vm.ExistingPeriods = await GetPeriodSelectList();
                return View(vm);
            }
        }

        // GET: /SciTech/Period/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var period = await _context.InitiativePeriods.FindAsync(id);
            if (period == null)
            {
                return NotFound();
            }

            var vm = new PeriodCreateEditVM
            {
                Id = period.Id,
                Name = period.Name,
                AcademicYearId = period.AcademicYearId,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                IsActive = period.IsActive,
                AcademicYears = await GetAcademicYearSelectList(),
                ExistingPeriods = await GetPeriodSelectList()
            };

            return View(vm);
        }

        // POST: /SciTech/Period/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PeriodCreateEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AcademicYears = await GetAcademicYearSelectList();
                vm.ExistingPeriods = await GetPeriodSelectList();
                return View(vm);
            }

            try
            {
                var period = await _context.InitiativePeriods.FindAsync(vm.Id);
                if (period == null)
                {
                    return NotFound();
                }

                // If setting as active, deactivate others
                if (vm.IsActive && !period.IsActive)
                {
                    var activePeriods = await _context.InitiativePeriods
                        .Where(p => p.IsActive && p.Id != vm.Id)
                        .ToListAsync();

                    foreach (var p in activePeriods)
                    {
                        p.IsActive = false;
                    }
                }

                period.Name = vm.Name;
                period.AcademicYearId = vm.AcademicYearId;
                period.StartDate = vm.StartDate;
                period.EndDate = vm.EndDate;
                period.IsActive = vm.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated InitiativePeriod {Id}: {Name}", period.Id, period.Name);
                TempData["SuccessMessage"] = $"Đã cập nhật đợt \"{period.Name}\" thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating InitiativePeriod {Id}", vm.Id);
                TempData["ErrorMessage"] = "Lỗi khi cập nhật đợt sáng kiến. Vui lòng thử lại.";
                vm.AcademicYears = await GetAcademicYearSelectList();
                vm.ExistingPeriods = await GetPeriodSelectList();
                return View(vm);
            }
        }

        // POST: /SciTech/Period/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var period = await _context.InitiativePeriods.FindAsync(id);
                if (period == null)
                {
                    return NotFound();
                }

                if (!period.IsActive)
                {
                    // Activating: deactivate all others first
                    var activePeriods = await _context.InitiativePeriods
                        .Where(p => p.IsActive)
                        .ToListAsync();

                    foreach (var p in activePeriods)
                    {
                        p.IsActive = false;
                    }

                    period.IsActive = true;
                    TempData["SuccessMessage"] = $"Đã mở đợt \"{period.Name}\"!";
                }
                else
                {
                    // Deactivating
                    period.IsActive = false;
                    TempData["SuccessMessage"] = $"Đã đóng đợt \"{period.Name}\"!";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Toggled InitiativePeriod {Id} active: {IsActive}", period.Id, period.IsActive);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling InitiativePeriod {Id}", id);
                TempData["ErrorMessage"] = "Lỗi khi thay đổi trạng thái đợt. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /SciTech/Period/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var period = await _context.InitiativePeriods
                    .Include(p => p.Initiatives)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (period == null)
                {
                    return NotFound();
                }

                if (period.Initiatives.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa đợt \"{period.Name}\" vì có sáng kiến liên quan.";
                    return RedirectToAction(nameof(Index));
                }

                _context.InitiativePeriods.Remove(period);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted InitiativePeriod {Id}: {Name}", period.Id, period.Name);
                TempData["SuccessMessage"] = $"Đã xóa đợt \"{period.Name}\" thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting InitiativePeriod {Id}", id);
                TempData["ErrorMessage"] = "Lỗi khi xóa đợt sáng kiến. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Helper Methods

        private async Task<List<SelectListItem>> GetAcademicYearSelectList()
        {
            return await _context.AcademicYears
                .OrderByDescending(y => y.Name)
                .Select(y => new SelectListItem
                {
                    Value = y.Id.ToString(),
                    Text = y.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetPeriodSelectList()
        {
            return await _context.InitiativePeriods
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                })
                .ToListAsync();
        }

        #endregion
    }
}
