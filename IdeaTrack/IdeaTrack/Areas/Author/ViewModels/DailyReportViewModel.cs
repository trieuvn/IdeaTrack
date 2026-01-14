using IdeaTrack.Models;

namespace IdeaTrack.Areas.Author.ViewModels
{
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

        // Filters
        public InitiativeStatus? SelectedStatus { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedYearId { get; set; }
        public int? SelectedPeriodId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Filter Lists
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Statuses { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Categories { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Years { get; set; }
        public Microsoft.AspNetCore.Mvc.Rendering.SelectList? Periods { get; set; }
    }

    public class CategoryDistribution
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#3b82f6";
    }
}
