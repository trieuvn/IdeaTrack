namespace IdeaTrack.Areas.SciTech.Models
{
    /// <summary>
    /// ViewModel for displaying and managing InitiativePeriod
    /// </summary>
    public class PeriodViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string AcademicYearName { get; set; } = "";
        public int AcademicYearId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryCount { get; set; }
        public int InitiativeCount { get; set; }
    }

    /// <summary>
    /// ViewModel for creating/editing InitiativePeriod
    /// </summary>
    public class PeriodCreateEditVM
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = "";
        
        public int AcademicYearId { get; set; }
        
        public DateTime StartDate { get; set; } = DateTime.Today;
        
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);
        
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Optional: Clone categories from this period when creating new one
        /// </summary>
        public int? CloneFromPeriodId { get; set; }
        
        // For dropdowns
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AcademicYears { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> ExistingPeriods { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for listing periods
    /// </summary>
    public class PeriodListVM
    {
        public List<PeriodViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
