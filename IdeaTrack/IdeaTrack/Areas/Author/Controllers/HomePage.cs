using IdeaTrack.Areas.Author.ViewModels;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.Author.Controllers
{
    [Area("Author")]
    public class HomePage : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomePage(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Author/HomePage
        public async Task<IActionResult> Index()
        {
            // Tạm thời lấy tất cả initiatives (sau này sẽ filter theo user đăng nhập)
            var initiatives = await _context.Initiatives
                .Include(i => i.Category)
                .Include(i => i.AcademicYear)
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .ToListAsync();

            var allInitiatives = await _context.Initiatives.ToListAsync();
            var approvedCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Approved);
            var totalCount = allInitiatives.Count;

            var viewModel = new AuthorDashboardViewModel
            {
                UserName = "John", // Tạm thời hardcode
                RecentInitiatives = initiatives,
                DraftCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Draft),
                SubmittedCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Pending),
                ApprovedCount = approvedCount,
                UnderReviewCount = allInitiatives.Count(i => i.Status == InitiativeStatus.Reviewing || i.Status == InitiativeStatus.Dept_Review),
                TotalScore = 845, // Demo value
                SuccessRate = totalCount > 0 ? Math.Round((decimal)approvedCount / totalCount * 100, 0) : 0
            };

            return View(viewModel);
        }

        // GET: /Author/HomePage/Detail/5
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var initiative = await _context.Initiatives
                .Include(i => i.Category)
                .Include(i => i.AcademicYear)
                .Include(i => i.Department)
                .Include(i => i.Files)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

            var viewModel = new InitiativeDetailViewModel
            {
                Initiative = initiative,
                Files = initiative.Files.ToList()
            };

            return View(viewModel);
        }

        // GET: /Author/HomePage/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new InitiativeCreateViewModel
            {
                Initiative = new Initiative(),
                AcademicYears = new SelectList(await _context.AcademicYears.ToListAsync(), "Id", "Name"),
                Categories = new SelectList(await _context.InitiativeCategories.ToListAsync(), "Id", "Name"),
                Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name")
            };

            return View(viewModel);
        }

        // POST: /Author/HomePage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InitiativeCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var initiative = viewModel.Initiative;
                
                // Tự động sinh mã sáng kiến
                var count = await _context.Initiatives.CountAsync() + 1;
                initiative.InitiativeCode = $"SK-{DateTime.Now.Year}-{count:D4}";
                
                // Tạm thời gán ProposerId = 1 (sau này sẽ lấy từ user đăng nhập)
                initiative.ProposerId = 1;
                initiative.Status = InitiativeStatus.Draft;
                initiative.CreatedAt = DateTime.Now;

                _context.Initiatives.Add(initiative);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Detail), new { id = initiative.Id });
            }

            // Nếu validation fail, load lại dropdowns
            viewModel.AcademicYears = new SelectList(await _context.AcademicYears.ToListAsync(), "Id", "Name", viewModel.Initiative.AcademicYearId);
            viewModel.Categories = new SelectList(await _context.InitiativeCategories.ToListAsync(), "Id", "Name", viewModel.Initiative.CategoryId);
            viewModel.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", viewModel.Initiative.DepartmentId);

            return View(viewModel);
        }

        // GET: /Author/HomePage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null)
            {
                return NotFound();
            }

            var viewModel = new InitiativeCreateViewModel
            {
                Initiative = initiative,
                AcademicYears = new SelectList(await _context.AcademicYears.ToListAsync(), "Id", "Name", initiative.AcademicYearId),
                Categories = new SelectList(await _context.InitiativeCategories.ToListAsync(), "Id", "Name", initiative.CategoryId),
                Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", initiative.DepartmentId)
            };

            return View(viewModel);
        }

        // POST: /Author/HomePage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InitiativeCreateViewModel viewModel)
        {
            if (id != viewModel.Initiative.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInitiative = await _context.Initiatives.FindAsync(id);
                    if (existingInitiative == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các field
                    existingInitiative.Title = viewModel.Initiative.Title;
                    existingInitiative.Description = viewModel.Initiative.Description;
                    existingInitiative.Budget = viewModel.Initiative.Budget;
                    existingInitiative.CategoryId = viewModel.Initiative.CategoryId;
                    existingInitiative.AcademicYearId = viewModel.Initiative.AcademicYearId;
                    existingInitiative.DepartmentId = viewModel.Initiative.DepartmentId;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InitiativeExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Detail), new { id });
            }

            viewModel.AcademicYears = new SelectList(await _context.AcademicYears.ToListAsync(), "Id", "Name", viewModel.Initiative.AcademicYearId);
            viewModel.Categories = new SelectList(await _context.InitiativeCategories.ToListAsync(), "Id", "Name", viewModel.Initiative.CategoryId);
            viewModel.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", viewModel.Initiative.DepartmentId);

            return View(viewModel);
        }

        // POST: /Author/HomePage/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var initiative = await _context.Initiatives.FindAsync(id);
            if (initiative == null)
            {
                return NotFound();
            }

            _context.Initiatives.Remove(initiative);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(History));
        }

        // GET: /Author/HomePage/History
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> History()
        {
            var initiatives = await _context.Initiatives
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(initiatives);
        }

        // GET: /Author/HomePage/Files
        public async Task<IActionResult> Files()
        {
            var files = await _context.InitiativeFiles
                .Include(f => f.Initiative)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            return View(files);
        }

        // GET: /Author/HomePage/Attachments/5
        public async Task<IActionResult> Attachments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var initiative = await _context.Initiatives
                .Include(i => i.Files)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (initiative == null)
            {
                return NotFound();
            }

            return View(initiative);
        }

        // GET: /Author/HomePage/Setting
        public IActionResult Setting()
        {
            return View();
        }

        // GET: /Author/HomePage/DailyReport
        public async Task<IActionResult> DailyReport()
        {
            var allInitiatives = await _context.Initiatives
                .Include(i => i.Category)
                .Include(i => i.Proposer)
                .ToListAsync();

            var totalCount = allInitiatives.Count;
            
            // Category distribution
            var categoryColors = new[] { "#3b82f6", "#60a5fa", "#93c5fd", "#bfdbfe", "#dbeafe" };
            var categoryGroups = allInitiatives
                .GroupBy(i => i.Category?.Name ?? "Uncategorized")
                .Select((g, index) => new CategoryDistribution
                {
                    CategoryName = g.Key,
                    Count = g.Count(),
                    Percentage = totalCount > 0 ? Math.Round((decimal)g.Count() / totalCount * 100, 1) : 0,
                    Color = categoryColors[index % categoryColors.Length]
                })
                .OrderByDescending(c => c.Count)
                .ToList();

            // Today's submissions
            var today = DateTime.Today;
            var todaySubmissions = allInitiatives
                .Where(i => i.CreatedAt.Date == today)
                .OrderByDescending(i => i.CreatedAt)
                .ToList();

            // Recent submissions (last 7 days)
            var weekAgo = today.AddDays(-7);
            var recentSubmissions = allInitiatives
                .Where(i => i.CreatedAt.Date >= weekAgo)
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .ToList();

            // Monthly stats
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthlyInitiatives = allInitiatives.Where(i => i.CreatedAt >= monthStart).ToList();

            var viewModel = new DailyReportViewModel
            {
                TotalInitiatives = totalCount,
                CategoryDistributions = categoryGroups,
                TodaySubmissions = todaySubmissions,
                RecentSubmissions = recentSubmissions,
                SelectedDate = today,
                MonthlyTotal = monthlyInitiatives.Count,
                MonthlyApproved = monthlyInitiatives.Count(i => i.Status == InitiativeStatus.Approved),
                MonthlyPending = monthlyInitiatives.Count(i => i.Status == InitiativeStatus.Pending || i.Status == InitiativeStatus.Reviewing)
            };

            return View(viewModel);
        }

        private bool InitiativeExists(int id)
        {
            return _context.Initiatives.Any(e => e.Id == id);
        }
    }
}
