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
                new Department { Code = "CNTT", Name = "Công nghệ thông tin" },
                new Department { Code = "NN", Name = "Nông nghiệp" },
                new Department { Code = "MT", Name = "Môi trường" }
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
                new AcademicYear { Name = "2024-2025" }
            );
            await context.SaveChangesAsync();
        }

        // =======================
        // 3️⃣ SEED INITIATIVE CATEGORY
        // =======================
        if (!context.InitiativeCategories.Any())
        {
            context.InitiativeCategories.AddRange(
                new InitiativeCategory
                {
                 
                    Name = "Trí tuệ nhân tạo"
                },
                new InitiativeCategory
                {
                    Name = "Nông nghiệp"
                },
                new InitiativeCategory
                {
                    Name = "Môi trường"
                }
            );
            await context.SaveChangesAsync();
        }

        // =======================
        // 4️⃣ SEED USER (IDENTITY)
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
                    FullName = $"Giảng viên {i}",
                    DepartmentId = departments[i - 1].Id,
                    EmailConfirmed = true,
                    IsActive = true
                };

                await userManager.CreateAsync(user, "Test@123");
            }
        }

        // =======================
        // 5️⃣ SEED INITIATIVE
        // =======================
        if (context.Initiatives.Any())
            return;

        var users = await userManager.Users.Take(3).ToListAsync();
        var departmentsSeeded = await context.Departments.Take(3).ToListAsync();
        var categories = await context.InitiativeCategories.Take(3).ToListAsync();
        var academicYearId = await context.AcademicYears
            .OrderByDescending(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync();

        var initiatives = new List<Initiative>
        {
            new Initiative
            {
                InitiativeCode = "HS-001",
                Title = "Nghiên cứu AI trong chẩn đoán y tế",
                CreatedAt = DateTime.Now.AddDays(-30),
                SubmittedDate = DateTime.Now.AddDays(-30),
                Status = InitiativeStatus.Approved,

                ProposerId = users[0].Id,
                DepartmentId = departmentsSeeded[0].Id,
                AcademicYearId = academicYearId,
                CategoryId = categories[0].Id
            },
            new Initiative
            {
                InitiativeCode = "HS-002",
                Title = "Nông nghiệp xanh bền vững",
                CreatedAt = DateTime.Now.AddDays(-25),
                SubmittedDate = DateTime.Now.AddDays(-25),
                Status = InitiativeStatus.Pending,

                ProposerId = users[1].Id,
                DepartmentId = departmentsSeeded[1].Id,
                AcademicYearId = academicYearId,
                CategoryId = categories[1].Id
            },
            new Initiative
            {
                InitiativeCode = "HS-003",
                Title = "Hệ thống IoT giám sát môi trường",
                CreatedAt = DateTime.Now.AddDays(-20),
                SubmittedDate = DateTime.Now.AddDays(-20),
                Status = InitiativeStatus.Rejected,

                ProposerId = users[2].Id,
                DepartmentId = departmentsSeeded[2].Id,
                AcademicYearId = academicYearId,
                CategoryId = categories[2].Id
            }
        };

        context.Initiatives.AddRange(initiatives);
        await context.SaveChangesAsync();
    }
}
