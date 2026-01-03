using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Areas.Author.ViewModels
{
    /// <summary>
    /// ViewModel for displaying co-author information
    /// </summary>
    public class CoAuthorViewModel
    {
        public int AuthorshipId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Department { get; set; }
        public bool IsCreator { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for managing co-authors on an initiative
    /// </summary>
    public class CoAuthorManageViewModel
    {
        public int InitiativeId { get; set; }
        public string InitiativeTitle { get; set; } = "";
        public string InitiativeCode { get; set; } = "";
        public bool CanEdit { get; set; } // True if initiative is draft/pending

        // Current co-authors
        public List<CoAuthorViewModel> CoAuthors { get; set; } = new();

        // Add new co-author
        [Display(Name = "Thêm đồng tác giả")]
        public int? NewCoAuthorId { get; set; }

        // Available users to add as co-authors
        public List<SelectListItem> AvailableUsers { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for submitting initiative with period/category selection
    /// </summary>
    public class SubmitInitiativeViewModel
    {
        public int InitiativeId { get; set; }
        public string InitiativeTitle { get; set; } = "";
        public string InitiativeCode { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng chọn đợt sáng kiến")]
        [Display(Name = "Đợt sáng kiến")]
        public int PeriodId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // Dropdowns
        public List<SelectListItem> Periods { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for progress timeline
    /// </summary>
    public class ProgressTimelineViewModel
    {
        public int InitiativeId { get; set; }
        public string InitiativeTitle { get; set; } = "";
        public string InitiativeCode { get; set; } = "";
        public string CurrentStatus { get; set; } = "";

        public List<ProgressStepViewModel> Steps { get; set; } = new();
    }

    /// <summary>
    /// A single step in the progress timeline
    /// </summary>
    public class ProgressStepViewModel
    {
        public string StepName { get; set; } = "";
        public string? Description { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public string IconClass { get; set; } = "fas fa-circle";
    }
}
