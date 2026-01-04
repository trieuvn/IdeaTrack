using IdeaTrack.Areas.Author.ViewModels;
using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.Author.Controllers
{
    [Area("Author")]
    [Authorize(Roles = "Author,Lecturer,Admin")]
    public class InitiativeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<InitiativeController> _logger;
        private readonly IInitiativeService _initiativeService;
        private readonly IFileService _fileService;

        public InitiativeController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            ILogger<InitiativeController> logger,
            IInitiativeService initiativeService,
            IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _initiativeService = initiativeService;
            _fileService = fileService;
        }

        // GET: /Author/Initiative/Detail/5
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Detail action called with null id");
                return NotFound();
            }

            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Category)
                    .Include(i => i.Period)
                        .ThenInclude(p => p != null ? p.AcademicYear : null)
                    .Include(i => i.Department)
                    .Include(i => i.Files)
                    .Include(i => i.Authorships)
                        .ThenInclude(a => a.Author)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (initiative == null)
                {
                    _logger.LogWarning("Initiative with id {InitiativeId} not found", id);
                    return NotFound();
                }

                var viewModel = new InitiativeDetailViewModel
                {
                    Initiative = initiative,
                    Files = initiative.Files.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi tai chi tiet sang kien id {InitiativeId}", id);
                TempData["ErrorMessage"] = "Khong the tai chi tiet sang kien. Vui long thu lai sau.";
                return RedirectToAction(nameof(History));
            }
        }

        // GET: /Author/Initiative/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Get active period's categories
                var activePeriod = await _context.InitiativePeriods
                    .Where(p => p.IsActive)
                    .FirstOrDefaultAsync();

                var categories = activePeriod != null
                    ? await _context.InitiativeCategories.Where(c => c.PeriodId == activePeriod.Id).ToListAsync()
                    : await _context.InitiativeCategories.ToListAsync();

                var viewModel = new InitiativeCreateViewModel
                {
                    Initiative = new Initiative(),
                    Categories = new SelectList(categories, "Id", "Name"),
                    Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name"),
                    ActivePeriodId = activePeriod?.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi tai trang tao sang kien");
                TempData["ErrorMessage"] = "Khong the tai form tao sang kien. Vui long thu lai sau.";
                return RedirectToAction(nameof(History));
            }
        }

        // POST: /Author/Initiative/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InitiativeCreateViewModel viewModel, string action)
        {
            // Remove navigation properties and auto-generated fields from validation
            ModelState.Remove("Initiative.Creator");
            ModelState.Remove("Initiative.Category");
            ModelState.Remove("Initiative.Period");
            ModelState.Remove("Initiative.Department");
            ModelState.Remove("Initiative.InitiativeCode");
            ModelState.Remove("Initiative.Status");
            ModelState.Remove("Initiative.CreatedAt");

            if (ModelState.IsValid)
            {
                try
                {
                    var initiative = viewModel.Initiative;
                    
                    // Assign to current user or default to first user
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                         initiative.CreatorId = currentUser.Id;
                    }
                    else
                    {
                         var firstUser = await _context.Users.FirstOrDefaultAsync();
                         initiative.CreatorId = firstUser?.Id ?? 1;
                    }

                    initiative.CreatedAt = DateTime.Now;

                    // Set Status based on Action
                    if (action == "Submit")
                    {
                        initiative.Status = InitiativeStatus.Pending;
                        initiative.SubmittedDate = DateTime.Now;
                        
                        // Get active period
                        var activePeriod = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
                        initiative.PeriodId = activePeriod?.Id;
                    }
                    else
                    {
                        initiative.Status = InitiativeStatus.Draft;
                    }

                    // Save first to get the Id
                    _context.Initiatives.Add(initiative);
                    await _context.SaveChangesAsync();

                    // Generate Code using proper Id to prevent race conditions
                    // Format: SK-{Year}-{Id} (e.g. SK-2025-0042)
                    initiative.InitiativeCode = $"SK-{DateTime.Now.Year}-{initiative.Id:D4}";
                    
                    // Update the code
                    _context.Update(initiative);
                    await _context.SaveChangesAsync();

                    // Add creator as primary author
                    var authorship = new InitiativeAuthorship
                    {
                        InitiativeId = initiative.Id,
                        AuthorId = initiative.CreatorId,
                        IsCreator = true,
                        JoinedAt = DateTime.Now
                    };
                    _context.InitiativeAuthorships.Add(authorship);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Initiative {InitiativeCode} created successfully by user {UserId}", initiative.InitiativeCode, initiative.CreatorId);

                    // Handle file uploads using Service
                    _logger.LogInformation("[FileUpload] Checking for files. ProjectFiles is null: {IsNull}, Count: {Count}", 
                        viewModel.ProjectFiles == null, viewModel.ProjectFiles?.Count ?? 0);
                    
                    if (viewModel.ProjectFiles != null && viewModel.ProjectFiles.Count > 0)
                    {
                        _logger.LogInformation("[FileUpload] Processing {Count} files for initiative {InitiativeId}", 
                            viewModel.ProjectFiles.Count, initiative.Id);
                        try 
                        {
                            var uploadedFiles = await _fileService.UploadFilesAsync(viewModel.ProjectFiles, initiative.Id);
                            _logger.LogInformation("[FileUpload] Successfully uploaded {Count} files", uploadedFiles.Count);
                        }
                        catch (Exception uploadEx)
                        {
                            _logger.LogError(uploadEx, "[FileUpload] Error processing file uploads for initiative {InitiativeId}", initiative.Id);
                            TempData["WarningMessage"] = "Initiative created but some files failed to upload.";
                        }
                    }
                    else
                    {
                        _logger.LogInformation("[FileUpload] No files to upload");
                    }

                    if (action == "Submit")
                    {
                        TempData["SuccessMessage"] = "Sang kien da duoc nop thanh cong va dang cho duyet!";
                        return RedirectToAction(nameof(History));
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Sang kien da duoc luu nhap!";
                        return RedirectToAction(nameof(Detail), new { id = initiative.Id });
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Error database khi tao sang kien");
                    ModelState.AddModelError("", "Error khi luu sang kien vao co so du lieu. Vui long kiem tra lai thong tin va thu lai.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error khong xac dinh khi tao sang kien");
                    ModelState.AddModelError("", $"Error khi luu sang kien: {ex.Message}");
                }
            }

            // If validation fails or error occurred, reload dropdowns
            try
            {
                var activePeriod = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
                var categories = activePeriod != null
                    ? await _context.InitiativeCategories.Where(c => c.PeriodId == activePeriod.Id).ToListAsync()
                    : await _context.InitiativeCategories.ToListAsync();

                viewModel.Categories = new SelectList(categories, "Id", "Name", viewModel.Initiative.CategoryId);
                viewModel.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", viewModel.Initiative.DepartmentId);
                viewModel.ActivePeriodId = activePeriod?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi tai lai dropdowns");
                ModelState.AddModelError("", "Error khi tai du lieu form. Vui long tai lai trang.");
            }

            return View(viewModel);
        }

        // GET: /Author/Initiative/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action called with null id");
                return NotFound();
            }

            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Files)
                    .FirstOrDefaultAsync(i => i.Id == id);
                    
                if (initiative == null)
                {
                    _logger.LogWarning("Initiative with id {InitiativeId} not found for editing", id);
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? 0;
                var isAuthorized = initiative.CreatorId == userId || 
                                 (initiative.Authorships != null && initiative.Authorships.Any(a => a.AuthorId == userId)) ||
                                 User.IsInRole("Admin");

                if (!isAuthorized)
                {
                    _logger.LogWarning("Unauthorized access attempt by user {UserId} to edit initiative {InitiativeId}", userId, id);
                    return Forbid();
                }

                var activePeriod = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
                var categories = activePeriod != null
                    ? await _context.InitiativeCategories.Where(c => c.PeriodId == activePeriod.Id).ToListAsync()
                    : await _context.InitiativeCategories.ToListAsync();

                var viewModel = new InitiativeCreateViewModel
                {
                    Initiative = initiative,
                    Categories = new SelectList(categories, "Id", "Name", initiative.CategoryId),
                    Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", initiative.DepartmentId),
                    ActivePeriodId = activePeriod?.Id,
                    ExistingFiles = initiative.Files?.ToList() ?? new List<InitiativeFile>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit page for initiative {InitiativeId}", id);
                TempData["ErrorMessage"] = "Cannot load edit form. Please try again later.";
                return RedirectToAction(nameof(History));
            }
        }

        // POST: /Author/Initiative/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InitiativeCreateViewModel viewModel, string action)
        {
            if (id != viewModel.Initiative.Id)
            {
                _logger.LogWarning("Initiative id mismatch: URL id {UrlId} vs Model id {ModelId}", id, viewModel.Initiative.Id);
                return NotFound();
            }

            // Remove navigation properties and auto-generated fields from validation
            ModelState.Remove("Initiative.Creator");
            ModelState.Remove("Initiative.Category");
            ModelState.Remove("Initiative.Period");
            ModelState.Remove("Initiative.Department");
            ModelState.Remove("Initiative.InitiativeCode");
            ModelState.Remove("Initiative.Status");
            ModelState.Remove("Initiative.CreatedAt");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInitiative = await _context.Initiatives.FindAsync(id);
                    if (existingInitiative == null)
                    {
                        _logger.LogWarning("Initiative with id {InitiativeId} not found for update", id);
                        TempData["ErrorMessage"] = "Khong tim thay sang kien de cap nhat.";
                        return RedirectToAction(nameof(History));
                    }

                    // Security Check
                    var currentUser = await _userManager.GetUserAsync(User);
                    var userId = currentUser?.Id ?? 0;
                    
                    // Need to load authorships to check permission if not creator
                    // For massive optimization, we could assume CreatorId check is enough for Edit POST as Co-authors should be loaded, 
                    // but safer to strictly check. Since we didn't Include Authorships in FindAsync, we need to verify.
                    // However, for update, usually checking Creator is the bare minimum, or we load it.
                    // Let's load it properly.
                    var authCheck = await _context.InitiativeAuthorships
                        .AnyAsync(a => a.InitiativeId == id && a.AuthorId == userId);
                    
                    var isAuthorized = existingInitiative.CreatorId == userId || authCheck || User.IsInRole("Admin");

                    if (!isAuthorized)
                    {
                         _logger.LogWarning("Unauthorized update attempt by user {UserId} to initiative {InitiativeId}", userId, id);
                         return Forbid();
                    }

                    // Update fields
                    existingInitiative.Title = viewModel.Initiative.Title;
                    existingInitiative.Description = viewModel.Initiative.Description;
                    existingInitiative.Budget = viewModel.Initiative.Budget;
                    existingInitiative.CategoryId = viewModel.Initiative.CategoryId;
                    existingInitiative.DepartmentId = viewModel.Initiative.DepartmentId;

                    if (action == "Submit")
                    {
                        existingInitiative.Status = InitiativeStatus.Pending;
                        existingInitiative.SubmittedDate = DateTime.Now;
                        
                        // Set period when submitting
                        var activePeriod = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
                        existingInitiative.PeriodId = activePeriod?.Id;
                    }

                    // Handle file uploads using Service
                    _logger.LogInformation("[FileUpload-Edit] Checking for files. ProjectFiles is null: {IsNull}, Count: {Count}", 
                        viewModel.ProjectFiles == null, viewModel.ProjectFiles?.Count ?? 0);
                        
                    if (viewModel.ProjectFiles != null && viewModel.ProjectFiles.Count > 0)
                    {
                        _logger.LogInformation("[FileUpload-Edit] Processing {Count} files for initiative {InitiativeId}", 
                            viewModel.ProjectFiles.Count, existingInitiative.Id);
                        try
                        {
                            var uploadedFiles = await _fileService.UploadFilesAsync(viewModel.ProjectFiles, existingInitiative.Id);
                            _logger.LogInformation("[FileUpload-Edit] Successfully uploaded {Count} files", uploadedFiles.Count);
                        }
                        catch (Exception uploadEx)
                        {
                            _logger.LogError(uploadEx, "[FileUpload-Edit] Error processing file uploads for initiative {InitiativeId}", existingInitiative.Id);
                            TempData["WarningMessage"] = "Initiative updated but some files failed to upload.";
                        }
                    }
                    else
                    {
                        _logger.LogInformation("[FileUpload-Edit] No new files to upload");
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Initiative {InitiativeId} updated successfully", id);

                    if (action == "Submit")
                    {
                        TempData["SuccessMessage"] = "Initiative submitted successfully and pending review!";
                        return RedirectToAction(nameof(History));
                    }

                    TempData["SuccessMessage"] = "Initiative updated successfully!";
                    return RedirectToAction(nameof(Detail), new { id = existingInitiative.Id });
                }
                catch (DbUpdateConcurrencyException concurrencyEx)
                {
                    _logger.LogError(concurrencyEx, "Error concurrency khi cap nhat sang kien {InitiativeId}", id);
                    ModelState.AddModelError("", "Sang kien da duoc thay doi boi nguoi dung khac. Vui long tai lai trang va thu lai.");
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Error database khi cap nhat sang kien {InitiativeId}", id);
                    ModelState.AddModelError("", "Error khi cap nhat sang kien vao co so du lieu. Vui long kiem tra lai thong tin va thu lai.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error khong xac dinh khi cap nhat sang kien {InitiativeId}", id);
                    ModelState.AddModelError("", $"Error khi luu sang kien: {ex.Message}");
                }
            }

            // If validation fails or error occurred, reload dropdowns
            try
            {
                var activePeriod = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
                var categories = activePeriod != null
                    ? await _context.InitiativeCategories.Where(c => c.PeriodId == activePeriod.Id).ToListAsync()
                    : await _context.InitiativeCategories.ToListAsync();

                viewModel.Categories = new SelectList(categories, "Id", "Name", viewModel.Initiative.CategoryId);
                viewModel.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", viewModel.Initiative.DepartmentId);
                viewModel.ActivePeriodId = activePeriod?.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi tai lai dropdowns");
                ModelState.AddModelError("", "Error khi tai du lieu form. Vui long tai lai trang.");
            }

            return View(viewModel);
        }

        // POST: /Author/Initiative/SubmitInitiative/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitInitiative(int id)
        {
            try
            {
                var initiative = await _context.Initiatives.FindAsync(id);
                if (initiative == null)
                {
                    _logger.LogWarning("Initiative with id {InitiativeId} not found for submission", id);
                    TempData["ErrorMessage"] = "Khong tim thay sang kien de nop.";
                    return RedirectToAction(nameof(History));
                }

                // Security Check
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? 0;
                // Check authorization (Creator or Co-author)
                // Note: FindAsync doesn't include navigation props, need explicit check
                var isAuthorized = initiative.CreatorId == userId || 
                                 await _context.InitiativeAuthorships.AnyAsync(a => a.InitiativeId == id && a.AuthorId == userId) ||
                                 User.IsInRole("Admin");

                if (!isAuthorized)
                {
                     return Forbid();
                }

                if (initiative.Status == InitiativeStatus.Draft)
                {
                    initiative.Status = InitiativeStatus.Pending;
                    initiative.SubmittedDate = DateTime.Now;
                    
                    // Set period when submitting
                    var activePeriod = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
                    initiative.PeriodId = activePeriod?.Id;
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Sang kien {InitiativeId} duoc nop thanh cong", id);
                    TempData["SuccessMessage"] = "Sang kien da duoc nop thanh cong va dang cho duyet!";
                }
                else
                {
                    _logger.LogWarning("Attempt to submit non-draft initiative {InitiativeId} with status {Status}", id, initiative.Status);
                    TempData["WarningMessage"] = "Sang kien nay da duoc nop truoc do.";
                }

                return RedirectToAction(nameof(History));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error database khi nop sang kien {InitiativeId}", id);
                TempData["ErrorMessage"] = "Error khi nop sang kien. Vui long thu lai sau.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khong xac dinh khi nop sang kien {InitiativeId}", id);
                TempData["ErrorMessage"] = "Error khi nop sang kien. Vui long thu lai sau.";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        // POST: /Author/Initiative/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var initiative = await _context.Initiatives.FindAsync(id);
                if (initiative == null)
                {
                    _logger.LogWarning("Initiative with id {InitiativeId} not found for deletion", id);
                    TempData["ErrorMessage"] = "Khong tim thay sang kien de xoa.";
                    return RedirectToAction(nameof(History));
                }

                // Security Check
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? 0;
                var isAuthorized = initiative.CreatorId == userId || User.IsInRole("Admin"); // Only Creator or Admin can delete

                if (!isAuthorized)
                {
                     _logger.LogWarning("Unauthorized delete attempt by user {UserId} on initiative {InitiativeId}", userId, id);
                     return Forbid();
                }

                _context.Initiatives.Remove(initiative);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Sang kien {InitiativeId} ({InitiativeCode}) da duoc xoa", id, initiative.InitiativeCode);
                TempData["SuccessMessage"] = "Sang kien da duoc xoa thanh cong.";

                return RedirectToAction(nameof(History));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error database khi xoa sang kien {InitiativeId} (co the do rang buoc khoa ngoai)", id);
                TempData["ErrorMessage"] = "Khong the xoa sang kien nay vi co du lieu lien quan. Vui long lien he quan tri vien.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting initiative {InitiativeId}", id);
                TempData["ErrorMessage"] = "Error deleting initiative. Please try again later.";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        // POST: /Author/Initiative/DeleteFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int fileId, int initiativeId)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? 0;

                // Verify initiative ownership
                var initiative = await _context.Initiatives.FindAsync(initiativeId);
                if (initiative == null || (initiative.CreatorId != userId && !User.IsInRole("Admin")))
                {
                    return Forbid();
                }

                // Delete file from storage and database
                var success = await _fileService.DeleteFileAsync(fileId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "File deleted successfully.";
                    _logger.LogInformation("File {FileId} deleted from initiative {InitiativeId} by user {UserId}", fileId, initiativeId, userId);
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not delete file.";
                }

                return RedirectToAction(nameof(Edit), new { id = initiativeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                TempData["ErrorMessage"] = "Error deleting file. Please try again.";
                return RedirectToAction(nameof(Edit), new { id = initiativeId });
            }
        }

        // GET: /Author/Initiative/History
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> History()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? 0;

                // Get initiatives where user is creator or co-author
                var initiatives = await _context.Initiatives
                    .Include(i => i.Category)
                    .Include(i => i.Authorships)
                    .Where(i => i.CreatorId == userId || i.Authorships.Any(a => a.AuthorId == userId))
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                return View(initiatives);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi tai trang lich su sang kien");
                TempData["ErrorMessage"] = "Khong the tai danh sach sang kien. Vui long thu lai sau.";
                return View(new List<Initiative>());
            }
        }

        // POST: /Author/Initiative/CopyToDraft/5
        // Copy a rejected/completed initiative to a new draft for resubmission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopyToDraft(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? 0;

                if (userId == 0)
                {
                    TempData["ErrorMessage"] = "Vui long dang nhap de thuc hien chuc nang nay.";
                    return RedirectToAction(nameof(History));
                }

                var newDraft = await _initiativeService.CopyToDraftAsync(id, userId);

                if (newDraft == null)
                {
                    TempData["ErrorMessage"] = "Khong the tao ban sao. Vui long thu lai sau.";
                    return RedirectToAction(nameof(Detail), new { id });
                }

                _logger.LogInformation("User {UserId} copied Initiative {OriginalId} to new draft {NewId}", 
                    userId, id, newDraft.Id);
                
                TempData["SuccessMessage"] = $"Da tao ban sao sang kien thanh cong! Ma moi: {newDraft.InitiativeCode}";
                return RedirectToAction(nameof(Edit), new { id = newDraft.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error khi sao chep sang kien {InitiativeId}", id);
                TempData["ErrorMessage"] = "Error khi tao ban sao. Vui long thu lai sau.";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        // ============ PHASE 4: CO-AUTHOR MANAGEMENT ============

        // GET: /Author/Initiative/CoAuthors/5
        public async Task<IActionResult> CoAuthors(int id)
        {
            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Authorships)
                        .ThenInclude(a => a.Author)
                            .ThenInclude(u => u.Department)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (initiative == null)
                {
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var isAuthorOrCreator = initiative.CreatorId == currentUser?.Id ||
                    initiative.Authorships.Any(a => a.AuthorId == currentUser?.Id);

                if (!isAuthorOrCreator)
                {
                    TempData["ErrorMessage"] = "Ban khong co quyen quan ly dong tac gia cua sang kien nay.";
                    return RedirectToAction(nameof(History));
                }

                // Get existing author IDs
                var existingAuthorIds = initiative.Authorships.Select(a => a.AuthorId).ToList();

                // Get available users (lecturers/authors not already co-authors)
                var availableUsers = await _context.Users
                    .Include(u => u.Department)
                    .Where(u => !existingAuthorIds.Contains(u.Id))
                    .OrderBy(u => u.FullName)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FullName} ({u.Email}){(u.Department != null ? " - " + u.Department.Name : "")}"
                    })
                    .ToListAsync();

                var vm = new CoAuthorManageViewModel
                {
                    InitiativeId = initiative.Id,
                    InitiativeTitle = initiative.Title,
                    InitiativeCode = initiative.InitiativeCode,
                    CanEdit = initiative.Status == InitiativeStatus.Draft || initiative.Status == InitiativeStatus.Pending,
                    CoAuthors = initiative.Authorships.Select(a => new CoAuthorViewModel
                    {
                        AuthorshipId = a.Id,
                        AuthorId = a.AuthorId,
                        AuthorName = a.Author.FullName,
                        Email = a.Author.Email ?? "",
                        Department = a.Author.Department?.Name,
                        IsCreator = a.IsCreator,
                        JoinedAt = a.JoinedAt
                    }).ToList(),
                    AvailableUsers = availableUsers
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading co-authors for initiative {Id}", id);
                TempData["ErrorMessage"] = "Error khi tai danh sach dong tac gia.";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        // POST: /Author/Initiative/AddCoAuthor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCoAuthor(int initiativeId, int newCoAuthorId)
        {
            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Authorships)
                    .FirstOrDefaultAsync(i => i.Id == initiativeId);

                if (initiative == null)
                {
                    return NotFound();
                }

                // Check if already a co-author
                if (initiative.Authorships.Any(a => a.AuthorId == newCoAuthorId))
                {
                    TempData["WarningMessage"] = "Nguoi dung nay da la dong tac gia.";
                    return RedirectToAction(nameof(CoAuthors), new { id = initiativeId });
                }

                // Add new co-author
                var authorship = new InitiativeAuthorship
                {
                    InitiativeId = initiativeId,
                    AuthorId = newCoAuthorId,
                    IsCreator = false,
                    JoinedAt = DateTime.Now
                };

                _context.InitiativeAuthorships.Add(authorship);
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(newCoAuthorId);
                _logger.LogInformation("Added co-author {UserId} to initiative {InitiativeId}", newCoAuthorId, initiativeId);
                TempData["SuccessMessage"] = $"Da them {user?.FullName} lam dong tac gia!";

                return RedirectToAction(nameof(CoAuthors), new { id = initiativeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding co-author to initiative {Id}", initiativeId);
                TempData["ErrorMessage"] = "Error khi them dong tac gia.";
                return RedirectToAction(nameof(CoAuthors), new { id = initiativeId });
            }
        }

        // POST: /Author/Initiative/RemoveCoAuthor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoAuthor(int authorshipId)
        {
            try
            {
                var authorship = await _context.InitiativeAuthorships
                    .Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.Id == authorshipId);

                if (authorship == null)
                {
                    return NotFound();
                }

                if (authorship.IsCreator)
                {
                    TempData["ErrorMessage"] = "Khong the xoa tac gia chinh cua sang kien.";
                    return RedirectToAction(nameof(CoAuthors), new { id = authorship.InitiativeId });
                }

                var authorName = authorship.Author.FullName;
                var initiativeId = authorship.InitiativeId;

                _context.InitiativeAuthorships.Remove(authorship);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed co-author {AuthorshipId} from initiative {InitiativeId}", authorshipId, initiativeId);
                TempData["SuccessMessage"] = $"Da xoa {authorName} khoi danh sach dong tac gia!";

                return RedirectToAction(nameof(CoAuthors), new { id = initiativeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing co-author {AuthorshipId}", authorshipId);
                TempData["ErrorMessage"] = "Error khi xoa dong tac gia.";
                return RedirectToAction(nameof(History));
            }
        }

        // ============ PHASE 4: PROGRESS TIMELINE ============

        // GET: /Author/Initiative/Progress/5
        public async Task<IActionResult> Progress(int id)
        {
            try
            {
                var initiative = await _context.Initiatives
                    .Include(i => i.Assignments)
                    .Include(i => i.FinalResult)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (initiative == null)
                {
                    return NotFound();
                }

                var steps = new List<ProgressStepViewModel>
                {
                    new ProgressStepViewModel
                    {
                        StepName = "Tao sang kien",
                        Description = "Sang kien duoc tao va luu nhap",
                        CompletedAt = initiative.CreatedAt,
                        IsCompleted = true,
                        IconClass = "fas fa-file-alt"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Nop sang kien",
                        Description = "Gui sang kien de cho phe duyet",
                        CompletedAt = initiative.SubmittedDate,
                        IsCompleted = initiative.SubmittedDate.HasValue,
                        IsCurrent = initiative.Status == InitiativeStatus.Pending,
                        IconClass = "fas fa-paper-plane"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Approve OST",
                        Description = "Phong KHCN xem xet va phe duyet sang kien",
                        IsCompleted = initiative.Status == InitiativeStatus.Faculty_Approved || 
                                      initiative.Status == InitiativeStatus.Evaluating ||
                                      initiative.Status == InitiativeStatus.Approved ||
                                      initiative.Status == InitiativeStatus.Rejected,
                        IsCurrent = initiative.Status == InitiativeStatus.Pending,
                        IconClass = "fas fa-check-circle"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Cham diem Hoi dong",
                        Description = $"Cac thanh vien hoi dong danh gia (Da co {initiative.Assignments?.Count(a => a.Status == AssignmentStatus.Completed)} danh gia)",
                        IsCompleted = initiative.Status == InitiativeStatus.Approved || initiative.Status == InitiativeStatus.Rejected,
                        IsCurrent = initiative.Status == InitiativeStatus.Evaluating || initiative.Status == InitiativeStatus.Faculty_Approved,
                        IconClass = "fas fa-users"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Ket qua cuoi cung",
                        Description = initiative.FinalResult != null 
                            ? $"Diem: {initiative.FinalResult.AverageScore:F2} - {initiative.FinalResult.ChairmanDecision}"
                            : "Cho quyet dinh cua Chu tich Hoi dong",
                        IsCompleted = initiative.Status == InitiativeStatus.Approved || initiative.Status == InitiativeStatus.Rejected,
                        IsCurrent = false,
                        IconClass = initiative.Status == InitiativeStatus.Approved ? "fas fa-trophy" : 
                                   initiative.Status == InitiativeStatus.Rejected ? "fas fa-times-circle" : "fas fa-hourglass-half"
                    }
                };

                var vm = new ProgressTimelineViewModel
                {
                    InitiativeId = initiative.Id,
                    InitiativeTitle = initiative.Title,
                    InitiativeCode = initiative.InitiativeCode,
                    CurrentStatus = GetStatusDisplay(initiative.Status),
                    Steps = steps
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading progress for initiative {Id}", id);
                TempData["ErrorMessage"] = "Error khi tai tien trinh sang kien.";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        private string GetStatusDisplay(InitiativeStatus status)
        {
            return status switch
            {
                InitiativeStatus.Draft => "Ban nhap",
                InitiativeStatus.Pending => "Cho duyet",
                InitiativeStatus.Faculty_Approved => "Da duyet (Khoa)",
                InitiativeStatus.Evaluating => "Dang cham diem",
                InitiativeStatus.Re_Evaluating => "Dang cham lai",
                InitiativeStatus.Revision_Required => "Request Revision",
                InitiativeStatus.Pending_Final => "Cho quyet dinh",
                InitiativeStatus.Approved => "Da phe duyet",
                InitiativeStatus.Rejected => "Reject",
                _ => status.ToString()
            };
        }

        // ============ PHASE 4: SUBMIT WITH PERIOD/CATEGORY SELECTION ============

        // GET: /Author/Initiative/SubmitWithSelection/5
        public async Task<IActionResult> SubmitWithSelection(int id)
        {
            try
            {
                var initiative = await _context.Initiatives.FindAsync(id);
                if (initiative == null)
                {
                    return NotFound();
                }

                if (initiative.Status != InitiativeStatus.Draft)
                {
                    TempData["ErrorMessage"] = "Chi co the nop sang kien o trang thai nhap.";
                    return RedirectToAction(nameof(Detail), new { id });
                }

                // Get active periods
                var periods = await _context.InitiativePeriods
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync();

                if (!periods.Any())
                {
                    TempData["ErrorMessage"] = "Khong co dot sang kien nao dang mo. Vui long lien he quan tri vien.";
                    return RedirectToAction(nameof(Detail), new { id });
                }

                var vm = new SubmitInitiativeViewModel
                {
                    InitiativeId = initiative.Id,
                    InitiativeTitle = initiative.Title,
                    InitiativeCode = initiative.InitiativeCode,
                    CategoryId = initiative.CategoryId,
                    Periods = periods,
                    Categories = new List<SelectListItem>() // Will be loaded via AJAX
                };

                // If there's only one period, pre-load its categories
                if (periods.Count == 1)
                {
                    var periodId = int.Parse(periods.First().Value);
                    vm.PeriodId = periodId;
                    vm.Categories = await GetCategoriesForPeriod(periodId);
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading submit form for initiative {Id}", id);
                TempData["ErrorMessage"] = "Error khi tai form nop sang kien.";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        // POST: /Author/Initiative/SubmitWithSelection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWithSelection(SubmitInitiativeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Periods = await _context.InitiativePeriods
                    .Where(p => p.IsActive)
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                    .ToListAsync();
                vm.Categories = await GetCategoriesForPeriod(vm.PeriodId);
                return View(vm);
            }

            try
            {
                var initiative = await _context.Initiatives.FindAsync(vm.InitiativeId);
                if (initiative == null)
                {
                    return NotFound();
                }

                if (initiative.Status != InitiativeStatus.Draft)
                {
                    TempData["ErrorMessage"] = "Sang kien nay da duoc nop truoc do.";
                    return RedirectToAction(nameof(History));
                }

                // Update initiative with selected period and category
                initiative.PeriodId = vm.PeriodId;
                initiative.CategoryId = vm.CategoryId;
                initiative.Status = InitiativeStatus.Pending;
                initiative.SubmittedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Initiative {Id} submitted with Period {PeriodId} and Category {CategoryId}",
                    initiative.Id, vm.PeriodId, vm.CategoryId);

                TempData["SuccessMessage"] = "Sang kien da duoc nop thanh cong va dang cho duyet!";
                return RedirectToAction(nameof(History));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting initiative {Id}", vm.InitiativeId);
                TempData["ErrorMessage"] = "Error khi nop sang kien. Vui long thu lai.";
                return RedirectToAction(nameof(Detail), new { id = vm.InitiativeId });
            }
        }

        // API: Get categories for a specific period (for AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCategories(int periodId)
        {
            var categories = await GetCategoriesForPeriod(periodId);
            return Json(categories.Select(c => new { value = c.Value, text = c.Text }));
        }

        private async Task<List<SelectListItem>> GetCategoriesForPeriod(int periodId)
        {
            return await _context.InitiativeCategories
                .Where(c => c.PeriodId == periodId)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
    }
}

