using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE PERIOD - Đợt Sáng kiến
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Represents a submission period within an academic year during which
    /// authors can submit initiatives for evaluation.
    /// 
    /// BUSINESS RULES:
    /// 1. A period is considered "Open" for submissions if:
    ///    StartDate <= CurrentDate <= EndDate
    /// 2. Multiple periods CAN be open simultaneously (parallel submission windows)
    /// 3. Each period contains multiple categories (1-to-Many)
    /// 4. The IsActive flag is a legacy manual toggle; prefer date-based logic
    /// 
    /// RELATIONSHIPS:
    /// - Many Periods belong to 1 AcademicYear (N:1)
    /// - 1 Period has Many Categories (1:N)
    /// - 1 Period has Many Initiatives (1:N)
    /// - 1 Period has Many ReferenceForms (1:N)
    /// 
    /// CLONE FEATURE:
    /// When creating a new period, admin can clone categories from a previous
    /// period to reduce data entry effort.
    /// </summary>
    public class InitiativePeriod
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Display name of the period (e.g., "Đợt 1 - 2024-2025")
        /// </summary>
        public string Name { get; set; } = "";
        
        /// <summary>
        /// Optional description or notes about this period
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Foreign key to the parent academic year
        /// </summary>
        public int AcademicYearId { get; set; }
        
        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
        
        /// <summary>
        /// Start date of the submission window.
        /// Period is "Open" when: StartDate <= Today <= EndDate
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date of the submission window.
        /// Period automatically closes after this date.
        /// </summary>
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Legacy manual activation flag.
        /// IMPORTANT: Use date-based logic (StartDate/EndDate) for determining
        /// if a period is open for submissions. This flag is for admin override.
        /// </summary>
        public bool IsActive { get; set; } = false;
        
        /// <summary>
        /// Record creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // =====================
        // Navigation Properties
        // =====================
        
        /// <summary>
        /// Categories available in this period (1-to-Many).
        /// Each category links to exactly 1 Board + 1 Template (1-1-1 mechanism).
        /// </summary>
        public virtual ICollection<InitiativeCategory> Categories { get; set; } = new List<InitiativeCategory>();
        
        /// <summary>
        /// Initiatives submitted to this period (1-to-Many)
        /// </summary>
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
        
        /// <summary>
        /// Reference forms (templates, guidelines) for this period (1-to-Many).
        /// These are typically cloned from the previous period.
        /// </summary>
        public virtual ICollection<ReferenceForm> ReferenceForms { get; set; } = new List<ReferenceForm>();
    }
}
