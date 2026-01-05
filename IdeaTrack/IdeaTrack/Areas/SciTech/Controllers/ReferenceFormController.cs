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
    public class ReferenceFormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;
        private readonly ILogger<ReferenceFormController> _logger;

        // Allowed file extensions
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public ReferenceFormController(
            ApplicationDbContext context,
            IStorageService storageService,
            ILogger<ReferenceFormController> logger)
        {
            _context = context;
            _storageService = storageService;
            _logger = logger;
        }

        // GET: /SciTech/ReferenceForm
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

            var query = _context.ReferenceForms
                .Include(rf => rf.Period)
                .AsQueryable();

            if (periodId.HasValue)
            {
                query = query.Where(rf => rf.PeriodId == periodId.Value);
            }

            var items = await query
                .OrderBy(rf => rf.FormName)
                .Select(rf => new ReferenceFormViewModel
                {
                    Id = rf.Id,
                    FormName = rf.FormName,
                    FileUrl = rf.FileUrl,
                    FileName = rf.FileName,
                    FileType = rf.FileType,
                    Description = rf.Description,
                    PeriodId = rf.PeriodId,
                    PeriodName = rf.Period.Name,
                    UploadedAt = rf.UploadedAt
                })
                .ToListAsync();

            var vm = new ReferenceFormListVM
            {
                SelectedPeriodId = periodId,
                SelectedPeriodName = selectedPeriod?.Name,
                Items = items,
                Periods = periods
            };

            return View(vm);
        }

        // GET: /SciTech/ReferenceForm/Create
        public async Task<IActionResult> Create(int? periodId)
        {
            var vm = new ReferenceFormCreateVM
            {
                PeriodId = periodId ?? 0,
                Periods = await GetPeriodSelectList()
            };

            return View(vm);
        }

        // POST: /SciTech/ReferenceForm/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReferenceFormCreateVM vm)
        {
            // Validate file
            if (vm.File == null || vm.File.Length == 0)
            {
                ModelState.AddModelError("File", "Vui lAAAAAAAAAAAAAAAA²ng chAAAAAAAA¡AAAAAAAA»AAAAAAAAn file");
            }
            else
            {
                var extension = Path.GetExtension(vm.File.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("File", $"ChAAAAAAAA¡AAAAAAAA»AAAAAAAA chAAAAAAAA¡AAAAAAAAºAAAAAAAA¥p nhAAAAAAAA¡AAAAAAAAºAAAAAAAA­n cAAAAAAAAAAAAAAAA¡c AAAAAAAAAAAAAAAAAAAAAAAA¡AAAAAAAA»AAAAAAAAnh dAAAAAAAA¡AAAAAAAAºAAAAAAAA¡ng: {string.Join(", ", AllowedExtensions)}");
                }
                if (vm.File.Length > MaxFileSize)
                {
                    ModelState.AddModelError("File", "File khAAAAAAAAAAAAAAAA´ng AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA°AAAAAAAA¡AAAAAAAA»AAAAAAAA£c vAAAAAAAAAAAAAAAA°AAAAAAAA¡AAAAAAAA»AAAAAAAA£t quAAAAAAAAAAAAAAAA¡ 10MB");
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Periods = await GetPeriodSelectList();
                return View(vm);
            }

            try
            {
                // Upload file to Supabase
                using var fileStream = vm.File!.OpenReadStream();
                var fileUrl = await _storageService.UploadFileAsync(
                    fileStream,
                    vm.File.FileName,
                    "reference-forms");

                if (string.IsNullOrEmpty(fileUrl))
                {
                    TempData["ErrorMessage"] = "LAAAAAAAA¡AAAAAAAA»AAAAAAAAi khi tAAAAAAAA¡AAAAAAAAºAAAAAAAA£i file lAAAAAAAAAAAAAAAAªn. Vui lAAAAAAAAAAAAAAAA²ng thAAAAAAAA¡AAAAAAAA»AAAAAAAA­ lAAAAAAAA¡AAAAAAAAºAAAAAAAA¡i.";
                    vm.Periods = await GetPeriodSelectList();
                    return View(vm);
                }

                var referenceForm = new ReferenceForm
                {
                    FormName = vm.FormName,
                    Description = vm.Description,
                    PeriodId = vm.PeriodId,
                    FileUrl = fileUrl,
                    FileName = vm.File.FileName,
                    FileType = Path.GetExtension(vm.File.FileName).TrimStart('.').ToLowerInvariant(),
                    UploadedAt = DateTime.Now
                };

                _context.ReferenceForms.Add(referenceForm);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created ReferenceForm {Id}: {Name} - {FileUrl}",
                    referenceForm.Id, referenceForm.FormName, referenceForm.FileUrl);

                TempData["SuccessMessage"] = $"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA£ tAAAAAAAA¡AAAAAAAAºAAAAAAAA¡o biAAAAAAAA¡AAAAAAAA»AAAAAAAAu mAAAAAAAA¡AAAAAAAAºAAAAAAAA«u \"{referenceForm.FormName}\" thAAAAAAAAAAAAAAAA nh cAAAAAAAAAAAAAAAA´ng!";
                return RedirectToAction(nameof(Index), new { periodId = vm.PeriodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ReferenceForm");
                TempData["ErrorMessage"] = "LAAAAAAAA¡AAAAAAAA»AAAAAAAAi khi tAAAAAAAA¡AAAAAAAAºAAAAAAAA¡o biAAAAAAAA¡AAAAAAAA»AAAAAAAAu mAAAAAAAA¡AAAAAAAAºAAAAAAAA«u. Vui lAAAAAAAAAAAAAAAA²ng thAAAAAAAA¡AAAAAAAA»AAAAAAAA­ lAAAAAAAA¡AAAAAAAAºAAAAAAAA¡i.";
                vm.Periods = await GetPeriodSelectList();
                return View(vm);
            }
        }

        // GET: /SciTech/ReferenceForm/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var referenceForm = await _context.ReferenceForms.FindAsync(id);
            if (referenceForm == null)
            {
                return NotFound();
            }

            var vm = new ReferenceFormEditVM
            {
                Id = referenceForm.Id,
                FormName = referenceForm.FormName,
                Description = referenceForm.Description,
                PeriodId = referenceForm.PeriodId,
                FileUrl = referenceForm.FileUrl,
                FileName = referenceForm.FileName,
                Periods = await GetPeriodSelectList()
            };

            return View(vm);
        }

        // POST: /SciTech/ReferenceForm/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReferenceFormEditVM vm)
        {
            // Validate new file if provided
            if (vm.NewFile != null && vm.NewFile.Length > 0)
            {
                var extension = Path.GetExtension(vm.NewFile.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("NewFile", $"ChAAAAAAAA¡AAAAAAAA»AAAAAAAA chAAAAAAAA¡AAAAAAAAºAAAAAAAA¥p nhAAAAAAAA¡AAAAAAAAºAAAAAAAA­n cAAAAAAAAAAAAAAAA¡c AAAAAAAAAAAAAAAAAAAAAAAA¡AAAAAAAA»AAAAAAAAnh dAAAAAAAA¡AAAAAAAAºAAAAAAAA¡ng: {string.Join(", ", AllowedExtensions)}");
                }
                if (vm.NewFile.Length > MaxFileSize)
                {
                    ModelState.AddModelError("NewFile", "File khAAAAAAAAAAAAAAAA´ng AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA°AAAAAAAA¡AAAAAAAA»AAAAAAAA£c vAAAAAAAAAAAAAAAA°AAAAAAAA¡AAAAAAAA»AAAAAAAA£t quAAAAAAAAAAAAAAAA¡ 10MB");
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Periods = await GetPeriodSelectList();
                return View(vm);
            }

            try
            {
                var referenceForm = await _context.ReferenceForms.FindAsync(vm.Id);
                if (referenceForm == null)
                {
                    return NotFound();
                }

                // Handle file replacement if new file is provided
                if (vm.NewFile != null && vm.NewFile.Length > 0)
                {
                    // Delete old file
                    if (!string.IsNullOrEmpty(referenceForm.FileUrl))
                    {
                        await _storageService.DeleteFileAsync(referenceForm.FileUrl);
                    }

                    // Upload new file
                    using var fileStream = vm.NewFile.OpenReadStream();
                    var fileUrl = await _storageService.UploadFileAsync(
                        fileStream,
                        vm.NewFile.FileName,
                        "reference-forms");

                    if (string.IsNullOrEmpty(fileUrl))
                    {
                        TempData["ErrorMessage"] = "LAAAAAAAA¡AAAAAAAA»AAAAAAAAi khi tAAAAAAAA¡AAAAAAAAºAAAAAAAA£i file mAAAAAAAA¡AAAAAAAA»AAAAAAAAi lAAAAAAAAAAAAAAAAªn. Vui lAAAAAAAAAAAAAAAA²ng thAAAAAAAA¡AAAAAAAA»AAAAAAAA­ lAAAAAAAA¡AAAAAAAAºAAAAAAAA¡i.";
                        vm.Periods = await GetPeriodSelectList();
                        return View(vm);
                    }

                    referenceForm.FileUrl = fileUrl;
                    referenceForm.FileName = vm.NewFile.FileName;
                    referenceForm.FileType = Path.GetExtension(vm.NewFile.FileName).TrimStart('.').ToLowerInvariant();
                    referenceForm.UploadedAt = DateTime.Now;
                }

                referenceForm.FormName = vm.FormName;
                referenceForm.Description = vm.Description;
                referenceForm.PeriodId = vm.PeriodId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated ReferenceForm {Id}: {Name}", referenceForm.Id, referenceForm.FormName);
                TempData["SuccessMessage"] = $"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA£ cAAAAAAAA¡AAAAAAAAºAAAAAAAA­p nhAAAAAAAA¡AAAAAAAAºAAAAAAAA­t biAAAAAAAA¡AAAAAAAA»AAAAAAAAu mAAAAAAAA¡AAAAAAAAºAAAAAAAA«u \"{referenceForm.FormName}\" thAAAAAAAAAAAAAAAA nh cAAAAAAAAAAAAAAAA´ng!";

                return RedirectToAction(nameof(Index), new { periodId = vm.PeriodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ReferenceForm {Id}", vm.Id);
                TempData["ErrorMessage"] = "LAAAAAAAA¡AAAAAAAA»AAAAAAAAi khi cAAAAAAAA¡AAAAAAAAºAAAAAAAA­p nhAAAAAAAA¡AAAAAAAAºAAAAAAAA­t biAAAAAAAA¡AAAAAAAA»AAAAAAAAu mAAAAAAAA¡AAAAAAAAºAAAAAAAA«u. Vui lAAAAAAAAAAAAAAAA²ng thAAAAAAAA¡AAAAAAAA»AAAAAAAA­ lAAAAAAAA¡AAAAAAAAºAAAAAAAA¡i.";
                vm.Periods = await GetPeriodSelectList();
                return View(vm);
            }
        }

        // POST: /SciTech/ReferenceForm/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var referenceForm = await _context.ReferenceForms.FindAsync(id);
                if (referenceForm == null)
                {
                    return NotFound();
                }

                var periodId = referenceForm.PeriodId;
                var formName = referenceForm.FormName;

                // Delete file from Supabase
                if (!string.IsNullOrEmpty(referenceForm.FileUrl))
                {
                    await _storageService.DeleteFileAsync(referenceForm.FileUrl);
                }

                // Delete record
                _context.ReferenceForms.Remove(referenceForm);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted ReferenceForm {Id}: {Name}", id, formName);
                TempData["SuccessMessage"] = $"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA£ xAAAAAAAAAAAAAAAA³a biAAAAAAAA¡AAAAAAAA»AAAAAAAAu mAAAAAAAA¡AAAAAAAAºAAAAAAAA«u \"{formName}\" thAAAAAAAAAAAAAAAA nh cAAAAAAAAAAAAAAAA´ng!";

                return RedirectToAction(nameof(Index), new { periodId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ReferenceForm {Id}", id);
                TempData["ErrorMessage"] = "LAAAAAAAA¡AAAAAAAA»AAAAAAAAi khi xAAAAAAAAAAAAAAAA³a biAAAAAAAA¡AAAAAAAA»AAAAAAAAu mAAAAAAAA¡AAAAAAAAºAAAAAAAA«u. Vui lAAAAAAAAAAAAAAAA²ng thAAAAAAAA¡AAAAAAAA»AAAAAAAA­ lAAAAAAAA¡AAAAAAAAºAAAAAAAA¡i.";
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
                    Text = p.Name + (p.IsActive ? " (AAAAAAAAAAAAAAAAang mAAAAAAAA¡AAAAAAAA»AAAAAAAA)" : "")
                })
                .ToListAsync();
        }

        #endregion
    }
}
