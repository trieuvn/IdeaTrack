using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the main leaderboard page
    /// </summary>
    public class LeaderboardViewModel
    {
        public List<TopInitiativeItem> TopInitiatives { get; set; } = new();
        public List<TopAuthorItem> TopAuthors { get; set; } = new();
        public string? CurrentPeriodName { get; set; }
        public int? SelectedPeriodId { get; set; }
        public List<PeriodSelectItem> Periods { get; set; } = new();
    }

    /// <summary>
    /// Item for top initiatives leaderboard
    /// </summary>
    public class TopInitiativeItem
    {
        public int Rank { get; set; }
        public int InitiativeId { get; set; }
        public string InitiativeCode { get; set; } = "";
        public string Title { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public decimal AverageScore { get; set; }
        public string? FinalDecision { get; set; }
        public InitiativeStatus Status { get; set; }
    }

    /// <summary>
    /// Item for top authors leaderboard
    /// </summary>
    public class TopAuthorItem
    {
        public int Rank { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public string? DepartmentName { get; set; }
        public int TotalInitiatives { get; set; }
        public int ApprovedCount { get; set; }
        public decimal TotalScore { get; set; }
        public decimal AverageScore { get; set; }
    }

    /// <summary>
    /// Period selection dropdown item
    /// </summary>
    public class PeriodSelectItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel for reference forms download page
    /// </summary>
    public class ReferenceFormsDownloadViewModel
    {
        public int? SelectedPeriodId { get; set; }
        public string? PeriodName { get; set; }
        public List<PeriodSelectItem> Periods { get; set; } = new();
        public List<ReferenceFormDownloadItem> Forms { get; set; } = new();
    }

    /// <summary>
    /// Reference form item for download
    /// </summary>
    public class ReferenceFormDownloadItem
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string? Description { get; set; }
        public string FileUrl { get; set; } = "";
        public string FileType { get; set; } = "";
        public DateTime UploadedAt { get; set; }
    }
}
