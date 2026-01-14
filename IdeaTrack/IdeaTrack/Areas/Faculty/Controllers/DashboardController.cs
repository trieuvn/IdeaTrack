using IdeaTrack.Areas.Faculty.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace IdeaTrack.Areas.Faculty.Controllers
{
    [Area("Faculty")]
    [Authorize(Roles = "FacultyLeader,Faculty_Admin,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentUserDepartmentId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.DepartmentId;
        }

        // ==========================================
        // 1. DASHBOARD LIST (INDEX)
        // ==========================================
        // ==========================================
        // 1. DASHBOARD LIST (INDEX)
        // ==========================================
        public async Task<IActionResult> Index(string searchString, string statusFilter, int pageNumber = 1)
        {
            var deptId = await GetCurrentUserDepartmentId();
            if (deptId == null) return View("Error", new { message = "User not assigned to a department" });

            int pageSize = 5;

            // 1. Base Query - Everything EXCEPT Draft for Faculty View
            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Category)
                .Include(i => i.Department)
                .Include(i => i.Authorships).ThenInclude(a => a.Author)
                .Where(i => i.DepartmentId == deptId && i.Status != InitiativeStatus.Draft)
                .AsQueryable();

            // 2. Apply Search
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Title.Contains(searchString) || s.InitiativeCode.Contains(searchString) || s.Creator.FullName.Contains(searchString));
            }

            // 3. Apply Status Filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Evaluation")
                {
                    query = query.Where(s => s.Status == InitiativeStatus.Evaluating || s.Status == InitiativeStatus.Re_Evaluating);
                }
                else if (Enum.TryParse(typeof(InitiativeStatus), statusFilter, out var status))
                {
                    query = query.Where(s => s.Status == (InitiativeStatus)status);
                }
            }

            // 4. Sorting
            query = query.OrderByDescending(i => i.SubmittedDate ?? i.CreatedAt);

            // 5. Pagination
            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var initiatives = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 6. Dynamic Statistics (Count directly from DB, restricted to Dept, Excluding Draft)
            // Note: We use the base condition (Dept + No Draft) for all counts
            var baseStatsQuery = _context.Initiatives
                .Where(i => i.DepartmentId == deptId && i.Status != InitiativeStatus.Draft);
            
            var viewModel = new FacultyDashboardVM
            {
                PendingCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Pending),
                FacultyApprovedCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Faculty_Approved),
                EvaluationCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Evaluating || i.Status == InitiativeStatus.Re_Evaluating),
                PendingFinalCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Pending_Final),
                ApprovedCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Approved),
                RejectedCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Rejected),
                RevisionRequiredCount = await baseStatsQuery.CountAsync(i => i.Status == InitiativeStatus.Revision_Required),
                
                TotalInitiatives = await baseStatsQuery.CountAsync(), 
                Initiatives = initiatives.Select(i => new FacultyInitiativeItem
                {
                    Id = i.Id,
                    Title = i.Title,
                    InitiativeCode = i.InitiativeCode,
                    ProposerName = i.Authorships?.FirstOrDefault(a => a.IsCreator)?.Author?.FullName ?? i.Creator?.FullName ?? "Unknown",
                    MemberCount = i.Authorships?.Count ?? 1,
                    Category = i.Category,
                    Status = i.Status,
                    SubmittedDate = i.SubmittedDate ?? i.CreatedAt
                }).ToList()
            };

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;

            // Pass statuses list for dropdown (handled in View, but good to know)

            return View(viewModel);
        }

        // ==========================================
        // 2. DETAILS PAGE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var deptId = await GetCurrentUserDepartmentId();

            var initiative = await _context.Initiatives
                .Include(i => i.Creator).ThenInclude(u => u.Department)
                .Include(i => i.Category)
                .Include(i => i.Files)
                .Include(i => i.RevisionRequests)
                .Include(i => i.Authorships).ThenInclude(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (initiative == null) return NotFound();

            // Security Check
            if (initiative.DepartmentId != deptId)
            {
                return Forbid(); // Or RedirectToAction("Index") with error
            }

            // Get primary author (creator) and all authors
            var primaryAuthor = initiative.Authorships?.FirstOrDefault(a => a.IsCreator)?.Author?.FullName 
                                ?? initiative.Creator?.FullName ?? "Unknown";
            var allAuthors = initiative.Authorships?
                .OrderByDescending(a => a.IsCreator)  // Creator first
                .Select(a => a.Author?.FullName ?? "Unknown")
                .ToList() ?? new List<string> { primaryAuthor };

            var viewModel = new FacultyInitiativeDetailVM
            {
                Id = initiative.Id,
                Title = initiative.Title,
                InitiativeCode = initiative.InitiativeCode,
                Description = initiative.Description ?? "",
                ProposerName = primaryAuthor,
                Authors = allAuthors,
                Budget = initiative.Budget,
                SubmittedDate = initiative.SubmittedDate ?? DateTime.MinValue,
                Status = initiative.Status,
                Category = initiative.Category,
                Files = initiative.Files?.Select(f => new InitiativeFileVM 
                { 
                    FileName = f.FileName, 
                    FileUrl = f.FilePath 
                }).ToList()
            };

            return View(viewModel);
        }

        // ==========================================
        // 3. REVIEW PAGE (Tracking Revisions)
        // ==========================================
        public async Task<IActionResult> Review(int pageNumber = 1, string filterType = "All")
        {
            var deptId = await GetCurrentUserDepartmentId();
            if (deptId == null) return RedirectToAction("Index");

            int pageSize = 5;

            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.RevisionRequests)
                .Where(i => i.DepartmentId == deptId) // Strict Filter
                .AsQueryable();

            switch (filterType)
            {
                case "Editing":
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required);
                    break;
                case "Resubmitted":
                    query = query.Where(i => i.Status == InitiativeStatus.Pending);
                    break;
                default: // "All"
                    query = query.Where(i => i.Status == InitiativeStatus.Revision_Required || i.Status == InitiativeStatus.Pending);
                    break;
            }

            query = query.OrderByDescending(i => i.SubmittedDate);

            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var listData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Stats for this view - restricted to department
            var allData = _context.Initiatives.Where(i => i.DepartmentId == deptId).AsQueryable();
            ViewBag.CountEditing = await allData.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);
            ViewBag.CountResubmitted = await allData.CountAsync(i => i.Status == InitiativeStatus.Pending);

            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.CountCompleted = await allData.CountAsync(i => i.Status == InitiativeStatus.Approved && i.FinalResult != null && i.FinalResult.DecisionDate >= startOfMonth);

            ViewBag.DropdownList = await allData
                .Where(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Faculty_Approved)
                .Select(i => new { i.Id, i.InitiativeCode, i.Title, LecturerName = i.Creator.FullName })
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.FilterType = filterType;

            return View(listData);
        }

        // ==========================================
        // 4. REQUEST REVISION (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRevision(int InitiativeId, string Comments)
        {
            var deptId = await GetCurrentUserDepartmentId();
            var initiative = await _context.Initiatives.FindAsync(InitiativeId);
            
            if (initiative == null) return NotFound();
            if (initiative.DepartmentId != deptId) return Forbid();

            initiative.Status = InitiativeStatus.Revision_Required;
            
            if (!string.IsNullOrEmpty(Comments))
            {
                var revisionRequest = new RevisionRequest
                {
                    InitiativeId = InitiativeId,
                    RequestContent = Comments,
                    RequestedDate = DateTime.Now,
                    IsResolved = false,
                    Status = "Open",
                    RequesterId = int.Parse(_userManager.GetUserId(User))
                };
                _context.RevisionRequests.Add(revisionRequest);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Revision request sent to the author!";
            return RedirectToAction(nameof(Details), new { id = InitiativeId });
        }

        // ==========================================
        // 5. ACCEPT INITIATIVE (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int InitiativeId, string Comments)
        {
            var deptId = await GetCurrentUserDepartmentId();
            var initiative = await _context.Initiatives.FindAsync(InitiativeId);
            
            if (initiative == null) return NotFound();
            if (initiative.DepartmentId != deptId) return Forbid();

            initiative.Status = InitiativeStatus.Faculty_Approved;
            // You can log comments if needed

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Initiative accepted and forwarded to R&D Department!";
            return RedirectToAction(nameof(Details), new { id = InitiativeId });
        }

        // ==========================================
        // 6. REJECT INITIATIVE (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int InitiativeId, string Comments)
        {
            var deptId = await GetCurrentUserDepartmentId();
            var initiative = await _context.Initiatives.FindAsync(InitiativeId);

            if (initiative == null) return NotFound();
            if (initiative.DepartmentId != deptId) return Forbid();

            initiative.Status = InitiativeStatus.Rejected;
            // You can log comments if needed

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Initiative has been rejected.";
            return RedirectToAction(nameof(Details), new { id = InitiativeId });
        }

        // ==========================================
        // 7. STATISTICS PAGE (PROGRESS)
        // ==========================================
        // ==========================================
        // 7. STATISTICS PAGE (PROGRESS)
        // ==========================================
        public async Task<IActionResult> Progress(string searchString, int? yearId, int? periodId, int? categoryId)
        {
            var deptId = await GetCurrentUserDepartmentId();
            if (deptId == null) return RedirectToAction("Index");

            // Base Query: Dept Only + Exclude Draft
            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Category)
                    .ThenInclude(c => c.Period)
                        .ThenInclude(p => p.AcademicYear)
                .Where(i => i.DepartmentId == deptId && i.Status != InitiativeStatus.Draft)
                .AsQueryable();

            // 1. Search
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(i => i.Creator.FullName.Contains(searchString)
                                      || i.Title.Contains(searchString)
                                      || i.InitiativeCode.Contains(searchString));
            }

            // 2. Filters
            if (yearId.HasValue)
            {
                query = query.Where(i => i.Category.Period.AcademicYearId == yearId.Value);
            }
            if (periodId.HasValue)
            {
                query = query.Where(i => i.Category.PeriodId == periodId.Value);
            }
            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }

            var initiatives = await query
                .OrderByDescending(i => i.CreatedAt)
                .Take(50) // Limit to 50 for performance on chart page
                .ToListAsync();

            ViewBag.CurrentSearch = searchString;

            // Stats restricted to Dept + Filtered Data (Wait, usually charts show aggregate of filtered data)
            // But per request "Đảm bảo khi chọn lọc, danh sách sáng kiến bên dưới sẽ cập nhật chính xác theo điều kiện."
            // Implicitly charts should probably update too? The requirement says "Chart data..." well it says "danh sách sáng kiến bên dưới".
            // However existing code recalculates PieData from `allData`. 
            // If I filter by "Year 2024", showing pie chart for "All Time" is confusing.
            // I will apply filters to the STATS too.
            
            var statsQuery = _context.Initiatives
                .Include(i => i.Category).ThenInclude(c => c.Period)
                .Where(i => i.DepartmentId == deptId && i.Status != InitiativeStatus.Draft)
                .AsQueryable();

            if (yearId.HasValue) statsQuery = statsQuery.Where(i => i.Category.Period.AcademicYearId == yearId.Value);
            if (periodId.HasValue) statsQuery = statsQuery.Where(i => i.Category.PeriodId == periodId.Value);
            if (categoryId.HasValue) statsQuery = statsQuery.Where(i => i.CategoryId == categoryId.Value);
            // Search string usually doesn't affect high-level stats/charts in dashboards, but filters DO.
            
            int countPending = await statsQuery.CountAsync(i => i.Status == InitiativeStatus.Pending);
            int countApproved = await statsQuery.CountAsync(i => i.Status == InitiativeStatus.Faculty_Approved); // Just forwarded? Or Approved final?
            // Existing code: countApproved = Faculty_Approved OR Approved.
            // New logic: "Evaluation" group.
            int countForwarded = await statsQuery.CountAsync(i => i.Status == InitiativeStatus.Faculty_Approved);
             // Pie chart usually shows workflow status. Current view has "Pending", "Forwarded", "Revision".
             // I will stick to existing pie chart categories or update them?
             // User said: "Logic Trạng thái (Status): ... gộp hai trạng thái evaluating và re_evaluating ... Evaluation"
             // I should probably update the Pie Chart to match the new groups? 
             // "Submissions per Day" (Trend) and "Status Distribution" (Pie)
             // I'll keep simple: Pending, Faculty Approved, Revision, Evaluation, Final Approved/Rejected
             // But existing view only had 3 slices. I will expand it to match Dashboard groups?
             // Or just use the counts I requested in DashboardVM?
             // Let's count them all for flexibility.
             
            int countEvaluation = await statsQuery.CountAsync(i => i.Status == InitiativeStatus.Evaluating || i.Status == InitiativeStatus.Re_Evaluating);
            int countRevision = await statsQuery.CountAsync(i => i.Status == InitiativeStatus.Revision_Required);
            
            // For the Pie Chart, let's show: Pending, Approved (Faculty), Evaluation, Revision
            ViewBag.PieData = new List<int> { countPending, countForwarded, countEvaluation, countRevision };
            ViewBag.PieLabels = new List<string> { "Pending Review", "Forwarded (R&D)", "Under Evaluation", "Revision Required" };

            // Trend Data
            var today = DateTime.Today;
            var trendData = new List<int>();
            var trendLabels = new List<string>();

            for (int i = 6; i >= 0; i--) // Last 7 days
            {
                var date = today.AddDays(-i);
                int count = await statsQuery.CountAsync(x => x.CreatedAt.Date == date);
                trendData.Add(count);
                trendLabels.Add(date.ToString("dd/MM"));
            }

            ViewBag.TrendData = trendData;
            ViewBag.TrendLabels = trendLabels;

            // Populate Dropdowns
            var years = await _context.AcademicYears.OrderByDescending(y => y.Name).ToListAsync();
            
            var periodQ = _context.InitiativePeriods.AsQueryable();
            if (yearId.HasValue) periodQ = periodQ.Where(p => p.AcademicYearId == yearId.Value);
            var periods = await periodQ.OrderByDescending(p => p.StartDate).ToListAsync();

            var catQ = _context.InitiativeCategories.AsQueryable();
            if (periodId.HasValue) catQ = catQ.Where(c => c.PeriodId == periodId.Value);
            else if (yearId.HasValue) catQ = catQ.Where(c => c.Period.AcademicYearId == yearId.Value);
            var categories = await catQ.OrderBy(c => c.Name).ToListAsync();

            var viewModel = new FacultyProgressVM
            {
                Initiatives = initiatives,
                SelectedYearId = yearId,
                SelectedPeriodId = periodId,
                SelectedCategoryId = categoryId,
                Years = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(years, "Id", "Name", yearId),
                Periods = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(periods, "Id", "Name", periodId),
                Categories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(categories, "Id", "Name", categoryId)
            };

            return View(viewModel);
        }

        // ==========================================
        // 8. EXPORT TO EXCEL
        // ==========================================
        public async Task<IActionResult> ExportToExcel()
        {
            var deptId = await GetCurrentUserDepartmentId();
            if (deptId == null) return Forbid();

            var data = await _context.Initiatives
                .Include(i => i.Creator)
                .Where(i => i.DepartmentId == deptId) // Strict Filter
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Code,Title,Lecturer,Submitted Date,Status");

            foreach (var item in data)
            {
                string title = item.Title?.Replace(",", ";") ?? "";
                string creator = item.Creator?.FullName?.Replace(",", ";") ?? "";
                string status = item.Status switch
                {
                    InitiativeStatus.Pending => "Pending Review",
                    InitiativeStatus.Faculty_Approved => "Forwarded to R&D Dept",
                    InitiativeStatus.Revision_Required => "Revision Required",
                    InitiativeStatus.Evaluating => "Under Evaluation",
                    InitiativeStatus.Approved => "Approved",
                    InitiativeStatus.Rejected => "Rejected",
                    _ => item.Status.ToString()
                };

                builder.AppendLine($"{item.InitiativeCode},{title},{creator},{item.CreatedAt:dd/MM/yyyy},{status}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var result = new byte[bom.Length + buffer.Length];
            Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            Buffer.BlockCopy(buffer, 0, result, bom.Length, buffer.Length);

            return File(result, "text/csv", $"Initiative_Stats_{DateTime.Now:ddMMyyyy}.csv");
        }

        // ==========================================
        // 9. USER PROFILE PAGE
        // ==========================================
        public async Task<IActionResult> Profile()
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
                DepartmentName = user.Department?.Name ?? "Khoa Công nghệ thông tin",
                AvatarUrl = user.AvatarUrl ?? "https://i.pravatar.cc/150?img=32",
                InitiativeCount = initiativeCount,
                AchievementCount = 5
            };

            return View(viewModel);
        }
        [HttpGet("Faculty/ViewerPage")]
        public IActionResult ViewerPage(string file)
        {
            if (string.IsNullOrEmpty(file))
                return BadRequest();

            ViewBag.FileName = file;
            return View();
        }
        [HttpGet("/Faculty/ViewFilePdf")]
        public async Task<IActionResult> ViewFilePdf(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest();

            var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "initiatives");
            var inputPath = Path.Combine(uploadRoot, fileName);

            if (!System.IO.File.Exists(inputPath))
                return NotFound();

            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp-pdf");
            Directory.CreateDirectory(tempDir);

            var pdfName = Path.GetFileNameWithoutExtension(fileName) + ".pdf";
            var pdfPath = Path.Combine(tempDir, pdfName);

            if (!System.IO.File.Exists(pdfPath))
            {
                var ext = Path.GetExtension(fileName).ToLower();
                if (ext == ".pdf")
                    System.IO.File.Copy(inputPath, pdfPath, true);
                else
                    await ConvertToPdf(inputPath, tempDir);
            }
            foreach (var file in Directory.GetFiles(tempDir, "*.pdf"))
            {
                if (!Path.GetFileName(file)
                    .Equals(pdfName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch
                    {
                        // ignore: file đang bị lock
                    }
                }
            }
            return Json(new
            {
                url = $"/temp-pdf/{pdfName}"
            });
        }


        public async Task<string> ConvertToPdf(string inputPath, string outputDir)
        {
            var sofficePath = @"C:\Program Files\LibreOffice\program\soffice.exe";

            if (!System.IO.File.Exists(sofficePath))
                throw new Exception("LibreOffice (soffice.exe) not found");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sofficePath,
                    Arguments = $"--headless --convert-to pdf \"{inputPath}\" --outdir \"{outputDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            return Path.Combine(
                outputDir,
                Path.GetFileNameWithoutExtension(inputPath) + ".pdf"
            );
        }

    }
}