namespace IdeaTrack.Areas.SciTech.Models
{
    /// <summary>
    /// ViewModel for displaying category with Board/Template links
    /// </summary>
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int PeriodId { get; set; }
        public string PeriodName { get; set; } = "";
        public int? BoardId { get; set; }
        public string? BoardName { get; set; }
        public int? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public int InitiativeCount { get; set; }
    }

    /// <summary>
    /// ViewModel for creating/editing category
    /// </summary>
    public class CategoryCreateEditVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int PeriodId { get; set; }
        public int? BoardId { get; set; }
        public int? TemplateId { get; set; }

        // For dropdowns
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Periods { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Boards { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Templates { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for listing categories by period
    /// </summary>
    public class CategoryListVM
    {
        public int? SelectedPeriodId { get; set; }
        public string? SelectedPeriodName { get; set; }
        public List<CategoryViewModel> Items { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Periods { get; set; } = new();
    }
}
