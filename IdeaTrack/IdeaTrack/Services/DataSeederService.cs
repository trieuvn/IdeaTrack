using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Services
{
    /// <summary>
    /// Service to seed sample data into the database
    /// </summary>
    public class DataSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<DataSeederService> _logger;

        public DataSeederService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<DataSeederService> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAllAsync()
        {
            _logger.LogInformation("Starting data seeding...");

            await SeedRolesAsync();
            await SeedDepartmentsAsync();
            await SeedUsersAsync();
            await SeedAcademicYearsAsync();
            await SeedPeriodsAsync();
            await SeedTemplatesAsync();
            await SeedBoardsAsync();
            await SeedCategoriesAsync();
            await SeedInitiativesAsync();
            await SeedAssignmentsAsync();
            await SeedReferenceFormsAsync();
            
            // Expand data to 20+ rows per table for comprehensive testing
            await SeedExpandedDataAsync();
            await SeedLeaderboardDataAsync();

            _logger.LogInformation("Data seeding completed!");
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "SciTech", "FacultyLeader", "CouncilMember", "Lecturer", "Author", "User" };
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, Description = roleName });
                }
            }
        }

        private async Task SeedDepartmentsAsync()
        {
            if (await _context.Departments.AnyAsync()) return;

            var departments = new[]
            {
                new Department { Name = "Department of Information Technology", Code = "CNTT" },
                new Department { Name = "Department of Electrical & Electronics", Code = "DT" },
                new Department { Name = "Department of Mechanical Engineering", Code = "CK" },
                new Department { Name = "Department of Economics", Code = "KT" },
                new Department { Name = "Department of Foreign Languages", Code = "NN" },
                new Department { Name = "Department of Basic Sciences", Code = "KHCB" },
                new Department { Name = "Office of Science & Technology", Code = "KHCN" },
                new Department { Name = "Department of Construction", Code = "XD" },
                new Department { Name = "Department of Environment", Code = "MT" },
                new Department { Name = "Board of Directors", Code = "BGH" }
            };
            _context.Departments.AddRange(departments);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUsersAsync()
        {
            var password = "123456";
            var depts = await _context.Departments.ToDictionaryAsync(d => d.Code, d => d.Id);
            
            var users = new (string Username, string FullName, string Email, string DeptCode, string[] Roles)[]
            {
                ("admin", "Administrator", "admin@uni.edu.vn", "KHCN", new[] { "Admin" }),
                ("scitech1", "John Smith", "scitech1@uni.edu.vn", "KHCN", new[] { "SciTech" }),
                ("scitech2", "Jane Doe", "scitech2@uni.edu.vn", "KHCN", new[] { "SciTech" }),
                ("leader_cntt", "Prof. Michael Brown", "leader.cntt@uni.edu.vn", "CNTT", new[] { "FacultyLeader" }),
                ("leader_dt", "Dr. David Wilson", "leader.dt@uni.edu.vn", "DT", new[] { "FacultyLeader" }),
                ("council1", "Prof. Robert Johnson", "council1@uni.edu.vn", "BGH", new[] { "CouncilMember" }),
                ("council2", "Assoc. Prof. Emily Davis", "council2@uni.edu.vn", "CNTT", new[] { "CouncilMember" }),
                ("council3", "Dr. Sarah Miller", "council3@uni.edu.vn", "DT", new[] { "CouncilMember" }),
                ("author1", "MSc. Peter Lee", "author1@uni.edu.vn", "CNTT", new[] { "Lecturer", "Author" }),
                ("author2", "MSc. Mary Chen", "author2@uni.edu.vn", "CNTT", new[] { "Lecturer", "Author" }),
                ("author3", "Dr. James Wang", "author3@uni.edu.vn", "DT", new[] { "Lecturer", "Author" }),
                ("author4", "MSc. Kevin Nguyen", "author4@uni.edu.vn", "DT", new[] { "Lecturer", "Author" }),
                ("author5", "MSc. Linda Tran", "author5@uni.edu.vn", "CK", new[] { "Lecturer", "Author" })
            };

            foreach (var u in users)
            {
                if (await _userManager.FindByNameAsync(u.Username) != null) continue;

                var user = new ApplicationUser
                {
                    UserName = u.Username,
                    Email = u.Email,
                    EmailConfirmed = true,
                    FullName = u.FullName,
                    DepartmentId = depts.GetValueOrDefault(u.DeptCode)
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(user, u.Roles);
                }
            }
        }

        private async Task SeedAcademicYearsAsync()
        {
            if (await _context.AcademicYears.AnyAsync()) return;

            _context.AcademicYears.AddRange(
                new AcademicYear { Name = "Academic Year 2024-2025", IsCurrent = false },
                new AcademicYear { Name = "Academic Year 2025-2026", IsCurrent = true },
                new AcademicYear { Name = "Academic Year 2026-2027", IsCurrent = false }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedPeriodsAsync()
        {
            var years = await _context.AcademicYears.ToDictionaryAsync(y => y.Name, y => y.Id);
            
            // Get the "current" year ID
            int currentYearId = years.FirstOrDefault(y => y.Key.Contains("2025-2026")).Value;
            if (currentYearId == 0)
                currentYearId = await _context.AcademicYears.Where(y => y.IsCurrent).Select(y => y.Id).FirstOrDefaultAsync();
            
            // Create or update periods to ensure we have open periods for testing
            var today = DateTime.Today;
            var existingPeriods = await _context.InitiativePeriods.ToListAsync();
            
            if (existingPeriods.Any())
            {
                // Update existing periods to ensure at least some are "open" (covering today)
                foreach (var period in existingPeriods.Take(3))
                {
                    // Set dates to cover today if they don't already
                    if (period.StartDate > today || period.EndDate < today)
                    {
                        period.StartDate = today.AddMonths(-3);
                        period.EndDate = today.AddMonths(3);
                        _logger.LogInformation("Updated period {PeriodId} dates to be open: {Start} - {End}", 
                            period.Id, period.StartDate, period.EndDate);
                    }
                }
                await _context.SaveChangesAsync();
                return;
            }

            // Create new periods with dates covering today
            _context.InitiativePeriods.AddRange(
                new InitiativePeriod { Name = "Initiative Period S1 2024-2025", Description = "Period 1", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2024, 12, 31), IsActive = false, AcademicYearId = years.Values.FirstOrDefault() },
                new InitiativePeriod { Name = "Initiative Period S2 2024-2025", Description = "Period 2", StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 6, 30), IsActive = false, AcademicYearId = years.Values.FirstOrDefault() },
                new InitiativePeriod { Name = "Initiative Period S1 2025-2026 (OPEN)", Description = "Current Active Period", StartDate = today.AddMonths(-3), EndDate = today.AddMonths(6), IsActive = true, AcademicYearId = currentYearId },
                new InitiativePeriod { Name = "Innovation Period 2025-2026 (OPEN)", Description = "Innovation Initiative Period", StartDate = today.AddDays(-30), EndDate = today.AddMonths(6), IsActive = true, AcademicYearId = currentYearId }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedTemplatesAsync()
        {
            if (await _context.EvaluationTemplates.AnyAsync()) return;

            var t1 = new EvaluationTemplate { TemplateName = "Technical Initiative Evaluation Template", Description = "For technical initiatives", Type = TemplateType.Scoring, IsActive = true };
            var t2 = new EvaluationTemplate { TemplateName = "Management Initiative Evaluation Template", Description = "For management initiatives", Type = TemplateType.Scoring, IsActive = true };

            _context.EvaluationTemplates.AddRange(t1, t2);
            await _context.SaveChangesAsync();

            // Criteria for t1
            _context.EvaluationCriteria.AddRange(
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Novelty", Description = "Level of innovation", MaxScore = 25, SortOrder = 1 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Feasibility", Description = "Implementation capability", MaxScore = 20, SortOrder = 2 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Economic Impact", Description = "Economic benefits", MaxScore = 25, SortOrder = 3 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Scalability", Description = "Applicability to multiple areas", MaxScore = 15, SortOrder = 4 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Documentation Quality", Description = "Completeness of submission", MaxScore = 15, SortOrder = 5 }
            );

            // Criteria for t2
            _context.EvaluationCriteria.AddRange(
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Novelty", Description = "Process innovation", MaxScore = 20, SortOrder = 1 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Effectiveness", Description = "Performance improvement", MaxScore = 25, SortOrder = 2 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Cost Savings", Description = "Cost reduction", MaxScore = 20, SortOrder = 3 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Ease of Implementation", Description = "Easy to apply", MaxScore = 20, SortOrder = 4 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Documentation Quality", Description = "Completeness of submission", MaxScore = 15, SortOrder = 5 }
            );

            await _context.SaveChangesAsync();
        }

        private async Task SeedBoardsAsync()
        {
            if (await _context.Boards.AnyAsync()) return;

            var b1 = new Board { BoardName = "IT Initiative Evaluation Board", Description = "Evaluates IT initiatives", IsActive = true };
            var b2 = new Board { BoardName = "Technical Initiative Evaluation Board", Description = "Evaluates technical initiatives", IsActive = true };

            _context.Boards.AddRange(b1, b2);
            await _context.SaveChangesAsync();

            var council1 = await _userManager.FindByNameAsync("council1");
            var council2 = await _userManager.FindByNameAsync("council2");
            var council3 = await _userManager.FindByNameAsync("council3");

            if (council1 != null && council2 != null && council3 != null)
            {
                _context.BoardMembers.AddRange(
                    new BoardMember { BoardId = b1.Id, UserId = council1.Id, Role = BoardRole.Chairman },
                    new BoardMember { BoardId = b1.Id, UserId = council2.Id, Role = BoardRole.Member },
                    new BoardMember { BoardId = b1.Id, UserId = council3.Id, Role = BoardRole.Member },
                    new BoardMember { BoardId = b2.Id, UserId = council1.Id, Role = BoardRole.Chairman },
                    new BoardMember { BoardId = b2.Id, UserId = council2.Id, Role = BoardRole.Member }
                );
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedCategoriesAsync()
        {
            if (await _context.InitiativeCategories.AnyAsync()) return;

            var period = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
            if (period == null) return;

            var boards = await _context.Boards.ToDictionaryAsync(b => b.BoardName, b => b.Id);
            var templates = await _context.EvaluationTemplates.ToDictionaryAsync(t => t.TemplateName, t => t.Id);

            _context.InitiativeCategories.AddRange(
                new InitiativeCategory { PeriodId = period.Id, Name = "Software Initiative", BoardId = boards.GetValueOrDefault("IT Initiative Evaluation Board"), TemplateId = templates.GetValueOrDefault("Technical Initiative Evaluation Template") },
                new InitiativeCategory { PeriodId = period.Id, Name = "Hardware/IoT Initiative", BoardId = boards.GetValueOrDefault("Technical Initiative Evaluation Board"), TemplateId = templates.GetValueOrDefault("Technical Initiative Evaluation Template") },
                new InitiativeCategory { PeriodId = period.Id, Name = "Management Initiative", BoardId = boards.GetValueOrDefault("IT Initiative Evaluation Board"), TemplateId = templates.GetValueOrDefault("Management Initiative Evaluation Template") }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedInitiativesAsync()
        {
            if (await _context.Initiatives.AnyAsync()) return;

            var author1 = await _userManager.FindByNameAsync("author1");
            var author2 = await _userManager.FindByNameAsync("author2");
            var author3 = await _userManager.FindByNameAsync("author3");
            var period = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
            var category = await _context.InitiativeCategories.FirstOrDefaultAsync();

            if (author1 == null || author2 == null || author3 == null || period == null || category == null) return;

            var depts = await _context.Departments.ToDictionaryAsync(d => d.Code, d => d.Id);

            var initiatives = new[]
            {
                new Initiative { InitiativeCode = "SK2025-001", Title = "Smart Library Management System", Description = "Build an AI-powered library system using ML for book recommendations", Budget = 50000000, Status = InitiativeStatus.Draft, CreatorId = author1.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id },
                new Initiative { InitiativeCode = "SK2025-002", Title = "QR Attendance Application", Description = "Student attendance using QR codes", Budget = 10000000, Status = InitiativeStatus.Pending, CreatorId = author2.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-5) },
                new Initiative { InitiativeCode = "SK2025-003", Title = "Student Support Chatbot", Description = "Admission counseling chatbot using NLP", Budget = 30000000, Status = InitiativeStatus.Faculty_Approved, CreatorId = author3.Id, DepartmentId = depts["DT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-10) },
                new Initiative { InitiativeCode = "SK2025-004", Title = "Automatic Grading Software", Description = "OMR multiple-choice grading using OpenCV", Budget = 20000000, Status = InitiativeStatus.Evaluating, CreatorId = author1.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-15) },
                new Initiative { InitiativeCode = "SK2024-001", Title = "Grade Management Software", Description = "Online grade management with .NET + SQL", Budget = 40000000, Status = InitiativeStatus.Approved, CreatorId = author1.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-60) },
                new Initiative { InitiativeCode = "SK2024-002", Title = "Agricultural Drone", Description = "Agricultural drone for pesticide spraying", Budget = 500000000, Status = InitiativeStatus.Rejected, CreatorId = author3.Id, DepartmentId = depts["DT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-45) }
            };

            _context.Initiatives.AddRange(initiatives);
            await _context.SaveChangesAsync();

            // Add authorships
            foreach (var init in initiatives)
            {
                _context.InitiativeAuthorships.Add(new InitiativeAuthorship { InitiativeId = init.Id, AuthorId = init.CreatorId, IsCreator = true });
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedAssignmentsAsync()
        {
            if (await _context.InitiativeAssignments.AnyAsync()) return;

            var evaluating = await _context.Initiatives.FirstOrDefaultAsync(i => i.Status == InitiativeStatus.Evaluating);
            if (evaluating == null) return;

            var council1 = await _userManager.FindByNameAsync("council1");
            var council2 = await _userManager.FindByNameAsync("council2");
            var template = await _context.EvaluationTemplates.FirstOrDefaultAsync();
            var board = await _context.Boards.FirstOrDefaultAsync();

            if (council1 == null || council2 == null || template == null || board == null) return;

            _context.InitiativeAssignments.AddRange(
                new InitiativeAssignment { InitiativeId = evaluating.Id, BoardId = board.Id, MemberId = council1.Id, TemplateId = template.Id, RoundNumber = 1, StageName = "Round 1", Status = AssignmentStatus.Assigned },
                new InitiativeAssignment { InitiativeId = evaluating.Id, BoardId = board.Id, MemberId = council2.Id, TemplateId = template.Id, RoundNumber = 1, StageName = "Round 1", Status = AssignmentStatus.Assigned }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedReferenceFormsAsync()
        {
            var period = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
            if (period == null) return;

            // Ensure docs directory exists
            var docsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "docs");
            if (!Directory.Exists(docsPath))
            {
                Directory.CreateDirectory(docsPath);
            }

            // Create sample files if they don't exist
            var sampleFiles = new[]
            {
                ("Form_M01_DangKy.docx", "docx", "Form M01 - Đơn đăng ký sáng kiến"),
                ("Form_M02_MoTa.docx", "docx", "Form M02 - Mô tả chi tiết sáng kiến"),
                ("Huong_Dan.pdf", "pdf", "Hướng dẫn viết và nộp sáng kiến"),
                ("Tieu_Chi_Danh_Gia.pdf", "pdf", "Tiêu chí đánh giá sáng kiến")
            };

            foreach (var (fileName, fileType, formName) in sampleFiles)
            {
                var filePath = Path.Combine(docsPath, fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    // Create a simple sample file
                    if (fileType == "docx")
                    {
                        // Create a simple text file renamed to docx (for demo purposes)
                        await System.IO.File.WriteAllTextAsync(filePath, $"Sample content for {formName}");
                    }
                    else if (fileType == "pdf")
                    {
                        // Create a minimal PDF-like file for demo
                        await System.IO.File.WriteAllTextAsync(filePath, $"%PDF-1.4\nSample {formName}");
                    }
                }
            }

            // Check for existing records with external URLs and update them
            var existingForms = await _context.ReferenceForms.ToListAsync();
            if (existingForms.Any(f => f.FileUrl.StartsWith("http")))
            {
                // Remove old records with external URLs
                _context.ReferenceForms.RemoveRange(existingForms.Where(f => f.FileUrl.StartsWith("http")));
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed {Count} reference forms with external URLs", existingForms.Count(f => f.FileUrl.StartsWith("http")));
            }

            // Only add new records if we don't have enough local ones
            if (!await _context.ReferenceForms.AnyAsync(f => !f.FileUrl.StartsWith("http")))
            {
                _context.ReferenceForms.AddRange(
                    new ReferenceForm { PeriodId = period.Id, FormName = "Form M01 - Đơn đăng ký", FileName = "Form_M01_DangKy.docx", FileType = "docx", Description = "Đơn đăng ký sáng kiến chính thức", FileUrl = "docs/Form_M01_DangKy.docx" },
                    new ReferenceForm { PeriodId = period.Id, FormName = "Form M02 - Mô tả sáng kiến", FileName = "Form_M02_MoTa.docx", FileType = "docx", Description = "Mẫu mô tả chi tiết sáng kiến", FileUrl = "docs/Form_M02_MoTa.docx" },
                    new ReferenceForm { PeriodId = period.Id, FormName = "Hướng dẫn nộp sáng kiến", FileName = "Huong_Dan.pdf", FileType = "pdf", Description = "Hướng dẫn quy trình nộp sáng kiến", FileUrl = "docs/Huong_Dan.pdf" },
                    new ReferenceForm { PeriodId = period.Id, FormName = "Tiêu chí đánh giá", FileName = "Tieu_Chi_Danh_Gia.pdf", FileType = "pdf", Description = "Tiêu chí chấm điểm sáng kiến", FileUrl = "docs/Tieu_Chi_Danh_Gia.pdf" }
                );
                await _context.SaveChangesAsync();
                _logger.LogInformation("Seeded reference forms with local files");
            }
        }
        
        /// <summary>
        /// Expands existing seed data to ensure 20+ rows per table for comprehensive testing
        /// </summary>
        private async Task SeedExpandedDataAsync()
        {
            var today = DateTime.Today;
            
            // =====================================================
            // EXPAND DEPARTMENTS (ensure 20 total)
            // =====================================================
            var existingDeptCount = await _context.Departments.CountAsync();
            if (existingDeptCount < 20)
            {
                var additionalDepts = new[]
                {
                    new Department { Name = "Khoa Công nghệ Thông tin", Code = "IT" },
                    new Department { Name = "Khoa Hóa học", Code = "HH" },
                    new Department { Name = "Khoa Sinh học", Code = "SH" },
                    new Department { Name = "Khoa Toán học", Code = "TH" },
                    new Department { Name = "Khoa Vật lý", Code = "VL" },
                    new Department { Name = "Khoa Y học", Code = "YH" },
                    new Department { Name = "Khoa Dược học", Code = "DH" },
                    new Department { Name = "Khoa Luật", Code = "LUAT" },
                    new Department { Name = "Khoa Tâm lý học", Code = "TLH" },
                    new Department { Name = "Khoa Xã hội học", Code = "XHH" },
                    new Department { Name = "Khoa Địa lý", Code = "DL" },
                    new Department { Name = "Khoa Nghệ thuật", Code = "NT" }
                };
                
                foreach (var dept in additionalDepts)
                {
                    if (!await _context.Departments.AnyAsync(d => d.Code == dept.Code))
                    {
                        _context.Departments.Add(dept);
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Expanded departments to {Count} total", await _context.Departments.CountAsync());
            }
            
            // =====================================================
            // EXPAND ACADEMIC YEARS (ensure 20 total)
            // =====================================================
            var existingYearCount = await _context.AcademicYears.CountAsync();
            if (existingYearCount < 20)
            {
                for (int year = 2015; year <= 2035; year++)
                {
                    var yearName = $"Academic Year {year}-{year + 1}";
                    if (!await _context.AcademicYears.AnyAsync(y => y.Name == yearName))
                    {
                        bool isCurrent = (year == 2025);
                        _context.AcademicYears.Add(new AcademicYear 
                        { 
                            Name = yearName, 
                            IsCurrent = isCurrent,
                            CreatedAt = new DateTime(year, 9, 1)
                        });
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Expanded academic years to {Count} total", await _context.AcademicYears.CountAsync());
            }
            
            // =====================================================
            // EXPAND INITIATIVE PERIODS (ensure 20 total with OPEN ones)
            // =====================================================
            var existingPeriodCount = await _context.InitiativePeriods.CountAsync();
            if (existingPeriodCount < 40) // Target ~40 periods
            {
                var allYears = await _context.AcademicYears.OrderBy(y => y.Name).ToListAsync();
                
                var periodNames = new[]
                {
                    "Research Batch - Sem 1", "Research Batch - Sem 2", 
                    "Innovation Drive", "Digital Transformation Wave"
                };
                
                foreach (var year in allYears)
                {
                    // Create 2 periods per year by default
                    int yearVal = int.Parse(year.Name.Split(' ')[2].Split('-')[0]);
                    
                    foreach (var name in periodNames.Take(2))
                    {
                        var periodName = $"{name} ({yearVal}-{yearVal+1})";
                        if (!await _context.InitiativePeriods.AnyAsync(p => p.Name == periodName && p.AcademicYearId == year.Id))
                        {
                            var startDate = new DateTime(yearVal, 9, 1);
                            if (name.Contains("Sem 2")) startDate = new DateTime(yearVal + 1, 2, 1);
                            
                            var endDate = startDate.AddMonths(4);
                            
                            bool isOpen = (DateTime.Now >= startDate && DateTime.Now <= endDate);

                            _context.InitiativePeriods.Add(new InitiativePeriod
                            {
                                Name = periodName,
                                Description = $"{name} for {year.Name}, focused on university-wide initiatives.",
                                StartDate = startDate,
                                EndDate = endDate,
                                IsActive = isOpen,
                                AcademicYearId = year.Id,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Expanded periods to {Count} total", await _context.InitiativePeriods.CountAsync());
            }
            
            // =====================================================
            // EXPAND EVALUATION TEMPLATES (ensure 20 total)
            // =====================================================
            var existingTemplateCount = await _context.EvaluationTemplates.CountAsync();
            if (existingTemplateCount < 20)
            {
                var templateNames = new[]
                {
                    ("Mẫu chấm NCKH cơ bản", TemplateType.Scoring),
                    ("Mẫu chấm Sáng kiến cải tiến", TemplateType.Scoring),
                    ("Mẫu chấm Đề tài khoa học", TemplateType.Scoring),
                    ("Mẫu chấm Báo cáo nghiên cứu", TemplateType.Scoring),
                    ("Mẫu chấm Phát minh sáng chế", TemplateType.Scoring),
                    ("Mẫu chấm Ứng dụng CNTT", TemplateType.Scoring),
                    ("Mẫu chấm Giải pháp kỹ thuật", TemplateType.Scoring),
                    ("Mẫu chấm Đổi mới sáng tạo", TemplateType.Scoring),
                    ("Mẫu chấm Khởi nghiệp", TemplateType.Scoring),
                    ("Mẫu chấm Nghiên cứu ứng dụng", TemplateType.Scoring),
                    ("Mẫu sàng lọc cấp Khoa", TemplateType.Screening),
                    ("Mẫu sàng lọc nhanh", TemplateType.Screening),
                    ("Mẫu chấm Công nghệ AI", TemplateType.Scoring),
                    ("Mẫu chấm IoT/Robotics", TemplateType.Scoring),
                    ("Mẫu chấm Blockchain/Fintech", TemplateType.Scoring),
                    ("Mẫu chấm Năng lượng tái tạo", TemplateType.Scoring),
                    ("Mẫu chấm Y sinh học", TemplateType.Scoring),
                    ("Mẫu chấm Vật liệu mới", TemplateType.Scoring)
                };
                
                foreach (var (name, type) in templateNames)
                {
                    if (!await _context.EvaluationTemplates.AnyAsync(t => t.TemplateName == name))
                    {
                        var template = new EvaluationTemplate
                        {
                            TemplateName = name,
                            Description = $"Template for {name.Replace("Mẫu chấm ", "")}",
                            Type = type,
                            IsActive = true
                        };
                        _context.EvaluationTemplates.Add(template);
                    }
                }
                await _context.SaveChangesAsync();
                
                // Add criteria for new templates
                var templates = await _context.EvaluationTemplates.ToListAsync();
                foreach (var template in templates)
                {
                    if (!await _context.EvaluationCriteria.AnyAsync(c => c.TemplateId == template.Id))
                    {
                        _context.EvaluationCriteria.AddRange(
                            new EvaluationCriteria { TemplateId = template.Id, CriteriaName = "Tính mới và sáng tạo", MaxScore = 30, SortOrder = 1 },
                            new EvaluationCriteria { TemplateId = template.Id, CriteriaName = "Tính khả thi", MaxScore = 25, SortOrder = 2 },
                            new EvaluationCriteria { TemplateId = template.Id, CriteriaName = "Hiệu quả kinh tế", MaxScore = 25, SortOrder = 3 },
                            new EvaluationCriteria { TemplateId = template.Id, CriteriaName = "Chất lượng tài liệu", MaxScore = 20, SortOrder = 4 }
                        );
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Expanded templates to {Count} total", await _context.EvaluationTemplates.CountAsync());
            }
            
            // =====================================================
            // EXPAND BOARDS (ensure 20 total)
            // =====================================================
            var existingBoardCount = await _context.Boards.CountAsync();
            if (existingBoardCount < 20)
            {
                var boardNames = new[]
                {
                    "Hội đồng NCKH Trường", "Hội đồng Sáng kiến CNTT", "Hội đồng Sáng kiến Kinh tế",
                    "Hội đồng Kỹ thuật Xây dựng", "Hội đồng Điện - Điện tử", "Hội đồng Y học",
                    "Hội đồng Đổi mới Sáng tạo", "Hội đồng Khởi nghiệp", "Hội đồng Môi trường",
                    "Hội đồng Công nghệ AI", "Hội đồng Liên ngành", "Hội đồng Quốc tế",
                    "Hội đồng Sinh học & Dược", "Hội đồng Vật lý", "Hội đồng Hóa học",
                    "Hội đồng Chuyển đổi số", "Hội đồng Năng lượng", "Hội đồng Vật liệu mới"
                };
                
                foreach (var name in boardNames)
                {
                    if (!await _context.Boards.AnyAsync(b => b.BoardName == name))
                    {
                        _context.Boards.Add(new Board
                        {
                            BoardName = name,
                            Description = $"Evaluation board: {name}",
                            FiscalYear = 2025,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Expanded boards to {Count} total", await _context.Boards.CountAsync());
            }
            
            // =====================================================
            // EXPAND INITIATIVES (ensure 20 total)
            // =====================================================
            var existingInitCount = await _context.Initiatives.CountAsync();
            if (existingInitCount < 20)
            {
                var authors = await _context.Users.Where(u => u.UserName!.StartsWith("author")).Take(5).ToListAsync();
                var periods = await _context.InitiativePeriods.Where(p => p.IsActive).Take(3).ToListAsync();
                var categories = await _context.InitiativeCategories.Take(3).ToListAsync();
                var depts = await _context.Departments.Take(10).ToListAsync();
                
                if (authors.Any() && periods.Any() && categories.Any() && depts.Any())
                {
                    var titles = new[]
                    {
                        "Hệ thống quản lý thư viện thông minh", "Ứng dụng điểm danh QR Code",
                        "Chatbot hỗ trợ sinh viên", "Phần mềm chấm trắc nghiệm tự động",
                        "Hệ thống IoT giám sát môi trường", "Ứng dụng di động quản lý lịch học",
                        "Phần mềm phân tích dữ liệu sinh viên", "Hệ thống đánh giá năng lực",
                        "Robot hướng dẫn trong trường", "Ứng dụng AI nhận diện khuôn mặt",
                        "Hệ thống quản lý phòng thí nghiệm", "Phần mềm mô phỏng thí nghiệm",
                        "Ứng dụng blockchain quản lý bằng cấp", "Hệ thống năng lượng mặt trời",
                        "Máy lọc nước thông minh", "Drone giám sát cây trồng",
                        "Hệ thống tưới tiêu tự động", "Mạng xã hội học tập nội bộ",
                        "Ví điện tử thanh toán học phí", "Cổng thông tin việc làm sinh viên",
                        "Hệ thống đặt phòng họp trực tuyến", "Ứng dụng quản lý ký túc xá",
                        "Bản đồ số 3D khuôn viên trường", "Hệ thống cảnh báo cháy thông minh"
                    };
                    
                    var statuses = new[] { InitiativeStatus.Draft, InitiativeStatus.Pending, InitiativeStatus.Faculty_Approved, 
                                          InitiativeStatus.Evaluating, InitiativeStatus.Approved, InitiativeStatus.Rejected };
                    
                    var random = new Random(42);
                    int code = existingInitCount + 1;

                    foreach (var title in titles)
                    {
                        if (!await _context.Initiatives.AnyAsync(i => i.Title == title))
                        {
                            var author = authors[random.Next(authors.Count)];
                            var period = periods[random.Next(periods.Count)];
                            var category = categories[random.Next(categories.Count)];
                            var dept = depts[random.Next(depts.Count)];
                            var status = statuses[random.Next(statuses.Length)];
                            
                            var initiative = new Initiative
                            {
                                InitiativeCode = $"SK2025-{code:D3}",
                                Title = title,
                                Description = $"Mô tả chi tiết cho {title}. Đây là dự án nghiên cứu và phát triển.",
                                Budget = random.Next(10, 100) * 1000000,
                                Status = status,
                                CreatorId = author.Id,
                                DepartmentId = dept.Id,
                                CategoryId = category.Id,
                                PeriodId = period.Id,
                                SubmittedDate = status != InitiativeStatus.Draft ? DateTime.Now.AddDays(-random.Next(1, 60)) : null,
                                CreatedAt = DateTime.Now.AddDays(-random.Next(1, 90))
                            };
                            
                            _context.Initiatives.Add(initiative);
                            code++;
                        }
                    }
                    await _context.SaveChangesAsync();
                    
                    // Add authorships for new initiatives
                    var newInits = await _context.Initiatives.Where(i => !_context.InitiativeAuthorships.Any(a => a.InitiativeId == i.Id)).ToListAsync();
                    foreach (var init in newInits)
                    {
                        _context.InitiativeAuthorships.Add(new InitiativeAuthorship
                        {
                            InitiativeId = init.Id,
                            AuthorId = init.CreatorId,
                            IsCreator = true,
                            JoinedAt = init.CreatedAt
                        });
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Expanded initiatives to {Count} total", await _context.Initiatives.CountAsync());
                }
            }
            
            // =====================================================
            // EXPAND CATEGORIES (ensure categories for all open periods)
            // =====================================================
            var openPeriods = await _context.InitiativePeriods.Where(p => p.IsActive).ToListAsync();
            var boards = await _context.Boards.Take(10).ToListAsync();
            var templatesForCat = await _context.EvaluationTemplates.Take(10).ToListAsync();
            
            var categoryNames = new[] { "Sáng kiến Phần mềm", "Sáng kiến Phần cứng/IoT", "Sáng kiến Quản lý",
                                       "Đổi mới Công nghệ", "Nghiên cứu Ứng dụng" };
            
            foreach (var period in openPeriods)
            {
                foreach (var catName in categoryNames)
                {
                    var fullName = $"{catName}";
                    if (!await _context.InitiativeCategories.AnyAsync(c => c.PeriodId == period.Id && c.Name == fullName))
                    {
                        if (boards.Any() && templatesForCat.Any())
                        {
                            _context.InitiativeCategories.Add(new InitiativeCategory
                            {
                                PeriodId = period.Id,
                                Name = fullName,
                                Description = $"Category for {catName}",
                                BoardId = boards[new Random().Next(boards.Count)].Id,
                                TemplateId = templatesForCat[new Random().Next(templatesForCat.Count)].Id
                            });
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation("Ensured categories exist for all open periods");
            
            _logger.LogInformation("Expanded data seeding completed!");
        }
        /// <summary>
        /// Seeds specific data for Leaderboard testing (Authors, Initiatives, Scores for 2023-2025)
        /// </summary>
        private async Task SeedLeaderboardDataAsync()
        {
            _logger.LogInformation("Seeding Leaderboard data...");

            // 1. Ensure we have mock Authors
            var authors = new List<ApplicationUser>();
            for (int i = 1; i <= 10; i++)
            {
                var username = $"mock_author_{i}";
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = username,
                        Email = $"{username}@uni.edu.vn",
                        EmailConfirmed = true,
                        FullName = $"Dr. Mock Author {i}",
                        DepartmentId = await _context.Departments.Select(d => d.Id).OrderBy(r => Guid.NewGuid()).FirstOrDefaultAsync()
                    };
                    await _userManager.CreateAsync(user, "123456");
                    await _userManager.AddToRolesAsync(user, new[] { "Author", "Lecturer" });
                }
                authors.Add(user);
            }

            // 2. Ensure Categories exist for target years
            var targetYears = new[] { "2023-2024", "2024-2025", "2025-2026" };
            var categories = new List<InitiativeCategory>();

            foreach (var yearSuffix in targetYears)
            {
                var yearName = $"Academic Year {yearSuffix}";
                var year = await _context.AcademicYears.FirstOrDefaultAsync(y => y.Name == yearName);
                
                // Create year if missing
                if (year == null) 
                {
                    year = new AcademicYear { Name = yearName, IsCurrent = (yearSuffix == "2025-2026") };
                    _context.AcademicYears.Add(year);
                    await _context.SaveChangesAsync();
                }

                // Create period if missing
                var periodName = $"Mock Period {yearSuffix}";
                var period = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.Name == periodName);
                if (period == null)
                {
                    int startYear = int.Parse(yearSuffix.Split('-')[0]);
                    period = new InitiativePeriod 
                    { 
                        Name = periodName, 
                        AcademicYearId = year.Id,
                        StartDate = new DateTime(startYear, 9, 1),
                        EndDate = new DateTime(startYear + 1, 6, 30),
                        IsActive = true
                    };
                    _context.InitiativePeriods.Add(period);
                    await _context.SaveChangesAsync();
                }

                // Create category if missing
                var cat = await _context.InitiativeCategories.FirstOrDefaultAsync(c => c.PeriodId == period.Id);
                if (cat == null)
                {
                    var board = await _context.Boards.FirstOrDefaultAsync();
                    var template = await _context.EvaluationTemplates.FirstOrDefaultAsync();
                    cat = new InitiativeCategory 
                    { 
                        Name = $"Mock Category {yearSuffix}", 
                        PeriodId = period.Id,
                        BoardId = board?.Id ?? 1,
                        TemplateId = template?.Id ?? 1
                    };
                    _context.InitiativeCategories.Add(cat);
                    await _context.SaveChangesAsync();
                }
                categories.Add(cat);
            }

            // 3. Create Initiatives with Final Results (Approved)
            var random = new Random();
            int createdCount = 0;

            foreach (var cat in categories)
            {
                // Create 5-8 initiatives per period
                int count = random.Next(5, 9);
                for (int i = 0; i < count; i++)
                {
                    var author = authors[random.Next(authors.Count)];
                    var title = $"Mock Initiative {cat.Name} #{i + 1}";
                    
                    if (!await _context.Initiatives.AnyAsync(x => x.Title == title))
                    {
                        var initiative = new Initiative
                        {
                            InitiativeCode = $"MOCK-{random.Next(1000, 9999)}",
                            Title = title,
                            Description = "Mock data for leaderboard testing",
                            Status = InitiativeStatus.Approved,
                            CreatorId = author.Id,
                            DepartmentId = author.DepartmentId ?? 1,
                            CategoryId = cat.Id,
                            PeriodId = cat.PeriodId,
                            SubmittedDate = DateTime.Now.AddMonths(-random.Next(1, 12))
                        };
                        _context.Initiatives.Add(initiative);
                        await _context.SaveChangesAsync();

                        // Add Authorship
                        _context.InitiativeAuthorships.Add(new InitiativeAuthorship 
                        { 
                            InitiativeId = initiative.Id, 
                            AuthorId = author.Id, 
                            IsCreator = true 
                        });

                        // Add Final Result (CRITICAL for Leaderboard)
                        decimal score = random.Next(65, 98); // Score between 65 and 98
                        
                        // We need a proper ChairmanId. Let's use the first available admin or creating user if no board member
                        var chairman = await _userManager.FindByEmailAsync("council1@uni.edu.vn") 
                                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == "admin")
                                       ?? author;

                        _context.FinalResults.Add(new FinalResult
                        {
                            InitiativeId = initiative.Id,
                            AverageScore = score,
                            FinalScore = score,
                            ChairmanDecision = "Approved", // Must be "Approved" to show
                            Rank = score >= 90 ? "Excellent" : score >= 80 ? "Good" : "Fair",
                            DecisionDate = DateTime.Now,
                            ChairmanId = chairman.Id
                        });

                        createdCount++;
                    }
                }
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Seeded {createdCount} mock initiatives with scores for Leaderboard.");
        }
    }
}
