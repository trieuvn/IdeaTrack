using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// ACADEMIC YEAR - Năm học
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// The largest time container in the system. All initiative periods,
    /// submissions, and evaluations are organized within academic years.
    /// 
    /// BUSINESS RULES:
    /// 1. Only ONE academic year can be marked as "Current" (IsCurrent = true) at a time
    /// 2. Each academic year contains multiple initiative periods
    /// 3. Historical years are preserved for reporting and auditing
    /// 4. Typically spans from September to August of the following year
    /// 
    /// NAMING CONVENTION:
    /// Format: "Năm học YYYY-YYYY" (e.g., "Năm học 2025-2026")
    /// 
    /// RELATIONSHIPS:
    /// - 1 AcademicYear has Many InitiativePeriods (1:N)
    /// </summary>
    public class AcademicYear
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Display name of the academic year.
        /// Format: "Năm học YYYY-YYYY" (e.g., "Năm học 2025-2026")
        /// </summary>
        [Required]
        public string Name { get; set; } = "";
        
        /// <summary>
        /// Flag indicating if this is the currently active academic year.
        /// IMPORTANT: Only ONE academic year should have IsCurrent = true.
        /// This is used for default filtering in dropdowns and dashboards.
        /// </summary>
        public bool IsCurrent { get; set; }
        
        /// <summary>
        /// Record creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // =====================================================
        // NAVIGATION PROPERTIES
        // =====================================================
        
        /// <summary>
        /// Initiative periods within this academic year (1-to-Many).
        /// Each period has its own start/end dates for submissions.
        /// Multiple periods can be OPEN simultaneously based on date logic.
        /// </summary>
        public virtual ICollection<InitiativePeriod> Periods { get; set; } = new List<InitiativePeriod>();
    }
}
