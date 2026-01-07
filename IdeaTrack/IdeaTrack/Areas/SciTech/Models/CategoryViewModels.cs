using System.ComponentModel.DataAnnotations;

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
    /// ViewModel for creating/editing category.
    /// IMPORTANT: Enforces the 1-1-1 mechanism where each category must be linked
    /// to exactly one EvaluationBoard and one EvaluationTemplate.
    /// This enables automatic assignment of initiatives to boards and scoring templates.
    /// </summary>
    public class CategoryCreateEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Đợt sáng kiến")]
        public int PeriodId { get; set; }

        /// <summary>
        /// REQUIRED: The evaluation board that will score initiatives in this category.
        /// Part of the 1-1-1 mechanism for automatic assignment.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn Hội đồng đánh giá (bắt buộc theo quy tắc 1-1-1)")]
        public int? BoardId { get; set; }

        /// <summary>
        /// REQUIRED: The evaluation template (scoring criteria) for this category.
        /// Part of the 1-1-1 mechanism for automatic assignment.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn Mẫu chấm điểm (bắt buộc theo quy tắc 1-1-1)")]
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
