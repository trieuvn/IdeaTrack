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

// Identity với GUID
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // tùy chỉnh
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();


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
