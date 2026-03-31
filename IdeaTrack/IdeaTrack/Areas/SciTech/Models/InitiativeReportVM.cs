using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdeaTrack.Areas.SciTech.Models
{
    public class InitiativeReportVM
    {
        // DATA
        public List<InitiativeListVM> Items { get; set; } = new();

        // FILTER
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public string? Keyword { get; set; }
        public int? PeriodId { get; set; }
        public int? CategoryId { get; set; }
        public int? YearId { get; set; }

        // DROPDOWN DATA
        public List<SelectListItem> Periods { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Years { get; set; } = new();

        // PAGING
        public int CurrentPage { get; set; }
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
