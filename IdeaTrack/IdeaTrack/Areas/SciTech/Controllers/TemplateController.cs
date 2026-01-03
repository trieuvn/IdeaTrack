using IdeaTrack.Data;
using IdeaTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdeaTrack.Areas.SciTech.Controllers
{
    [Area("SciTech")]
    [Authorize(Roles = "SciTech,OST_Admin,Admin")]
    public class TemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SciTech/Template
        public IActionResult Index(int page = 1, int? type = null, bool? isActive = null, string sortOrder = "asc", string search = "")
        {
            int pageSize = 6;
            var query = _context.EvaluationTemplates.AsQueryable();

            // 1. Search
            if (!string.IsNullOrEmpty(search))
            {
                string s = search.ToLower().Trim();
                query = query.Where(t => t.TemplateName.ToLower().Contains(s)
                                      || (t.Description != null && t.Description.ToLower().Contains(s)));
            }

            // 2. Filter by type
            if (type.HasValue)
            {
                var enumValue = (TemplateType)type.Value;
                query = query.Where(t => t.Type == enumValue);
            }

            // 3. Filter by status
            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            // 4. Sort
            query = (sortOrder == "asc")
                    ? query.OrderBy(x => x.Id)
                    : query.OrderByDescending(x => x.Id);

            // 5. Pagination
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var templates = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.SelectedType = type;
            ViewBag.SelectedStatus = isActive;
            ViewBag.SortOrder = sortOrder;
            ViewBag.SearchTerm = search;

            return View("~/Areas/SciTech/Views/Port/Template.cshtml", templates);
        }

        // POST: /SciTech/Template/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromBody] EvaluationTemplate model)
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

        // POST: /SciTech/Template/Update
        [HttpPost]
        public IActionResult Update([FromBody] EvaluationTemplate model)
        {
            var template = _context.EvaluationTemplates.Find(model.Id);
            if (template == null) return NotFound();

            template.TemplateName = model.TemplateName;
            template.Description = model.Description;

            _context.SaveChanges();
            return Json(new { success = true });
        }

        // POST: /SciTech/Template/Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var template = _context.EvaluationTemplates.Find(id);
            if (template == null) return NotFound();

            _context.EvaluationTemplates.Remove(template);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        // GET: /SciTech/Template/Rule/5
        [HttpGet]
        public IActionResult Rule(int id)
        {
            var criteria = _context.EvaluationCriteria.Where(c => c.TemplateId == id).ToList();
            return View("~/Areas/SciTech/Views/Port/Rule.cshtml", criteria);
        }

        // POST: /SciTech/Template/Rule
        [HttpPost]
        public IActionResult Rule(Dictionary<int, EvaluationCriteriaDto> criteria, int id)
        {
            int templateId = id;
            if (criteria == null) criteria = new Dictionary<int, EvaluationCriteriaDto>();

            var dbItems = _context.EvaluationCriteria
                                  .Where(x => x.TemplateId == templateId)
                                  .ToList();

            // Remove deleted items
            var sentIds = criteria.Keys.ToList();
            var toDelete = dbItems.Where(x => !sentIds.Contains(x.Id)).ToList();
            if (toDelete.Any()) _context.EvaluationCriteria.RemoveRange(toDelete);

            int order = 1;
            foreach (var item in criteria)
            {
                int idSent = item.Key;
                var data = item.Value;
                if (string.IsNullOrWhiteSpace(data.CriteriaName))
                {
                    data.CriteriaName = "Empty";
                }

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
            return RedirectToAction(nameof(Rule), new { id = templateId });
        }
    }
}
