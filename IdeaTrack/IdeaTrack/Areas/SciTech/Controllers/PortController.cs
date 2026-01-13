using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using System.Diagnostics;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class PortController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInitiativeService _initiativeService;

        public PortController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IInitiativeService initiativeService)
        {
            _context = context;
            _userManager = userManager;
            _initiativeService = initiativeService;
        }

        // GET: /SciTech/Port
        public IActionResult Index(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? keyword,
            int page = 1)
        {
            const int PAGE_SIZE = 5;

            var allowedStatuses = new[] { 
                InitiativeStatus.Faculty_Approved, 
                InitiativeStatus.Evaluating, 
                InitiativeStatus.Re_Evaluating, 
                InitiativeStatus.Pending_Final, 
                InitiativeStatus.Approved 
            };

            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .Include(i => i.Period)
                .ThenInclude(p => p.AcademicYear)
                .Where(i => allowedStatuses.Contains(i.Status))
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(i =>
                    (i.SubmittedDate ?? i.CreatedAt) >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(i =>
                    (i.SubmittedDate ?? i.CreatedAt) <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(status)
                && Enum.TryParse<InitiativeStatus>(status, out var parsedStatus))
            {
                query = query.Where(i => i.Status == parsedStatus);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(i =>
                    i.Title.Contains(keyword) ||
                    i.InitiativeCode.Contains(keyword));
            }

            var totalItems = query.Count();

            var items = query
                .OrderByDescending(i => i.SubmittedDate ?? i.CreatedAt)
                .Skip((page - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .Select(i => new InitiativeListVM
                {
                    Id = i.Id,
                    InitiativeCode = i.InitiativeCode,
                    Title = i.Title,
                    ProposerName = i.Creator.FullName,
                    DepartmentName = i.Department.Name,
                    SubmittedDate = i.SubmittedDate ?? i.CreatedAt,
                    Status = i.Status.ToString(),
                    PeriodName = i.Period != null ? i.Period.Name : "N/A",
                    AcademicYear = i.Period != null && i.Period.AcademicYear != null ? i.Period.AcademicYear.Name : "N/A"
                })
                .ToList();

            var vm = new InitiativeReportVM
            {
                Items = items,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status,
                Keyword = keyword,
                CurrentPage = page,
                TotalItems = totalItems
            };

            // ==================== DASHBOARD STATS ====================
            var today = DateTime.Today;
            
            // 1. Calculate specific counts requested
            var allRelevantInitiatives = _context.Initiatives
                .Where(i => allowedStatuses.Contains(i.Status)) // Optimization: only query relevant range
                .Select(i => i.Status)
                .ToList(); // Determine in memory or DB? List is better for multiple counts if smallset, or use separate counts.
                           // Actually, given constraints, separate DB counts are safer for large datasets, but efficient enough here.
            
            // Use Direct DB counts for accuracy
            ViewBag.FacultyApprovedCount = _context.Initiatives.Count(i => i.Status == InitiativeStatus.Faculty_Approved);
            ViewBag.EvaluatingCount = _context.Initiatives.Count(i => i.Status == InitiativeStatus.Evaluating || i.Status == InitiativeStatus.Re_Evaluating);
            ViewBag.PendingFinalCount = _context.Initiatives.Count(i => i.Status == InitiativeStatus.Pending_Final);

            // 3. Audit logs per day (last 7 days) - Keeping existing chart logic as requested 'Update Statistics *Cards*' not chart removal.
            var startDate = today.AddDays(-6);
            var auditLogsPerDay = _context.SystemAuditLogs
                .Where(l => l.Timestamp >= startDate)
                .AsEnumerable()
                .GroupBy(l => l.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);
            
            var chartLabels = new List<string>();
            var chartData = new List<int>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dateStr = date.ToString("yyyy-MM-dd");
                chartLabels.Add(date.ToString("dd/MM"));
                chartData.Add(auditLogsPerDay.ContainsKey(dateStr) ? auditLogsPerDay[dateStr] : 0);
            }
            
            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;

            return View(vm);
        }

        // POST: /SciTech/Port/ExportExcel
        [HttpPost]
        public IActionResult ExportExcel(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? keyword)
        {
            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) <= toDate.Value);

            if (!string.IsNullOrEmpty(status)
                && Enum.TryParse<InitiativeStatus>(status, out var parsedStatus))
                query = query.Where(i => i.Status == parsedStatus);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(i =>
                    i.Title.Contains(keyword) ||
                    i.InitiativeCode.Contains(keyword));

            var data = query.Select(i => new
            {
                i.InitiativeCode,
                i.Title,
                Creator = i.Creator.FullName,
                Department = i.Department.Name,
                SubmittedDate = i.SubmittedDate ?? i.CreatedAt,
                Status = i.Status.ToString()
            }).ToList();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Initiatives");
            ws.Cells.LoadFromCollection(data, true);
            ws.Cells.AutoFitColumns();

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BaoCaoHoSo.xlsx");
        }

        // POST: /SciTech/Port/ExportPdf
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportPdf(
            DateTime? fromDate,
            DateTime? toDate,
            string? status,
            string? keyword)
        {
            var query = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => (i.SubmittedDate ?? i.CreatedAt) <= toDate.Value);

            if (!string.IsNullOrEmpty(status)
                && Enum.TryParse<InitiativeStatus>(status, out var parsedStatus))
                query = query.Where(i => i.Status == parsedStatus);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(i =>
                    i.Title.Contains(keyword) ||
                    i.InitiativeCode.Contains(keyword));

            var data = query.Select(i => new InitiativePdfVM
            {
                InitiativeCode = i.InitiativeCode,
                Title = i.Title,
                Proposer = i.Creator.FullName,
                Department = i.Department.Name,
                Status = i.Status.ToString()
            }).ToList();

            var document = new InitiativeReportPdf(data);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "BaoCaoHoSo.pdf");
        }

        // GET: /SciTech/Port/Result/5
        public async Task<IActionResult> Result(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .Include(i => i.Category)
                .Include(i => i.FinalResult)
                .Include(i => i.Assignments)
                    .ThenInclude(a => a.Member)
                .Include(i => i.Assignments)
                    .ThenInclude(a => a.EvaluationDetails)
                        .ThenInclude(ed => ed.Criteria)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null) return NotFound();

            // Calculate Stats
            var assignments = initiative.Assignments.Where(a => a.RoundNumber == initiative.CurrentRound).ToList();
            var completed = assignments.Count(a => a.Status == AssignmentStatus.Completed);
            var totalMembers = assignments.Count;
            var consensusRate = totalMembers > 0 ? (double)completed / totalMembers * 100 : 0;

            decimal averageScore = 0;
            if (completed > 0)
            {
                averageScore = assignments
                    .Where(a => a.Status == AssignmentStatus.Completed)
                    .SelectMany(a => a.EvaluationDetails)
                    .Sum(d => d.ScoreGiven) / completed; 
            }
            
            var memberScores = assignments.Select(a => new MemberScoreVM
            {
                MemberId = a.MemberId,
                MemberName = a.Member.FullName,
                Role = "Member", 
                Scores = a.EvaluationDetails.ToDictionary(d => d.Criteria.CriteriaName, d => d.ScoreGiven),
                TotalScore = a.EvaluationDetails.Sum(d => d.ScoreGiven),
                IsCompleted = a.Status == AssignmentStatus.Completed,
                Strengths = a.Strengths,
                Limitations = a.Limitations,
                Recommendations = a.Recommendations
            }).ToList();

            if (completed > 0)
            {
                 averageScore = memberScores.Where(ms => ms.IsCompleted).Average(ms => ms.TotalScore);
            }

            var vm = new InitiativeResultVM
            {
                Id = initiative.Id,
                Title = initiative.Title,
                InitiativeCode = initiative.InitiativeCode,
                ProposerName = initiative.Creator.FullName,
                AverageScore = averageScore,
                ConsensusRate = consensusRate,
                CompletedCount = completed,
                TotalMembers = totalMembers,
                FinalStatus = initiative.FinalResult?.ChairmanDecision ?? "Pending",
                Rank = initiative.FinalResult?.Rank ?? "N/A",
                MemberScores = memberScores,
                Status = initiative.Status.ToString(),
                ConsolidatedStrengths = string.Join("\n\n", memberScores.Where(m => !string.IsNullOrWhiteSpace(m.Strengths)).Select(m => $"- {m.MemberName}: {m.Strengths}")),
                ConsolidatedLimitations = string.Join("\n\n", memberScores.Where(m => !string.IsNullOrWhiteSpace(m.Limitations)).Select(m => $"- {m.MemberName}: {m.Limitations}")),
                ConsolidatedRecommendations = string.Join("\n\n", memberScores.Where(m => !string.IsNullOrWhiteSpace(m.Recommendations)).Select(m => $"- {m.MemberName}: {m.Recommendations}"))
            };

            // RANK CALCULATION (Threshold-based)
            if (string.IsNullOrEmpty(vm.Rank) || vm.Rank == "N/A")
            {
                if (averageScore >= 90) vm.Rank = "Xuất sắc";
                else if (averageScore >= 80) vm.Rank = "Tốt";
                else if (averageScore >= 70) vm.Rank = "Khá";
                else if (averageScore >= 50) vm.Rank = "Đạt";
                else vm.Rank = "Không đạt";
            }

            return View(vm);
        }

        // GET: /SciTech/Port/Approve/5
        [HttpGet]
        public IActionResult Approve(int id)
        {
            var initiative = _context.Initiatives
                .Include(i => i.Creator)
                .Include(i => i.Department)
                .Include(i => i.Category)
                .Include(i => i.Files)
                .FirstOrDefault(i => i.Id == id);

            if (initiative == null) return NotFound();

            // Auto-redirect to Result view for evaluating/finished initiatives
            var resultStatuses = new[] { 
                InitiativeStatus.Evaluating, 
                InitiativeStatus.Re_Evaluating, 
                InitiativeStatus.Pending_Final, 
                InitiativeStatus.Approved,
                InitiativeStatus.Rejected
            };

            if (resultStatuses.Contains(initiative.Status))
            {
                return RedirectToAction("Result", new { id = initiative.Id });
            }

            var vm = new InitiativeDetailVM
            {
                Id = initiative.Id,
                InitiativeCode = initiative.InitiativeCode,
                Title = initiative.Title,
                ProposerName = initiative.Creator.FullName,
                DepartmentName = initiative.Department.Name,
                SubmittedDate = initiative.SubmittedDate ?? initiative.CreatedAt,
                Status = initiative.Status,
                Budget = initiative.Budget,
                Code = initiative.Department.Code,
                Description = initiative.Description,
                Category = initiative.Category.Name,
                Files = initiative.Files.Select(f => new InitiativeFileVM
                {
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType
                }).ToList()
            };

            return View(vm);
        }

        // POST: /SciTech/Port/ApproveInitiative
        // When OST approves, use IInitiativeService for auto-assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInitiative(int id, string? requestContent)
        {
            var initiative = await _context.Initiatives.FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
                return NotFound();

            // Log the approval request
            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1, // TODO: Replace with current user ID
                RequestContent = requestContent ?? "Council approval and grading assignment initiated.",
                RequestedDate = DateTime.Now,
                Status = "Approved",
                IsResolved = true
            };

            _context.RevisionRequests.Add(revision);
            await _context.SaveChangesAsync();

            // Use service for auto-assignment (delegates to IInitiativeService)
            var assigned = await _initiativeService.AutoAssignToBoardAsync(id);

            if (!assigned)
            {
                // If no board/template configured, approve directly
                initiative.Status = InitiativeStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Approved, but no Council or Grading Criteria configured for this category.";
            }

            TempData["SuccessMessage"] = "Successfully approved and assigned to the Council for grading!";
            return RedirectToAction("Result", new { id });
        }

        // POST: /SciTech/Port/RejectInitiative
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInitiative(int id, DateTime? deadline, string requestContent)
        {
            var initiative = await _context.Initiatives.FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
                return NotFound();

            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1, // TODO: Replace with current user ID
                RequestContent = requestContent,
                Deadline = deadline,
                RequestedDate = DateTime.Now,
                Status = "Open"
            };

            initiative.Status = InitiativeStatus.Rejected;

            _context.RevisionRequests.Add(revision);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Initiative has been rejected.";
            return RedirectToAction("Result", new { id });
        }

        // POST: /SciTech/Port/RequestReEvaluation
        // When PKHCN requests a new evaluation round
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReEvaluation(int id, string? reason)
        {
            var success = await _initiativeService.CreateNewRoundAsync(id);

            if (!success)
            {
                TempData["ErrorMessage"] = "Could not create a new evaluation round. Please try again.";
                return RedirectToAction("Approve", new { id });
            }

            // Log the re-evaluation request
            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1,
                RequestContent = reason ?? "Requesting re-evaluation for next round",
                RequestedDate = DateTime.Now,
                Status = "Re-evaluation",
                IsResolved = false
            };

            _context.RevisionRequests.Add(revision);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Re-evaluation round requested successfully!";
            return RedirectToAction("Index");
        }

        // POST: /SciTech/Port/MakeFinalDecision
        // When PKHCN makes final decision (Approve/Reject)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeFinalDecision(int id, string decision)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var decidedByUserId = currentUser?.Id ?? 1;

            var finalStatus = decision == "Approve" 
                ? InitiativeStatus.Approved 
                : InitiativeStatus.Rejected;

            var success = await _initiativeService.CreateFinalResultAsync(id, finalStatus, decidedByUserId);

            if (!success)
            {
                TempData["ErrorMessage"] = "Could not make the final decision. Please try again.";
                return RedirectToAction("Approve", new { id });
            }

            TempData["SuccessMessage"] = $"Successfully {(decision == "Approve" ? "Approved" : "Rejected")} this initiative!";
            return RedirectToAction("Index");
        }

        // POST: /SciTech/Port/RestartEvaluation
        // Reset scores and set status to Re_Evaluating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestartEvaluation(int id)
        {
            var initiative = await _context.Initiatives
                .Include(i => i.Assignments)
                .ThenInclude(a => a.EvaluationDetails)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null) return NotFound();

            // Reset Assignments
            var currentAssignments = initiative.Assignments.Where(a => a.RoundNumber == initiative.CurrentRound).ToList();
            foreach (var assign in currentAssignments)
            {
                assign.Status = AssignmentStatus.Assigned; // Reset status
                assign.Strengths = null;
                assign.Limitations = null;
                assign.Recommendations = null;
                assign.Decision = null;
                assign.DecisionDate = null;
                assign.ReviewComment = null;
                
                // Remove scores? Or just reset them. Removing details is cleaner.
                _context.EvaluationDetails.RemoveRange(assign.EvaluationDetails);
            }

            initiative.Status = InitiativeStatus.Re_Evaluating;
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Evaluation has been restarted. Scores are reset.";
            return RedirectToAction("Result", new { id });
        }

        // GET: /SciTech/Port/Follow
        public IActionResult Follow() => View();

        // GET: /SciTech/Port/Profile
        public IActionResult Profile() => View();

        // ==================== USER MANAGEMENT ACTIONS ====================

        // POST: /SciTech/Port/ExportExcelUser
        // Export User List
        public IActionResult ExportExcelUser(string keyword, string role, string status)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Position == role);

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status)
                {
                    case "active": query = query.Where(u => u.IsActive); break;
                    case "locked":
                    case "pending": query = query.Where(u => !u.IsActive); break;
                }
            }

            var users = query.OrderBy(u => u.Id).Select(u => new
            {
                u.FullName,
                u.Email,
                Role = u.Position,
                Status = u.IsActive ? "Active" : "Locked"
            }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("User_List");

            ws.Cells[1, 1].Value = "USER REPORT";
            ws.Cells[1, 1, 1, 4].Merge = true;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            var headers = new[] { "Full Name", "Email", "Role", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[2, i + 1].Value = headers[i];
                ws.Cells[2, i + 1].Style.Font.Bold = true;
            }

            int row = 3;
            foreach (var u in users)
            {
                ws.Cells[row, 1].Value = u.FullName;
                ws.Cells[row, 2].Value = u.Email;
                ws.Cells[row, 3].Value = u.Role;
                ws.Cells[row, 4].Value = u.Status;
                row++;
            }
            ws.Cells.AutoFitColumns();

            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"User_Report_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // POST: /SciTech/Port/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return RedirectToAction("Index", "User");
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Email already exists.";
                return RedirectToAction("Index", "User");
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                DepartmentId = model.DepartmentId,
                IsActive = true,
                EmailConfirmed = true,
                AcademicRank = model.AcademicRank,
                Degree = model.Degree,
                Position = model.Position
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                TempData["SuccessMessage"] = $"User {model.FullName} created successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index", "User");
        }

        // GET: /SciTech/Port/LockUser/5
        public async Task<IActionResult> LockUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await _userManager.UpdateAsync(user); // Ensure IsActive is saved
                TempData["SuccessMessage"] = "User locked successfully.";
            }
            return RedirectToAction("Index", "User");
        }

        // GET: /SciTech/Port/UnlockUser/5
        public async Task<IActionResult> UnlockUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.IsActive = true;
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.SetLockoutEnabledAsync(user, false);
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = "User unlocked successfully.";
            }
            return RedirectToAction("Index", "User");
        }

        // GET: /SciTech/Port/GetUserDetail
        [HttpGet]
        public IActionResult GetUserDetail(int id)
        {
            var user = _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    fullName = u.FullName,
                    avatarUrl = u.AvatarUrl,
                    academicRank = u.AcademicRank,
                    degree = u.Degree,
                    departmentName = u.Department != null ? u.Department.Name : "N/A",
                    position = u.Position,
                    isActive = u.IsActive
                })
                .FirstOrDefault();

            if (user == null) return NotFound();
            return Json(user);
        }
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name is required.");

            // 1. Làm sạch tham số đầu vào
            var cleanedSearchName = fileName.Replace("\n", "").Replace("\r", "").Trim();

            // 2. Tìm kiếm trong DB (Sử dụng Contains hoặc Equals tùy vào cách bạn lưu)
            // Lưu ý: Nếu DB lớn, việc xử lý chuỗi trong FirstOrDefaultAsync có thể chậm.
            var file = await _context.InitiativeFiles
                .FirstOrDefaultAsync(f => f.FileName.Replace("\n", "").Replace("\r", "").Trim() == cleanedSearchName);

            if (file == null)
                return NotFound("Không tìm thấy thông tin file trong cơ sở dữ liệu.");

            // 3. Xử lý đường dẫn vật lý (Physical Path)
            // Lấy tên file thực tế từ cột FilePath (bỏ các ký tự dẫn đầu nếu có)
            string actualFileName = Path.GetFileName(file.FilePath);

            // Kết hợp đường dẫn tuyệt đối đến thư mục chứa file
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "initiatives");
            var filePath = Path.Combine(uploadsFolder, actualFileName);

            // 4. Kiểm tra sự tồn tại của file vật lý trên ổ cứng
            if (!System.IO.File.Exists(filePath))
            {
                // Debug: Có thể log filePath ở đây để kiểm tra server đang tìm ở đâu
                return NotFound($"File vật lý không tồn tại tại: {actualFileName}");
            }

            // 5. Xác định Content Type
            var contentType = GetContentType(file.FileName);

            // 6. Trả về file (Dùng cleanedSearchName để tên file tải về không bị xuống dòng)
            return PhysicalFile(filePath, contentType, cleanedSearchName);
        }

        // Hàm phụ trợ để code sạch hơn
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower().Trim();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };
        }
        [HttpGet("/SciTech/ViewerPage")]
        public IActionResult ViewerPage(string file)
        {
            if (string.IsNullOrEmpty(file))
                return BadRequest();

            ViewBag.FileName = file;
            return View();
        }
        [HttpGet("/SciTech/ViewFilePdf")]
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
