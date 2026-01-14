using IdeaTrack.Models;

namespace IdeaTrack.ViewModels
{
    /// <summary>
    /// ViewModel for the Privacy/Guidelines page with Year/Period filtering
    /// </summary>
    public class PrivacyViewModel
    {
        /// <summary>
        /// List of academic years that have at least one active/open period
        /// </summary>
        public List<AcademicYearOption> AvailableYears { get; set; } = new();

        /// <summary>
        /// List of active/open periods for the selected year
        /// </summary>
        public List<PeriodOption> AvailablePeriods { get; set; } = new();

        /// <summary>
        /// Reference forms (documents) for the selected period
        /// </summary>
        public List<ReferenceForm> ReferenceForms { get; set; } = new();

        /// <summary>
        /// Currently selected year ID
        /// </summary>
        public int? SelectedYearId { get; set; }

        /// <summary>
        /// Currently selected period ID
        /// </summary>
        public int? SelectedPeriodId { get; set; }

        /// <summary>
        /// Name of the selected period (for display)
        /// </summary>
        public string? SelectedPeriodName { get; set; }

        /// <summary>
        /// Information about the currently active period (for status card)
        /// </summary>
        public ActivePeriodInfo? ActivePeriodInfo { get; set; }
    }

    public class AcademicYearOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsCurrent { get; set; }
    }

    public class PeriodOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ActivePeriodInfo
    {
        public string PeriodName { get; set; } = "";
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsOpen { get; set; }
    }
}
