using IdeaTrack.Models;

namespace IdeaTrack.Areas.Councils.Models
{
    public class AssignedListVM
    {
        public List<AssignedListItem> Items { get; set; } = new();

        public string? Keyword { get; set; }

        // All | Assigned | Completed
        public string Status { get; set; } = "Assigned";

        // Deadline | Newest
        public string SortOrder { get; set; } = "Deadline";

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class AssignedListItem
    {
        public int AssignmentId { get; set; }
        public int InitiativeId { get; set; }
        public string InitiativeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public AssignmentStatus Status { get; set; }

        public bool IsDueSoon => Status != AssignmentStatus.Completed
                                 && DueDate.HasValue
                                 && (DueDate.Value.Date - DateTime.Now.Date).TotalDays <= 7;
    }
}