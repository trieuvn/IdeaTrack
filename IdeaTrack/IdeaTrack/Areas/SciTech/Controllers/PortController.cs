using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Reflection.Metadata;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    public class PortController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PortController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(
     DateTime? fromDate,
     DateTime? toDate,
     string? status,
     string? keyword,
     int page = 1)
        {
            const int PAGE_SIZE = 5;

            var query = _context.Initiatives
                .Include(i => i.Proposer)
                .Include(i => i.Department)
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
                    ProposerName = i.Proposer.FullName,
                    DepartmentName = i.Department.Name,
                   
                    SubmittedDate = i.SubmittedDate ?? i.CreatedAt,
                    Status = i.Status.ToString() // OK vì chạy sau SQL
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

            return View(vm);
        }
        [HttpPost]
        public IActionResult ExportExcel(
    DateTime? fromDate,
    DateTime? toDate,
    string? status,
    string? keyword)
        {
            var query = _context.Initiatives
                .Include(i => i.Proposer)
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
                Proposer = i.Proposer.FullName,
                Department = i.Department.Name,
                SubmittedDate = i.SubmittedDate ?? i.CreatedAt,
                Status = i.Status.ToString()
            }).ToList();

            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Initiatives");
            ws.Cells.LoadFromCollection(data, true);
            ws.Cells.AutoFitColumns();

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BaoCaoHoSo.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportPdf(
     DateTime? fromDate,
     DateTime? toDate,
     string? status,
     string? keyword)
        {
            var query = _context.Initiatives
                .Include(i => i.Proposer)
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
                Proposer = i.Proposer.FullName,
                Department = i.Department.Name,
                Status = i.Status.ToString()
            }).ToList();

            var document = new InitiativeReportPdf(data);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", "BaoCaoHoSo.pdf");
        }


        public IActionResult Result() => View();
        [HttpGet]
        public IActionResult Approve(int id)
        {
            var initiative = _context.Initiatives
                .Include(i => i.Proposer)
                .Include(i => i.Department)
                .Include(i => i.Category)
                .Include(i => i.Files) // thêm include Files
                .FirstOrDefault(i => i.Id == id);

            if (initiative == null) return NotFound();

            var vm = new InitiativeDetailVM
            {
                Id = initiative.Id,
                InitiativeCode = initiative.InitiativeCode,
                Title = initiative.Title,
                ProposerName = initiative.Proposer.FullName,
                DepartmentName = initiative.Department.Name,
                SubmittedDate = initiative.SubmittedDate ?? initiative.CreatedAt,
                Status = initiative.Status,
                Budget = initiative.Budget,
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


        public IActionResult Follow() => View();
        public IActionResult Rule() => View();
        public IActionResult Profile() => View();
        public IActionResult User() => View();
        public IActionResult Councils() => View();
    }
}
