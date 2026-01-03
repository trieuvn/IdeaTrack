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

            _logger.LogInformation("Data seeding completed!");
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "SciTech", "FacultyLeader", "CouncilMember", "Lecturer", "Author" };
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
                new Department { Name = "Khoa Công nghệ Thông tin", Code = "CNTT" },
                new Department { Name = "Khoa Điện - Điện tử", Code = "DT" },
                new Department { Name = "Khoa Cơ khí", Code = "CK" },
                new Department { Name = "Khoa Kinh tế", Code = "KT" },
                new Department { Name = "Khoa Ngoại ngữ", Code = "NN" },
                new Department { Name = "Khoa Khoa học Cơ bản", Code = "KHCB" },
                new Department { Name = "Phòng KHCN", Code = "KHCN" },
                new Department { Name = "Khoa Xây dựng", Code = "XD" },
                new Department { Name = "Khoa Môi trường", Code = "MT" },
                new Department { Name = "Ban Giám hiệu", Code = "BGH" }
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
                ("admin", "Quản trị viên", "admin@uni.edu.vn", "KHCN", new[] { "Admin" }),
                ("scitech1", "Nguyễn Văn KHCN", "scitech1@uni.edu.vn", "KHCN", new[] { "SciTech" }),
                ("scitech2", "Trần Thị KHCN", "scitech2@uni.edu.vn", "KHCN", new[] { "SciTech" }),
                ("leader_cntt", "PGS.TS Lê Văn An", "leader.cntt@uni.edu.vn", "CNTT", new[] { "FacultyLeader" }),
                ("leader_dt", "TS. Phạm Văn Bình", "leader.dt@uni.edu.vn", "DT", new[] { "FacultyLeader" }),
                ("council1", "GS.TS Nguyễn Hữu Đức", "council1@uni.edu.vn", "BGH", new[] { "CouncilMember" }),
                ("council2", "PGS.TS Trần Mai Hoa", "council2@uni.edu.vn", "CNTT", new[] { "CouncilMember" }),
                ("council3", "TS. Lê Minh Giang", "council3@uni.edu.vn", "DT", new[] { "CouncilMember" }),
                ("author1", "ThS. Nguyễn Văn Linh", "author1@uni.edu.vn", "CNTT", new[] { "Lecturer", "Author" }),
                ("author2", "ThS. Trần Thị Mai", "author2@uni.edu.vn", "CNTT", new[] { "Lecturer", "Author" }),
                ("author3", "TS. Lê Hoàng Nam", "author3@uni.edu.vn", "DT", new[] { "Lecturer", "Author" }),
                ("author4", "ThS. Phạm Văn Oanh", "author4@uni.edu.vn", "DT", new[] { "Lecturer", "Author" }),
                ("author5", "ThS. Hoàng Thị Phương", "author5@uni.edu.vn", "CK", new[] { "Lecturer", "Author" })
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
                new AcademicYear { Name = "Năm học 2024-2025", IsCurrent = false },
                new AcademicYear { Name = "Năm học 2025-2026", IsCurrent = true },
                new AcademicYear { Name = "Năm học 2026-2027", IsCurrent = false }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedPeriodsAsync()
        {
            if (await _context.InitiativePeriods.AnyAsync()) return;

            var years = await _context.AcademicYears.ToDictionaryAsync(y => y.Name, y => y.Id);

            _context.InitiativePeriods.AddRange(
                new InitiativePeriod { Name = "Đợt Sáng kiến HK1 2024-2025", Description = "Đợt 1", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2024, 12, 31), IsActive = false, AcademicYearId = years["Năm học 2024-2025"] },
                new InitiativePeriod { Name = "Đợt Sáng kiến HK2 2024-2025", Description = "Đợt 2", StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 6, 30), IsActive = false, AcademicYearId = years["Năm học 2024-2025"] },
                new InitiativePeriod { Name = "Đợt Sáng kiến HK1 2025-2026", Description = "Đợt 1 (Đang mở)", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 12, 31), IsActive = true, AcademicYearId = years["Năm học 2025-2026"] }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedTemplatesAsync()
        {
            if (await _context.EvaluationTemplates.AnyAsync()) return;

            var t1 = new EvaluationTemplate { TemplateName = "Mẫu chấm Sáng kiến Kỹ thuật", Description = "Dùng cho sáng kiến kỹ thuật", Type = TemplateType.Scoring, IsActive = true };
            var t2 = new EvaluationTemplate { TemplateName = "Mẫu chấm Sáng kiến Quản lý", Description = "Dùng cho sáng kiến quản lý", Type = TemplateType.Scoring, IsActive = true };

            _context.EvaluationTemplates.AddRange(t1, t2);
            await _context.SaveChangesAsync();

            // Criteria for t1
            _context.EvaluationCriteria.AddRange(
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Tính mới", Description = "Mức độ mới mẻ", MaxScore = 25, SortOrder = 1 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Tính khả thi", Description = "Khả năng áp dụng", MaxScore = 20, SortOrder = 2 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Hiệu quả kinh tế", Description = "Lợi ích kinh tế", MaxScore = 25, SortOrder = 3 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Khả năng nhân rộng", Description = "Áp dụng nhiều nơi", MaxScore = 15, SortOrder = 4 },
                new EvaluationCriteria { TemplateId = t1.Id, CriteriaName = "Chất lượng hồ sơ", Description = "Tính đầy đủ", MaxScore = 15, SortOrder = 5 }
            );

            // Criteria for t2
            _context.EvaluationCriteria.AddRange(
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Tính mới", Description = "Đổi mới quy trình", MaxScore = 20, SortOrder = 1 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Tính hiệu quả", Description = "Cải thiện hiệu suất", MaxScore = 25, SortOrder = 2 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Tiết kiệm chi phí", Description = "Giảm chi phí", MaxScore = 20, SortOrder = 3 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Dễ triển khai", Description = "Dễ áp dụng", MaxScore = 20, SortOrder = 4 },
                new EvaluationCriteria { TemplateId = t2.Id, CriteriaName = "Chất lượng hồ sơ", Description = "Tính đầy đủ", MaxScore = 15, SortOrder = 5 }
            );

            await _context.SaveChangesAsync();
        }

        private async Task SeedBoardsAsync()
        {
            if (await _context.Boards.AnyAsync()) return;

            var b1 = new Board { BoardName = "Hội đồng Sáng kiến CNTT", Description = "Chấm sáng kiến CNTT", IsActive = true };
            var b2 = new Board { BoardName = "Hội đồng Sáng kiến Kỹ thuật", Description = "Chấm sáng kiến Kỹ thuật", IsActive = true };

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
                new InitiativeCategory { PeriodId = period.Id, Name = "Sáng kiến Phần mềm", BoardId = boards.GetValueOrDefault("Hội đồng Sáng kiến CNTT"), TemplateId = templates.GetValueOrDefault("Mẫu chấm Sáng kiến Kỹ thuật") },
                new InitiativeCategory { PeriodId = period.Id, Name = "Sáng kiến Phần cứng/IoT", BoardId = boards.GetValueOrDefault("Hội đồng Sáng kiến Kỹ thuật"), TemplateId = templates.GetValueOrDefault("Mẫu chấm Sáng kiến Kỹ thuật") },
                new InitiativeCategory { PeriodId = period.Id, Name = "Sáng kiến Quản lý", BoardId = boards.GetValueOrDefault("Hội đồng Sáng kiến CNTT"), TemplateId = templates.GetValueOrDefault("Mẫu chấm Sáng kiến Quản lý") }
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
                new Initiative { InitiativeCode = "SK2025-001", Title = "Hệ thống quản lý thư viện thông minh", Description = "Xây dựng hệ thống thư viện AI sử dụng ML để recommend sách", Budget = 50000000, Status = InitiativeStatus.Draft, CreatorId = author1.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id },
                new Initiative { InitiativeCode = "SK2025-002", Title = "Ứng dụng điểm danh QR", Description = "Điểm danh sinh viên bằng QR code", Budget = 10000000, Status = InitiativeStatus.Pending, CreatorId = author2.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-5) },
                new Initiative { InitiativeCode = "SK2025-003", Title = "Chatbot hỗ trợ sinh viên", Description = "Chatbot tư vấn tuyển sinh sử dụng NLP", Budget = 30000000, Status = InitiativeStatus.Faculty_Approved, CreatorId = author3.Id, DepartmentId = depts["DT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-10) },
                new Initiative { InitiativeCode = "SK2025-004", Title = "Phần mềm chấm bài tự động", Description = "Chấm trắc nghiệm OMR sử dụng OpenCV", Budget = 20000000, Status = InitiativeStatus.Evaluating, CreatorId = author1.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-15) },
                new Initiative { InitiativeCode = "SK2024-001", Title = "Phần mềm quản lý điểm", Description = "Quản lý điểm online với .NET + SQL", Budget = 40000000, Status = InitiativeStatus.Approved, CreatorId = author1.Id, DepartmentId = depts["CNTT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-60) },
                new Initiative { InitiativeCode = "SK2024-002", Title = "Máy bay không người lái", Description = "Drone nông nghiệp phun thuốc", Budget = 500000000, Status = InitiativeStatus.Rejected, CreatorId = author3.Id, DepartmentId = depts["DT"], CategoryId = category.Id, PeriodId = period.Id, SubmittedDate = DateTime.Now.AddDays(-45) }
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
                new InitiativeAssignment { InitiativeId = evaluating.Id, BoardId = board.Id, MemberId = council1.Id, TemplateId = template.Id, RoundNumber = 1, StageName = "Vòng 1", Status = AssignmentStatus.Assigned },
                new InitiativeAssignment { InitiativeId = evaluating.Id, BoardId = board.Id, MemberId = council2.Id, TemplateId = template.Id, RoundNumber = 1, StageName = "Vòng 1", Status = AssignmentStatus.Assigned }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedReferenceFormsAsync()
        {
            if (await _context.ReferenceForms.AnyAsync()) return;

            var period = await _context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
            if (period == null) return;

            _context.ReferenceForms.AddRange(
                new ReferenceForm { PeriodId = period.Id, FileName = "Mẫu đơn đăng ký.docx", Description = "Mẫu đơn chính thức", FileUrl = "https://example.com/form1.docx" },
                new ReferenceForm { PeriodId = period.Id, FileName = "Hướng dẫn viết thuyết minh.pdf", Description = "Hướng dẫn chi tiết", FileUrl = "https://example.com/guide.pdf" },
                new ReferenceForm { PeriodId = period.Id, FileName = "Tiêu chí đánh giá.pdf", Description = "Tiêu chí chấm điểm", FileUrl = "https://example.com/criteria.pdf" }
            );
            await _context.SaveChangesAsync();
        }
    }
}
