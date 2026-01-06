namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// EVALUATION TEMPLATE - Bộ Tiêu chí Đánh giá
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Defines a scoring rubric/criteria set used by Boards to evaluate
    /// initiatives. Templates are reusable across multiple categories/periods.
    /// 
    /// BUSINESS RULES:
    /// 1. Each template contains multiple criteria (EvaluationCriteria)
    /// 2. Each criterion has a maximum score and optional weight
    /// 3. Templates are assigned to Categories via the 1-1-1 mechanism
    /// 4. The system calculates percentage scores: (Achieved / MaxPossible) * 100
    /// 5. This enables fair comparison across categories with different max scores
    /// 
    /// RELATIONSHIPS:
    /// - 1 Template has Many Criteria (1:N via EvaluationCriteria)
    /// - 1 Template is linked to Many Categories (1:N via Category.TemplateId)
    /// 
    /// EXAMPLE TEMPLATES:
    /// - "Technical Innovation Rubric" - 100 points total
    /// - "Software Project Rubric" - 50 points total
    /// - "Management Improvement Rubric" - 80 points total
    /// </summary>
    public class EvaluationTemplate
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Display name of the template (e.g., "Rubric Kỹ thuật Loại A")
        /// </summary>
        public string TemplateName { get; set; } = "";
        
        /// <summary>
        /// Type classification (e.g., Technical, Management, Software)
        /// </summary>
        public TemplateType Type { get; set; }
        
        /// <summary>
        /// Optional description of the template's purpose and scope
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Whether this template is currently active and available for assignment.
        /// Inactive templates are hidden from dropdowns but preserved for history.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // =====================
        // Navigation Properties
        // =====================
        
        /// <summary>
        /// Criteria list for this template (1-to-Many).
        /// Each criterion defines a specific aspect to evaluate with max points.
        /// Total score = Sum of all criteria max points.
        /// </summary>
        public virtual ICollection<EvaluationCriteria> CriteriaList { get; set; } = new List<EvaluationCriteria>();
    }
}
