using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// SYSTEM AUDIT LOG - Nhật ký Thao tác Hệ thống
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Tracks all significant user actions for auditing and compliance.
    /// Stores who did what, when, and from where.
    /// 
    /// BUSINESS RULES:
    /// 1. Automatically created for sensitive operations
    /// 2. Cannot be modified or deleted (append-only)
    /// 3. Used for security audits and dispute resolution
    /// 4. Details field stores JSON of changed values
    /// 
    /// TRACKED ACTIONS:
    /// - "Login" / "Logout"
    /// - "Create" / "Update" / "Delete"
    /// - "Approve" / "Reject"
    /// - "Score" / "FinalDecision"
    /// - "AssignBoard" / "RemoveMember"
    /// 
    /// RELATIONSHIPS:
    /// - Many Logs reference 1 User (N:1)
    /// </summary>
    public class SystemAuditLog
    {
        /// <summary>
        /// Primary key - Auto-incremented ID (long for high volume)
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Foreign key to the User who performed the action
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Action performed (e.g., "Approve", "Update Score", "Delete")
        /// </summary>
        public string Action { get; set; } = "";
        
        /// <summary>
        /// Database table affected (e.g., "Initiatives", "Boards")
        /// </summary>
        public string TargetTable { get; set; } = "";
        
        /// <summary>
        /// Primary key of the affected record
        /// </summary>
        public int TargetId { get; set; }
        
        /// <summary>
        /// JSON string with before/after values for detailed change tracking
        /// </summary>
        public string? Details { get; set; }
        
        /// <summary>
        /// Timestamp when the action occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        /// <summary>
        /// IP address from which the action was performed
        /// </summary>
        public string IpAddress { get; set; } = "";

        // =====================
        // Navigation Property
        // =====================
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
