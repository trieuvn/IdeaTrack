namespace IdeaTrack.Areas.Author.ViewModels
{
    using IdeaTrack.Models;

    public class AuthorDashboardViewModel
    {
        public string UserName { get; set; } = "John";
        public List<Initiative> RecentInitiatives { get; set; } = new();
        public int DraftCount { get; set; }
        public int SubmittedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int UnderReviewCount { get; set; }
        public decimal TotalScore { get; set; }
        public decimal SuccessRate { get; set; }
    }
}
