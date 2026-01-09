namespace IdeaTrack.ViewModels
{
    /// <summary>
    /// ViewModel for displaying top initiatives on the leaderboard
    /// </summary>
    public class LeaderboardInitiativeItem
    {
        public int Rank { get; set; }
        public string InitiativeCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal ScorePercent { get; set; }
        public string RankLabel { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for displaying top lecturers on the leaderboard
    /// </summary>
    public class LeaderboardLecturerItem
    {
        public int Rank { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int ApprovedCount { get; set; }
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// Combined leaderboard data for HomePage display
    /// </summary>
    public class LeaderboardVM
    {
        public List<LeaderboardInitiativeItem> TopInitiatives { get; set; } = new();
        public List<LeaderboardLecturerItem> TopLecturers { get; set; } = new();
        public string? CurrentPeriodName { get; set; }
        public string? CurrentAcademicYear { get; set; }
    }
}
