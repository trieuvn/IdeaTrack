namespace IdeaTrack.Areas.Author.ViewModels
{
    using IdeaTrack.Models;

    public class DailyReportViewModel
    {
        public int TotalInitiatives { get; set; }
        public List<CategoryDistribution> CategoryDistributions { get; set; } = new();
        public List<Initiative> TodaySubmissions { get; set; } = new();
        public List<Initiative> RecentSubmissions { get; set; } = new();
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        // Monthly stats
        public int MonthlyTotal { get; set; }
        public int MonthlyApproved { get; set; }
        public int MonthlyPending { get; set; }
    }

    public class CategoryDistribution
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#3b82f6";
    }
}
