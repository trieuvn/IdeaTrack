using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// EVALUATION DETAIL - Chi tiết Điểm từng Tiêu chí
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Stores individual criterion scores given by an evaluator for an assignment.
    /// One EvaluationDetail row per (Assignment × Criterion) combination.
    /// 
    /// BUSINESS RULES:
    /// 1. ScoreGiven must be between 0 and Criteria.MaxScore
    /// 2. Total assignment score = Sum of all EvaluationDetails.ScoreGiven
    /// 3. System calculates percentage automatically for ranking
    /// 
    /// RELATIONSHIPS:
    /// - Many Details belong to 1 Assignment (N:1)
    /// - Many Details reference 1 Criteria (N:1)
    /// 
    /// EXAMPLE:
    /// Assignment #5 (Member: Dr. Smith):
    /// - Criteria "Novelty" (max 30): ScoreGiven = 25
    /// - Criteria "Feasibility" (max 25): ScoreGiven = 20
    /// - Total: 45/55 = 81.8%
    /// </summary>
    public class EvaluationDetail
    {
        /// <summary>
        /// Primary key - Auto-incremented ID (long for large volume)
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Foreign key to the parent InitiativeAssignment
        /// </summary>
        public int AssignmentId { get; set; }
        
        /// <summary>
        /// Foreign key to the EvaluationCriteria being scored
        /// </summary>
        public int CriteriaId { get; set; }
        
        /// <summary>
        /// Actual score given by the evaluator (0 to MaxScore)
        /// </summary>
        public decimal ScoreGiven { get; set; }
        
        /// <summary>
        /// Optional notes/comments for this specific criterion
        /// </summary>
        public string? Note { get; set; }
        
        // =====================
        // Navigation Properties
        // =====================
        
        [ForeignKey("AssignmentId")]
        public virtual InitiativeAssignment Assignment { get; set; } = null!;
        
        [ForeignKey("CriteriaId")]
        public virtual EvaluationCriteria Criteria { get; set; } = null!;
    }
}
