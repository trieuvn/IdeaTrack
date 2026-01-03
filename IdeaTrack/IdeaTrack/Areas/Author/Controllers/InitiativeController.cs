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
                _logger.LogError(ex, "Lỗi khi tải chi tiết sáng kiến id {InitiativeId}", id);
                TempData["ErrorMessage"] = "Không thể tải chi tiết sáng kiến. Vui lòng thử lại sau.";
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
                _logger.LogError(ex, "Lỗi khi tải trang tạo sáng kiến");
                TempData["ErrorMessage"] = "Không thể tải form tạo sáng kiến. Vui lòng thử lại sau.";
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

                    _logger.LogInformation("Sáng kiến {InitiativeCode} được tạo thành công bởi user {UserId}", initiative.InitiativeCode, initiative.CreatorId);

                    // Handle file uploads using Service
                    if (viewModel.UploadedFiles != null && viewModel.UploadedFiles.Count > 0)
                    {
                        try 
                        {
                            await _fileService.UploadFilesAsync(viewModel.UploadedFiles, initiative.Id);
                        }
                        catch (Exception uploadEx)
                        {
                            _logger.LogError(uploadEx, "Lỗi khi xử lý file uploads cho sáng kiến {InitiativeId}", initiative.Id);
                            TempData["WarningMessage"] = "Sáng kiến đã được tạo nhưng có lỗi khi tải lên một số file.";
                        }
                    }

                    if (action == "Submit")
                    {
                        TempData["SuccessMessage"] = "Sáng kiến đã được nộp thành công và đang chờ duyệt!";
                        return RedirectToAction(nameof(History));
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Sáng kiến đã được lưu nháp!";
                        return RedirectToAction(nameof(Detail), new { id = initiative.Id });
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Lỗi database khi tạo sáng kiến");
                    ModelState.AddModelError("", "Lỗi khi lưu sáng kiến vào cơ sở dữ liệu. Vui lòng kiểm tra lại thông tin và thử lại.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi không xác định khi tạo sáng kiến");
                    ModelState.AddModelError("", $"Lỗi khi lưu sáng kiến: {ex.Message}");
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
                _logger.LogError(ex, "Lỗi khi tải lại dropdowns");
                ModelState.AddModelError("", "Lỗi khi tải dữ liệu form. Vui lòng tải lại trang.");
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
                var initiative = await _context.Initiatives.FindAsync(id);
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
                    ActivePeriodId = activePeriod?.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chỉnh sửa sáng kiến {InitiativeId}", id);
                TempData["ErrorMessage"] = "Không thể tải form chỉnh sửa. Vui lòng thử lại sau.";
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
                        TempData["ErrorMessage"] = "Không tìm thấy sáng kiến để cập nhật.";
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
                    if (viewModel.UploadedFiles != null && viewModel.UploadedFiles.Count > 0)
                    {
                        try
                        {
                             await _fileService.UploadFilesAsync(viewModel.UploadedFiles, existingInitiative.Id);
                        }
                        catch (Exception uploadEx)
                        {
                            _logger.LogError(uploadEx, "Lỗi khi xử lý file uploads cho sáng kiến {InitiativeId}", existingInitiative.Id);
                            TempData["WarningMessage"] = "Sáng kiến đã được cập nhật nhưng có lỗi khi tải lên một số file.";
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Sáng kiến {InitiativeId} được cập nhật thành công", id);

                    if (action == "Submit")
                    {
                        TempData["SuccessMessage"] = "Sáng kiến đã được nộp thành công và đang chờ duyệt!";
                        return RedirectToAction(nameof(History));
                    }

                    TempData["SuccessMessage"] = "Sáng kiến đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Detail), new { id = existingInitiative.Id });
                }
                catch (DbUpdateConcurrencyException concurrencyEx)
                {
                    _logger.LogError(concurrencyEx, "Lỗi concurrency khi cập nhật sáng kiến {InitiativeId}", id);
                    ModelState.AddModelError("", "Sáng kiến đã được thay đổi bởi người dùng khác. Vui lòng tải lại trang và thử lại.");
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Lỗi database khi cập nhật sáng kiến {InitiativeId}", id);
                    ModelState.AddModelError("", "Lỗi khi cập nhật sáng kiến vào cơ sở dữ liệu. Vui lòng kiểm tra lại thông tin và thử lại.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi không xác định khi cập nhật sáng kiến {InitiativeId}", id);
                    ModelState.AddModelError("", $"Lỗi khi lưu sáng kiến: {ex.Message}");
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
                _logger.LogError(ex, "Lỗi khi tải lại dropdowns");
                ModelState.AddModelError("", "Lỗi khi tải dữ liệu form. Vui lòng tải lại trang.");
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
                    TempData["ErrorMessage"] = "Không tìm thấy sáng kiến để nộp.";
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
                    
                    _logger.LogInformation("Sáng kiến {InitiativeId} được nộp thành công", id);
                    TempData["SuccessMessage"] = "Sáng kiến đã được nộp thành công và đang chờ duyệt!";
                }
                else
                {
                    _logger.LogWarning("Attempt to submit non-draft initiative {InitiativeId} with status {Status}", id, initiative.Status);
                    TempData["WarningMessage"] = "Sáng kiến này đã được nộp trước đó.";
                }

                return RedirectToAction(nameof(History));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Lỗi database khi nộp sáng kiến {InitiativeId}", id);
                TempData["ErrorMessage"] = "Lỗi khi nộp sáng kiến. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi nộp sáng kiến {InitiativeId}", id);
                TempData["ErrorMessage"] = "Lỗi khi nộp sáng kiến. Vui lòng thử lại sau.";
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
                    TempData["ErrorMessage"] = "Không tìm thấy sáng kiến để xóa.";
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
                
                _logger.LogInformation("Sáng kiến {InitiativeId} ({InitiativeCode}) đã được xóa", id, initiative.InitiativeCode);
                TempData["SuccessMessage"] = "Sáng kiến đã được xóa thành công.";

                return RedirectToAction(nameof(History));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Lỗi database khi xóa sáng kiến {InitiativeId} (có thể do ràng buộc khóa ngoại)", id);
                TempData["ErrorMessage"] = "Không thể xóa sáng kiến này vì có dữ liệu liên quan. Vui lòng liên hệ quản trị viên.";
                return RedirectToAction(nameof(Detail), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi xóa sáng kiến {InitiativeId}", id);
                TempData["ErrorMessage"] = "Lỗi khi xóa sáng kiến. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Detail), new { id });
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
                _logger.LogError(ex, "Lỗi khi tải trang lịch sử sáng kiến");
                TempData["ErrorMessage"] = "Không thể tải danh sách sáng kiến. Vui lòng thử lại sau.";
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
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để thực hiện chức năng này.";
                    return RedirectToAction(nameof(History));
                }

                var newDraft = await _initiativeService.CopyToDraftAsync(id, userId);

                if (newDraft == null)
                {
                    TempData["ErrorMessage"] = "Không thể tạo bản sao. Vui lòng thử lại sau.";
                    return RedirectToAction(nameof(Detail), new { id });
                }

                _logger.LogInformation("User {UserId} copied Initiative {OriginalId} to new draft {NewId}", 
                    userId, id, newDraft.Id);
                
                TempData["SuccessMessage"] = $"Đã tạo bản sao sáng kiến thành công! Mã mới: {newDraft.InitiativeCode}";
                return RedirectToAction(nameof(Edit), new { id = newDraft.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sao chép sáng kiến {InitiativeId}", id);
                TempData["ErrorMessage"] = "Lỗi khi tạo bản sao. Vui lòng thử lại sau.";
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
                    TempData["ErrorMessage"] = "Bạn không có quyền quản lý đồng tác giả của sáng kiến này.";
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
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách đồng tác giả.";
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
                    TempData["WarningMessage"] = "Người dùng này đã là đồng tác giả.";
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
                TempData["SuccessMessage"] = $"Đã thêm {user?.FullName} làm đồng tác giả!";

                return RedirectToAction(nameof(CoAuthors), new { id = initiativeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding co-author to initiative {Id}", initiativeId);
                TempData["ErrorMessage"] = "Lỗi khi thêm đồng tác giả.";
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
                    TempData["ErrorMessage"] = "Không thể xóa tác giả chính của sáng kiến.";
                    return RedirectToAction(nameof(CoAuthors), new { id = authorship.InitiativeId });
                }

                var authorName = authorship.Author.FullName;
                var initiativeId = authorship.InitiativeId;

                _context.InitiativeAuthorships.Remove(authorship);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed co-author {AuthorshipId} from initiative {InitiativeId}", authorshipId, initiativeId);
                TempData["SuccessMessage"] = $"Đã xóa {authorName} khỏi danh sách đồng tác giả!";

                return RedirectToAction(nameof(CoAuthors), new { id = initiativeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing co-author {AuthorshipId}", authorshipId);
                TempData["ErrorMessage"] = "Lỗi khi xóa đồng tác giả.";
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
                        StepName = "Tạo sáng kiến",
                        Description = "Sáng kiến được tạo và lưu nháp",
                        CompletedAt = initiative.CreatedAt,
                        IsCompleted = true,
                        IconClass = "fas fa-file-alt"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Nộp sáng kiến",
                        Description = "Gửi sáng kiến để chờ phê duyệt",
                        CompletedAt = initiative.SubmittedDate,
                        IsCompleted = initiative.SubmittedDate.HasValue,
                        IsCurrent = initiative.Status == InitiativeStatus.Pending,
                        IconClass = "fas fa-paper-plane"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Phê duyệt OST",
                        Description = "Phòng KHCN xem xét và phê duyệt sáng kiến",
                        IsCompleted = initiative.Status == InitiativeStatus.Faculty_Approved || 
                                      initiative.Status == InitiativeStatus.Evaluating ||
                                      initiative.Status == InitiativeStatus.Approved ||
                                      initiative.Status == InitiativeStatus.Rejected,
                        IsCurrent = initiative.Status == InitiativeStatus.Pending,
                        IconClass = "fas fa-check-circle"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Chấm điểm Hội đồng",
                        Description = $"Các thành viên hội đồng đánh giá (Đã có {initiative.Assignments?.Count(a => a.Status == AssignmentStatus.Completed)} đánh giá)",
                        IsCompleted = initiative.Status == InitiativeStatus.Approved || initiative.Status == InitiativeStatus.Rejected,
                        IsCurrent = initiative.Status == InitiativeStatus.Evaluating || initiative.Status == InitiativeStatus.Faculty_Approved,
                        IconClass = "fas fa-users"
                    },
                    new ProgressStepViewModel
                    {
                        StepName = "Kết quả cuối cùng",
                        Description = initiative.FinalResult != null 
                            ? $"Điểm: {initiative.FinalResult.AverageScore:F2} - {initiative.FinalResult.ChairmanDecision}"
                            : "Chờ quyết định của Chủ tịch Hội đồng",
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
                TempData["ErrorMessage"] = "Lỗi khi tải tiến trình sáng kiến.";
                return RedirectToAction(nameof(Detail), new { id });
            }
        }

        private string GetStatusDisplay(InitiativeStatus status)
        {
            return status switch
            {
                InitiativeStatus.Draft => "Bản nháp",
                InitiativeStatus.Pending => "Chờ duyệt",
                InitiativeStatus.Faculty_Approved => "Đã duyệt (Khoa)",
                InitiativeStatus.Evaluating => "Đang chấm điểm",
                InitiativeStatus.Re_Evaluating => "Đang chấm lại",
                InitiativeStatus.Revision_Required => "Yêu cầu chỉnh sửa",
                InitiativeStatus.Pending_Final => "Chờ quyết định",
                InitiativeStatus.Approved => "Đã phê duyệt",
                InitiativeStatus.Rejected => "Từ chối",
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
                    TempData["ErrorMessage"] = "Chỉ có thể nộp sáng kiến ở trạng thái nháp.";
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
                    TempData["ErrorMessage"] = "Không có đợt sáng kiến nào đang mở. Vui lòng liên hệ quản trị viên.";
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
                TempData["ErrorMessage"] = "Lỗi khi tải form nộp sáng kiến.";
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
                    TempData["ErrorMessage"] = "Sáng kiến này đã được nộp trước đó.";
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

                TempData["SuccessMessage"] = "Sáng kiến đã được nộp thành công và đang chờ duyệt!";
                return RedirectToAction(nameof(History));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting initiative {Id}", vm.InitiativeId);
                TempData["ErrorMessage"] = "Lỗi khi nộp sáng kiến. Vui lòng thử lại.";
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

