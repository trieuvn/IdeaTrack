using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// REVISION REQUEST - Yêu cầu Chỉnh sửa
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Tracks requests from OST/Admin for authors to revise their initiatives.
    /// Created when an initiative needs corrections before final approval.
    /// 
    /// BUSINESS RULES:
    /// 1. Created by OST staff or Admin when issues found
    /// 2. Initiative status changes to "Revision_Required"
    /// 3. Author receives notification and must address concerns
    /// 4. Optional deadline for completing revisions
    /// 5. IsResolved set to true when author resubmits
    /// 
    /// STATUS VALUES:
    /// - "Open": Waiting for author response
    /// - "In Progress": Author working on revisions
    /// - "Resolved": Author has resubmitted
    /// - "Closed": No further action needed
    /// 
    /// RELATIONSHIPS:
    /// - Many RevisionRequests belong to 1 Initiative (N:1)
    /// - Many RevisionRequests reference 1 Requester/User (N:1)
    /// </summary>
    public class RevisionRequest
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the Initiative needing revision
        /// </summary>
        public int InitiativeId { get; set; }
        
        /// <summary>
        /// Foreign key to the User who created this request (OST or Admin)
        /// </summary>
        public int RequesterId { get; set; }

        /// <summary>
        /// Detailed description of what needs to be revised
        /// </summary>
        public string RequestContent { get; set; } = "";
        
        /// <summary>
        /// Timestamp when the request was created
        /// </summary>
        public DateTime RequestedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Optional deadline for completing the revisions
        /// </summary>
        public DateTime? Deadline { get; set; }
        
        /// <summary>
        /// Whether the author has addressed this request
        /// </summary>
        public bool IsResolved { get; set; } = false;
        
        /// <summary>
        /// Current status: "Open", "In Progress", "Resolved", "Closed"
        /// </summary>
        public string Status { get; set; } = "Open";

        // =====================
        // Navigation Properties
        // =====================
        
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; } = null!;
        
        [ForeignKey("RequesterId")]
        public virtual ApplicationUser Requester { get; set; } = null!;
    }
}
