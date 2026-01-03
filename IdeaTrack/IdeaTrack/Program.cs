using IdeaTrack.Areas.Faculty.Hubs;
using IdeaTrack.Data;
using IdeaTrack.Models;
using IdeaTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using OfficeOpenXml;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(36500);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register Business Services
builder.Services.AddScoped<IInitiativeService, InitiativeService>();
builder.Services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();

// MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Configure authentication cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
var app = builder.Build();
var supportedCultures = new[] { "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

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

app.UseRouting();
app.UseRewriter(rewriteOptionsadmin);
app.UseRewriter(rewriteOptions);
app.UseRewriter(rewriteOptionscouncils);
app.UseRewriter(rewriteOptionsdetails);
app.UseHttpsRedirection();
app.UseStaticFiles();



app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<NotificationHub>("/notificationHub");
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
