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

        public InitiativeController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            ILogger<InitiativeController> logger,
            IInitiativeService initiativeService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _initiativeService = initiativeService;
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
                    
                    // Auto-generate initiative code
                    var count = await _context.Initiatives.CountAsync() + 1;
                    initiative.InitiativeCode = $"SK-{DateTime.Now.Year}-{count:D4}";
                    
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

                    _context.Initiatives.Add(initiative);
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

                    // Handle file uploads
                    if (viewModel.UploadedFiles != null && viewModel.UploadedFiles.Count > 0)
                    {
                        try
                        {
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "initiatives");
                            
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            foreach (var file in viewModel.UploadedFiles)
                            {
                                if (file.Length > 0)
                                {
                                    try
                                    {
                                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                        using (var stream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await file.CopyToAsync(stream);
                                        }

                                        var initiativeFile = new InitiativeFile
                                        {
                                            FileName = file.FileName,
                                            FilePath = $"/uploads/initiatives/{uniqueFileName}",
                                            FileType = Path.GetExtension(file.FileName).ToLower(),
                                            FileSize = file.Length,
                                            UploadDate = DateTime.Now,
                                            InitiativeId = initiative.Id
                                        };

                                        _context.InitiativeFiles.Add(initiativeFile);
                                        _logger.LogInformation("File {FileName} uploaded cho sáng kiến {InitiativeId}", file.FileName, initiative.Id);
                                    }
                                    catch (Exception fileEx)
                                    {
                                        _logger.LogError(fileEx, "Lỗi khi lưu file {FileName} cho sáng kiến {InitiativeId}", file.FileName, initiative.Id);
                                    }
                                }
                            }

                            await _context.SaveChangesAsync();
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

                    // Handle file uploads
                    if (viewModel.UploadedFiles != null && viewModel.UploadedFiles.Count > 0)
                    {
                        try
                        {
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "initiatives");
                            
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            foreach (var file in viewModel.UploadedFiles)
                            {
                                if (file.Length > 0)
                                {
                                    try
                                    {
                                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                        using (var stream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await file.CopyToAsync(stream);
                                        }

                                        var initiativeFile = new InitiativeFile
                                        {
                                            FileName = file.FileName,
                                            FilePath = $"/uploads/initiatives/{uniqueFileName}",
                                            FileType = Path.GetExtension(file.FileName).ToLower(),
                                            FileSize = file.Length,
                                            UploadDate = DateTime.Now,
                                            InitiativeId = existingInitiative.Id
                                        };

                                        _context.InitiativeFiles.Add(initiativeFile);
                                        _logger.LogInformation("File {FileName} uploaded cho sáng kiến {InitiativeId}", file.FileName, existingInitiative.Id);
                                    }
                                    catch (Exception fileEx)
                                    {
                                        _logger.LogError(fileEx, "Lỗi khi lưu file {FileName} cho sáng kiến {InitiativeId}", file.FileName, existingInitiative.Id);
                                    }
                                }
                            }
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
    }
}

