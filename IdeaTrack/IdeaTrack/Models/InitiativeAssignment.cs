using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE ASSIGNMENT - Phân công Đánh giá
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Links an Initiative to Board Members for evaluation.
    /// Supports multi-round evaluation (faculty screening, final scoring).
    /// 
    /// BUSINESS RULES:
    /// 1. Created automatically when OST approves an initiative
    /// 2. All Board members receive assignments (based on 1-1-1 mechanism)
    /// 3. Multi-round support: RoundNumber tracks evaluation rounds
    /// 4. Round 1 = Faculty Screening (Approve/Reject decision)
    /// 5. Round 2+ = Final Scoring (detailed criteria scoring)
    /// 
    /// RELATIONSHIPS:
    /// - Many Assignments belong to 1 Initiative (N:1)
    /// - Many Assignments reference 1 Board (N:1)
    /// - Many Assignments reference 1 Member/User (N:1)
    /// - Many Assignments reference 1 Template (N:1)
    /// - 1 Assignment has Many EvaluationDetails (1:N)
    /// 
    /// STATUS FLOW:
    /// Assigned → InProgress → Completed
    ///         → Overdue (if past DueDate)
    /// </summary>
    public class InitiativeAssignment
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the Initiative being evaluated
        /// </summary>
        public int InitiativeId { get; set; }
        
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; } = null!;
        
        /// <summary>
        /// Foreign key to the Board handling this evaluation.
        /// Nullable for legacy data.
        /// </summary>
        public int? BoardId { get; set; }
        
        [ForeignKey("BoardId")]
        public virtual Board? Board { get; set; }
        
        /// <summary>
        /// Foreign key to the User (evaluator/council member)
        /// </summary>
        public int MemberId { get; set; }
        
        [ForeignKey("MemberId")]
        public virtual ApplicationUser Member { get; set; } = null!;
        
        /// <summary>
        /// Foreign key to the EvaluationTemplate used for scoring
        /// </summary>
        public int TemplateId { get; set; }
        
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate Template { get; set; } = null!;
        
        // =====================================================
        // MULTI-ROUND EVALUATION SUPPORT
        // =====================================================
        
        /// <summary>
        /// Round number for this assignment.
        /// Round 1 = Faculty screening, Round 2+ = Final scoring.
        /// Each round creates a new set of assignments.
        /// </summary>
        public int RoundNumber { get; set; } = 1;
        
        /// <summary>
        /// Date when this assignment was created
        /// </summary>
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Optional deadline for completing the evaluation
        /// </summary>
        public DateTime? DueDate { get; set; }
        
        /// <summary>
        /// Human-readable stage name (e.g., "Vòng sơ loại Khoa", "Vòng chung khảo")
        /// </summary>
        public string StageName { get; set; } = "";
        
        /// <summary>
        /// Current status of this assignment
        /// </summary>
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
        
        // =====================================================
        // FACULTY SCREENING (TemplateType = Screening)
        // =====================================================
        
        /// <summary>
        /// Faculty leader's decision: true = Approved, false = Rejected, null = Pending
        /// </summary>
        public bool? Decision { get; set; }
        
        /// <summary>
        /// Comments/feedback from the evaluator
        /// </summary>
        public string? ReviewComment { get; set; }
        
        /// <summary>
        /// Date when decision was made
        /// </summary>
        public DateTime? DecisionDate { get; set; }
        
        // =====================================================
        // CHILD RELATIONSHIPS
        // =====================================================
        
        /// <summary>
        /// Individual criterion scores for this assignment (1:N)
        /// </summary>
        public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
    }
}
