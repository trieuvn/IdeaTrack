using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE - Sáng kiến
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// The central entity of the system representing an innovation proposal
    /// submitted by faculty members for evaluation and recognition.
    /// 
    /// BUSINESS RULES:
    /// 1. Each initiative is created by one primary author (Creator)
    /// 2. Multiple co-authors can be added via InitiativeAuthorship
    /// 3. Initiatives go through a workflow: Draft → Pending → Evaluating → Approved/Rejected
    /// 4. When submitted, the initiative is linked to a Period and Category
    /// 5. The Category determines which Board evaluates and which Template is used
    /// 
    /// STATUS WORKFLOW:
    /// - Draft: Initial state, being composed by author
    /// - Pending: Submitted by author, waiting for OST approval
    /// - Faculty_Approved: Approved by Faculty Leader (optional step)
    /// - Evaluating: Assigned to Board members for scoring
    /// - Re_Evaluating: Being scored again in a new round
    /// - Revision_Required: Author needs to revise content
    /// - Pending_Final: All scores collected, waiting for Chairman decision
    /// - Approved: Final decision - initiative recognized
    /// - Rejected: Final decision - initiative not recognized
    /// 
    /// RELATIONSHIPS:
    /// - 1 Initiative belongs to 1 Creator (N:1 via CreatorId)
    /// - 1 Initiative belongs to 1 Department (N:1 via DepartmentId)
    /// - 1 Initiative belongs to 1 Category (N:1 via CategoryId)
    /// - 1 Initiative optionally belongs to 1 Period (N:1 via PeriodId, null when Draft)
    /// - 1 Initiative has Many Authors (1:N via InitiativeAuthorship)
    /// - 1 Initiative has Many Files (1:N via InitiativeFile)
    /// - 1 Initiative has Many Assignments (1:N via InitiativeAssignment)
    /// - 1 Initiative has Many Revision Requests (1:N via RevisionRequest)
    /// - 1 Initiative has 0 or 1 FinalResult (1:0..1)
    /// </summary>
    public class Initiative
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Unique initiative code (e.g., "SK-2025-0001").
        /// Auto-generated when submitting.
        /// </summary>
        [Required]
        public string InitiativeCode { get; set; } = "";
        
        /// <summary>
        /// Title of the initiative - displayed in lists and reports
        /// </summary>
        [Required]
        public string Title { get; set; } = "";
        
        /// <summary>
        /// Detailed description of the initiative content
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Estimated budget/cost for implementing this initiative.
        /// Must be a positive number.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000000000, ErrorMessage = "Budget must be a valid positive number")]
        public decimal Budget { get; set; }
        
        /// <summary>
        /// Current status in the workflow.
        /// See InitiativeStatus enum for all possible values.
        /// </summary>
        public InitiativeStatus Status { get; set; } = InitiativeStatus.Draft;
        
        /// <summary>
        /// Record creation timestamp - when the initiative was first created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Date when the initiative was submitted for review.
        /// Null until the author submits (Draft → Pending transition).
        /// </summary>
        public DateTime? SubmittedDate { get; set; }
        
        // =====================================================
        // MULTI-ROUND EVALUATION SUPPORT
        // =====================================================
        
        /// <summary>
        /// Current evaluation round (default: 1).
        /// Incremented when OST requests re-evaluation.
        /// Allows tracking of multiple scoring attempts.
        /// </summary>
        public int CurrentRound { get; set; } = 1;
        
        // =====================================================
        // CREATOR RELATIONSHIP (Primary Author)
        // =====================================================
        
        /// <summary>
        /// Foreign key to the user who created this initiative.
        /// This is the primary author who has full edit rights.
        /// Retained for backwards compatibility.
        /// </summary>
        public int CreatorId { get; set; }
        
        [ForeignKey("CreatorId")]
        public virtual ApplicationUser Creator { get; set; } = null!;
        
        // =====================================================
        // DEPARTMENT RELATIONSHIP
        // =====================================================
        
        /// <summary>
        /// Foreign key to the department/faculty this initiative belongs to.
        /// Typically the department of the primary author.
        /// </summary>
        public int DepartmentId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;
        
        // =====================================================
        // CATEGORY RELATIONSHIP (Links to Board + Template)
        // =====================================================
        
        /// <summary>
        /// Foreign key to the initiative category.
        /// IMPORTANT: Via the 1-1-1 mechanism, the Category determines:
        /// - Which Board will evaluate this initiative
        /// - Which Template (scoring criteria) will be used
        /// </summary>
        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual InitiativeCategory Category { get; set; } = null!;
        
        // =====================================================
        // PERIOD RELATIONSHIP (Submission Window)
        // =====================================================
        
        /// <summary>
        /// Foreign key to the initiative period (submission window).
        /// NULL when the initiative is still in Draft status.
        /// Set automatically when the author submits the initiative.
        /// Period is determined by date: StartDate <= Today <= EndDate.
        /// </summary>
        public int? PeriodId { get; set; }
        
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod? Period { get; set; }
        
        // =====================================================
        // FINAL RESULT (One-to-One, Optional)
        // =====================================================
        
        /// <summary>
        /// Final evaluation result (appears after Chairman makes decision).
        /// Contains average score and final decision.
        /// </summary>
        public virtual FinalResult? FinalResult { get; set; }
        
        // =====================================================
        // NAVIGATION COLLECTIONS
        // =====================================================
        
        /// <summary>
        /// List of all authors (including co-authors).
        /// Uses many-to-many relationship via InitiativeAuthorship.
        /// The primary author is marked with IsCreator = true.
        /// </summary>
        public virtual ICollection<InitiativeAuthorship> Authorships { get; set; } = new List<InitiativeAuthorship>();
        
        /// <summary>
        /// Attached files (documents, images, etc.)
        /// </summary>
        public virtual ICollection<InitiativeFile> Files { get; set; } = new List<InitiativeFile>();
        
        /// <summary>
        /// Board member assignments for evaluation.
        /// Created automatically when OST approves the initiative.
        /// </summary>
        public virtual ICollection<InitiativeAssignment> Assignments { get; set; } = new List<InitiativeAssignment>();
        
        /// <summary>
        /// History of revision requests from OST or Board.
        /// </summary>
        public virtual ICollection<RevisionRequest> RevisionRequests { get; set; } = new List<RevisionRequest>();
    }
}
