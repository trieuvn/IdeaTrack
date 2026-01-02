using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Tùy chỉnh thêm về Password nếu muốn
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Seed data for lookup tables
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Seed AcademicYears
    if (!context.AcademicYears.Any())
    {
        context.AcademicYears.AddRange(
            new AcademicYear { Name = "2023-2024", IsCurrent = false },
            new AcademicYear { Name = "2024-2025", IsCurrent = true },
            new AcademicYear { Name = "2025-2026", IsCurrent = false }
        );
        context.SaveChanges();
    }
    
    // Seed InitiativeCategories
    if (!context.InitiativeCategories.Any())
    {
        context.InitiativeCategories.AddRange(
            new InitiativeCategory { Name = "Cải tiến kỹ thuật", Description = "Sáng kiến về cải tiến quy trình, thiết bị" },
            new InitiativeCategory { Name = "Phần mềm", Description = "Sáng kiến về phần mềm, ứng dụng" },
            new InitiativeCategory { Name = "Quản lý", Description = "Sáng kiến về quản lý, điều hành" },
            new InitiativeCategory { Name = "Giáo dục", Description = "Sáng kiến về phương pháp giảng dạy" }
        );
        context.SaveChanges();
    }
    
    // Seed Departments
    if (!context.Departments.Any())
    {
        context.Departments.AddRange(
            new Department { Name = "Khoa Công nghệ thông tin", Code = "CNTT" },
            new Department { Name = "Khoa Kinh tế", Code = "KT" },
            new Department { Name = "Khoa Ngoại ngữ", Code = "NN" },
            new Department { Name = "Phòng Đào tạo", Code = "PDT" }
        );
        context.SaveChanges();
    }
    
    // Seed default user if needed
    if (!context.Users.Any())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var defaultUser = new ApplicationUser
        {
            UserName = "author@uef.edu.vn",
            Email = "author@uef.edu.vn",
            FullName = "John Doe",
            DepartmentId = context.Departments.First().Id
        };
        userManager.CreateAsync(defaultUser, "123456").Wait();
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
var rewriteOptions = new RewriteOptions()
    .AddRewrite(
        "^FacultyLeader/Review/?$",
        "FacultyLeader/Dashboard/Review",
        skipRemainingRules: true
    );
var rewriteOptionscouncils = new RewriteOptions()
    .AddRewrite(
       @"^Councils(/Home)?/?$",
        "Councils/Page",
        skipRemainingRules: true
    )
    .AddRewrite(
        "^Councils/AssignedInitiatives?$",
        "Councils/Page/AssignedInitiatives",
        skipRemainingRules: true)
    .AddRewrite(
        "^Councils/Details?$",
        "Councils/Page/Details",
        skipRemainingRules: true
        )
    .AddRewrite(
        "^Councils/History?$",
        "Councils/Page/History",
        skipRemainingRules: true
        
        )
    .AddRewrite(
        "^Councils/CouncilChair?$",
        "Councils/Page/CouncilChair",
        skipRemainingRules: true);
var rewriteOptionsdetails = new RewriteOptions()
    .AddRewrite(
        "^FacultyLeader/Details/?$",
        "FacultyLeader/Dashboard/Details",
        skipRemainingRules: true
    )
    .AddRewrite(
        "^FacultyLeader/Profile/?$",
        "FacultyLeader/Dashboard/Profile",
        skipRemainingRules: true
    )
    .AddRewrite(
        "^FacultyLeader/Progress/?$",
        "FacultyLeader/Dashboard/Progress",
        skipRemainingRules: true
    );
var rewriteOptionsadmin = new RewriteOptions()
    .AddRewrite(
        "^Admin/AuditLog/?$",
        "Admin/Intro/AuditLog",
        skipRemainingRules: true
    )
    .AddRewrite(
        "^Admin/User/?$",
        "Admin/Intro/User",
        skipRemainingRules: true
    )
    .AddRewrite(
    @"^Admin(/Dashboard)?/?$",
        "Admin/Intro/Dashboard",
        skipRemainingRules: true)
    ;
app.UseRewriter(rewriteOptionsadmin);
app.UseRewriter(rewriteOptions);
app.UseRewriter(rewriteOptionscouncils);
app.UseRewriter(rewriteOptionsdetails);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  
app.UseAuthorization();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
