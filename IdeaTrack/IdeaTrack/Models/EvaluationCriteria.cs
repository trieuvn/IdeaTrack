using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// EVALUATION CRITERIA - Tiêu chí Đánh giá
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Defines individual scoring criteria within an EvaluationTemplate.
    /// Each criterion has a name, max score, and optional description.
    /// 
    /// BUSINESS RULES:
    /// 1. Template's total score = Sum of all criteria MaxScore values
    /// 2. Evaluators score each criterion from 0 to MaxScore
    /// 3. SortOrder determines display order in evaluation forms
    /// 4. Percentage calculation: (AchievedScore / MaxScore) * 100
    /// 
    /// RELATIONSHIPS:
    /// - Many Criteria belong to 1 Template (N:1)
    /// - EvaluationDetails reference Criteria for actual scores
    /// 
    /// EXAMPLE:
    /// Template "Technical Innovation" (100 points total):
    /// - Novelty (30 pts)
    /// - Feasibility (25 pts)
    /// - Economic Impact (25 pts)
    /// - Documentation (20 pts)
    /// </summary>
    public class EvaluationCriteria
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the parent EvaluationTemplate
        /// </summary>
        public int TemplateId { get; set; }
        
        /// <summary>
        /// Display name of the criterion (e.g., "Tính mới", "Tính khả thi")
        /// </summary>
        public string CriteriaName { get; set; } = "";
        
        /// <summary>
        /// Optional detailed description of what this criterion measures
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Maximum score achievable for this criterion.
        /// Evaluators score from 0 to this value.
        /// </summary>
        public decimal MaxScore { get; set; }
        
        /// <summary>
        /// Order in which criteria appear in evaluation forms.
        /// Lower values appear first.
        /// </summary>
        public int SortOrder { get; set; }
        
        // =====================
        // Navigation Properties
        // =====================
        
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate Template { get; set; } = null!;
    }
}
