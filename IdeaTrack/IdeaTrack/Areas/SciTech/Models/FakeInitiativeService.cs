using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class FakeInitiativeService
{
    public static async Task SeedAllAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        // =======================
        // 1️⃣ SEED DEPARTMENT
        // =======================
        if (!context.Departments.Any())
        {
            context.Departments.AddRange(
                new Department { Code = "CNTT", Name = "Cong nghe thong tin" },
                new Department { Code = "NN", Name = "Nong nghiep" },
                new Department { Code = "MT", Name = "Moi truong" }
            );
            await context.SaveChangesAsync();
        }

        // =======================
        // 2️⃣ SEED ACADEMIC YEAR
        // =======================
        if (!context.AcademicYears.Any())
        {
            context.AcademicYears.AddRange(
                new AcademicYear { Name = "2023-2024" },
                new AcademicYear { Name = "2024-2025", IsCurrent = true }
            );
            await context.SaveChangesAsync();
        }

        // =======================
        // 3️⃣ SEED INITIATIVE PERIOD
        // =======================
        if (!context.InitiativePeriods.Any())
        {
            var currentYear = await context.AcademicYears.FirstOrDefaultAsync(a => a.IsCurrent);
            if (currentYear != null)
            {
                context.InitiativePeriods.Add(new InitiativePeriod
                {
                    Name = "Dot 1 - 2024-2025",
                    AcademicYearId = currentYear.Id,
                    StartDate = new DateTime(2024, 9, 1),
                    EndDate = new DateTime(2025, 6, 30),
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }
        }

        // =======================
        // 4️⃣ SEED EVALUATION TEMPLATE
        // =======================
        if (!context.EvaluationTemplates.Any())
        {
            context.EvaluationTemplates.AddRange(
                new EvaluationTemplate { TemplateName = "Evaluation Templates AI", Type = TemplateType.Scoring, IsActive = true },
                new EvaluationTemplate { TemplateName = "Evaluation Templates Nong nghiep", Type = TemplateType.Scoring, IsActive = true },
                new EvaluationTemplate { TemplateName = "Evaluation Templates Moi truong", Type = TemplateType.Scoring, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // =======================
        // 5️⃣ SEED BOARD (EVALUATION BOARD)
        // =======================
        if (!context.Boards.Any())
        {
            context.Boards.AddRange(
                new Board { BoardName = "Hoi dong Ky thuat", FiscalYear = 2024, IsActive = true },
                new Board { BoardName = "Hoi dong Nong nghiep", FiscalYear = 2024, IsActive = true },
                new Board { BoardName = "Hoi dong Moi truong", FiscalYear = 2024, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // =======================
        // 6️⃣ SEED INITIATIVE CATEGORY (linked to Period, Board, Template)
        // =======================
        if (!context.InitiativeCategories.Any())
        {
            var period = await context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);
            var templates = await context.EvaluationTemplates.ToListAsync();
            var boards = await context.Boards.ToListAsync();

            if (period != null && templates.Count >= 3 && boards.Count >= 3)
            {
                context.InitiativeCategories.AddRange(
                    new InitiativeCategory
                    {
                        Name = "Tri tue nhan tao",
                        PeriodId = period.Id,
                        BoardId = boards[0].Id,
                        TemplateId = templates[0].Id
                    },
                    new InitiativeCategory
                    {
                        Name = "Nong nghiep",
                        PeriodId = period.Id,
                        BoardId = boards[1].Id,
                        TemplateId = templates[1].Id
                    },
                    new InitiativeCategory
                    {
                        Name = "Moi truong",
                        PeriodId = period.Id,
                        BoardId = boards[2].Id,
                        TemplateId = templates[2].Id
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        // =======================
        // 7️⃣ SEED USER (IDENTITY)
        // =======================
        if (userManager.Users.Count() < 3)
        {
            var departments = await context.Departments.Take(3).ToListAsync();

            for (int i = 1; i <= 3; i++)
            {
                var email = $"user{i}@test.com";

                if (await userManager.FindByEmailAsync(email) != null)
                    continue;

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = $"Giang vien {i}",
                    DepartmentId = departments[i - 1].Id,
                    EmailConfirmed = true,
                    IsActive = true
                };

                await userManager.CreateAsync(user, "Test@123");
            }
        }

        // =======================
        // 8️⃣ SEED INITIATIVE
        // =======================
        if (context.Initiatives.Any())
            return;

        var users = await userManager.Users.Take(3).ToListAsync();
        var departmentsSeeded = await context.Departments.Take(3).ToListAsync();
        var categories = await context.InitiativeCategories.Take(3).ToListAsync();
        var activePeriod = await context.InitiativePeriods.FirstOrDefaultAsync(p => p.IsActive);

        if (users.Count < 3 || categories.Count < 3 || activePeriod == null)
            return;

        var initiatives = new List<Initiative>
        {
            new Initiative
            {
                InitiativeCode = "SK-2024-0001",
                Title = "Nghien cuu AI trong chan doan y te",
                CreatedAt = DateTime.Now.AddDays(-30),
                SubmittedDate = DateTime.Now.AddDays(-30),
                Status = InitiativeStatus.Approved,
                CreatorId = users[0].Id,
                DepartmentId = departmentsSeeded[0].Id,
                PeriodId = activePeriod.Id,
                CategoryId = categories[0].Id,
                CurrentRound = 1
            },
            new Initiative
            {
                InitiativeCode = "SK-2024-0002",
                Title = "Nong nghiep xanh ben vung",
                CreatedAt = DateTime.Now.AddDays(-25),
                SubmittedDate = DateTime.Now.AddDays(-25),
                Status = InitiativeStatus.Pending,
                CreatorId = users[1].Id,
                DepartmentId = departmentsSeeded[1].Id,
                PeriodId = activePeriod.Id,
                CategoryId = categories[1].Id,
                CurrentRound = 1
            },
            new Initiative
            {
                InitiativeCode = "SK-2024-0003",
                Title = "He thong IoT giam sat moi truong",
                CreatedAt = DateTime.Now.AddDays(-20),
                SubmittedDate = DateTime.Now.AddDays(-20),
                Status = InitiativeStatus.Rejected,
                CreatorId = users[2].Id,
                DepartmentId = departmentsSeeded[2].Id,
                PeriodId = activePeriod.Id,
                CategoryId = categories[2].Id,
                CurrentRound = 1
            }
        };

        context.Initiatives.AddRange(initiatives);
        await context.SaveChangesAsync();

        // =======================
        // 9️⃣ SEED AUTHORSHIP (Creator as primary author)
        // =======================
        var seededInitiatives = await context.Initiatives.ToListAsync();
        foreach (var initiative in seededInitiatives)
        {
            if (!context.InitiativeAuthorships.Any(a => a.InitiativeId == initiative.Id))
            {
                context.InitiativeAuthorships.Add(new InitiativeAuthorship
                {
                    InitiativeId = initiative.Id,
                    AuthorId = initiative.CreatorId,
                    IsCreator = true,
                    JoinedAt = initiative.CreatedAt
                });
            }
        }
        await context.SaveChangesAsync();
    }
}
