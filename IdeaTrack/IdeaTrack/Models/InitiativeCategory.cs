using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE CATEGORY - Danh mục Sáng kiến
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Classifies initiatives within a period (e.g., "Technical Innovation", 
    /// "Software Development", "Management Improvement").
    /// 
    /// CRITICAL BUSINESS RULE: 1-1-1 MECHANISM
    /// -------------------------------------------------------
    /// Each category MUST be linked to exactly:
    /// - 1 Board (EvaluationBoard) - The committee that will evaluate this category
    /// - 1 Template (EvaluationTemplate) - The scoring criteria used for evaluation
    /// 
    /// This enables AUTOMATIC ASSIGNMENT: When OST approves an initiative,
    /// the system automatically assigns the linked Board and Template based
    /// on the category the author selected during submission.
    /// 
    /// RELATIONSHIPS:
    /// - Many Categories belong to 1 Period (N:1)
    /// - 1 Category links to 1 Board (1:1)
    /// - 1 Category links to 1 Template (1:1)
    /// - 1 Category has Many Initiatives (1:N)
    /// 
    /// CLONE BEHAVIOR:
    /// When a new period is created and cloned from a previous period,
    /// categories are copied with their Board/Template links preserved.
    /// Admin only needs to update if there are personnel or criteria changes.
    /// </summary>
    public class InitiativeCategory
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Category name (e.g., "Cải tiến kỹ thuật", "Phần mềm", "Quản lý")
        /// Required field.
        /// </summary>
        [Required]
        public string Name { get; set; } = "";
        
        /// <summary>
        /// Optional description of what this category covers
        /// </summary>
        public string? Description { get; set; }
        
        // =====================================================
        // PARENT RELATIONSHIP: Links to Initiative Period
        // =====================================================
        
        /// <summary>
        /// Foreign key to the parent period.
        /// Each category belongs to exactly one period.
        /// </summary>
        public int PeriodId { get; set; }
        
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod Period { get; set; } = null!;
        
        // =====================================================
        // 1-1-1 MECHANISM: Board + Template Assignment
        // =====================================================
        
        /// <summary>
        /// Foreign key to the EvaluationBoard (Council).
        /// This board will automatically be assigned to evaluate initiatives
        /// submitted to this category when OST approves them.
        /// 
        /// NULLABLE for initial setup, but SHOULD be set before the period opens.
        /// </summary>
        public int? BoardId { get; set; }
        
        [ForeignKey("BoardId")]
        public virtual Board? Board { get; set; }
        
        /// <summary>
        /// Foreign key to the EvaluationTemplate (Scoring Criteria).
        /// This template defines the scoring rubric used by the Board
        /// when evaluating initiatives in this category.
        /// 
        /// NULLABLE for initial setup, but SHOULD be set before the period opens.
        /// </summary>
        public int? TemplateId { get; set; }
        
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate? Template { get; set; }
        
        // =====================================================
        // CHILD RELATIONSHIPS
        // =====================================================
        
        /// <summary>
        /// Initiatives submitted to this category (1-to-Many)
        /// </summary>
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
