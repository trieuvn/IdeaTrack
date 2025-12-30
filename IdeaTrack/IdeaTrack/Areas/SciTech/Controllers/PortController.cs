using IdeaTrack.Areas.SciTech.Models;
using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
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
        public IActionResult ApproveInitiative(
     int id,
     string requestContent)
        {
            var initiative = _context.Initiatives
                                     .FirstOrDefault(i => i.Id == id);

            if (initiative == null)
                return NotFound();

            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1,
                RequestContent = requestContent,
                RequestedDate = DateTime.Now,
                Status = "Open",
                IsResolved=true
            };

            initiative.Status = InitiativeStatus.Approved;

            _context.RevisionRequests.Add(revision);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult RejectInitiative(
     int id,
     DateTime deadline,
     string requestContent)
        {
            var initiative = _context.Initiatives
                                     .FirstOrDefault(i => i.Id == id);

            if (initiative == null)
                return NotFound();

            var revision = new RevisionRequest
            {
                InitiativeId = id,
                RequesterId = 1,
                RequestContent = requestContent,
                Deadline = deadline,
                RequestedDate = DateTime.Now,
                Status = "Open"
            };

            initiative.Status = InitiativeStatus.Rejected;

            _context.RevisionRequests.Add(revision);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTemplate([FromBody] EvaluationTemplate model)
        {
            if (model == null) return BadRequest(new { message = "Data is null" });

            ModelState.Remove("CriteriaList");
            ModelState.Remove("Type");
            ModelState.Remove("Id");

            if (string.IsNullOrEmpty(model.TemplateName))
            {
                ModelState.AddModelError("TemplateName", "Tên template không được để trống");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                       .ToDictionary(k => k.Key, v => v.Value.Errors.Select(e => e.ErrorMessage))
                });
            }
            model.IsActive = false;
            model.Type = TemplateType.Screening;
            _context.EvaluationTemplates.Add(model);
            _context.SaveChanges();

            return Ok(new { id = model.Id });
        }
        public IActionResult Follow() => View();
        [HttpGet]
        public IActionResult Rule(int id)
        {
            var criteria = _context.EvaluationCriteria.Where(c=>c.TemplateId==id).ToList();
            return View(criteria);
        }

        [HttpPost]
        public IActionResult Rule(Dictionary<int, EvaluationCriteriaDto> criteria,int id)
        {
            int templateId = id;
            if (criteria == null) criteria = new Dictionary<int, EvaluationCriteriaDto>();
            var dbItems = _context.EvaluationCriteria
                                  .Where(x => x.TemplateId == templateId)
                                  .ToList();

            
            var sentIds = criteria.Keys.ToList();
            var toDelete = dbItems.Where(x => !sentIds.Contains(x.Id)).ToList();
            if (toDelete.Any()) _context.EvaluationCriteria.RemoveRange(toDelete);

            int order = 1;
            foreach (var item in criteria)
            {
                int idSent = item.Key;
                var data = item.Value;

                var existing = dbItems.FirstOrDefault(x => x.Id == idSent);
                if (existing != null)
                {
                    existing.CriteriaName = data.CriteriaName;
                    existing.Description = data.Description;
                    existing.MaxScore = data.MaxScore;
                    existing.SortOrder = order++;
                }
                else
                {
                    _context.EvaluationCriteria.Add(new EvaluationCriteria
                    {
                        CriteriaName = data.CriteriaName,
                        Description = data.Description,
                        MaxScore = data.MaxScore,
                        SortOrder = order++,
                        TemplateId = templateId
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Rule));
        }
        public IActionResult Template(int page = 1, int? type = null, bool? isActive = null, string sortOrder = "asc", string search = "")
        {
            int pageSize = 6;
            var query = _context.EvaluationTemplates.AsQueryable();

            // 1. Tìm kiếm (Search)
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower().Trim();
                query = query.Where(t => t.TemplateName.ToLower().Contains(s)
                                      || (t.Description != null && t.Description.ToLower().Contains(s)));
            }

            // 2. Lọc (Filter)
            if (type.HasValue) query = query.Where(t => (int)t.Type == type.Value);
            if (isActive.HasValue) query = query.Where(t => t.IsActive == isActive.Value);

            // 3. Sắp xếp (FIXED)
            // Nếu sortOrder là "asc" thì OrderBy, ngược lại thì OrderByDescending
            query = (sortOrder == "asc")
                    ? query.OrderBy(x => x.Id)
                    : query.OrderByDescending(x => x.Id);

            // 4. Phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var templates = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // ViewBag giữ trạng thái
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.SelectedType = type;
            ViewBag.SelectedStatus = isActive;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SearchTerm = search;

            return View(templates);
        }
        [HttpPost]
        public IActionResult UpdateTemplate([FromBody] EvaluationTemplate model)
        {
            var template = _context.EvaluationTemplates.Find(model.Id);
            if (template == null) return NotFound();

            template.TemplateName = model.TemplateName;
            template.Description = model.Description;

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteTemplate(int id)
        {
            var template = _context.EvaluationTemplates.Find(id);
            if (template == null) return NotFound();

            _context.EvaluationTemplates.Remove(template);
            _context.SaveChanges();
            return Json(new { success = true });
        }
        public IActionResult Profile() => View();
        public IActionResult User() => View();
        public IActionResult Councils() => View();
    }
}
