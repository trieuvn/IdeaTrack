using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE AUTHORSHIP - Đồng tác giả Sáng kiến
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Junction table supporting many-to-many relationship between
    /// Initiatives and Users (authors). Enables co-authorship.
    /// 
    /// BUSINESS RULES:
    /// 1. One Initiative can have multiple authors (N:N via this table)
    /// 2. The creator (IsCreator = true) cannot be removed from the initiative
    /// 3. All co-authors can view and edit the initiative
    /// 4. When initiative is approved, all authors receive credit/benefits
    /// 5. Co-authors are added by the creator before submission
    /// 
    /// RELATIONSHIPS:
    /// - Many Authorships link 1 Initiative (N:1)
    /// - Many Authorships link 1 User/Author (N:1)
    /// 
    /// EXAMPLE:
    /// Initiative "AI Attendance System":
    /// - Dr. Smith (IsCreator = true, JoinedAt = 2025-01-01)
    /// - Prof. Jones (IsCreator = false, JoinedAt = 2025-01-05)
    /// - Ms. Lee (IsCreator = false, JoinedAt = 2025-01-05)
    /// </summary>
    public class InitiativeAuthorship
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the Initiative
        /// </summary>
        public int InitiativeId { get; set; }
        
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; } = null!;
        
        /// <summary>
        /// Foreign key to the User (author)
        /// </summary>
        public int AuthorId { get; set; }
        
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; } = null!;
        
        /// <summary>
        /// Marks the original creator of the initiative.
        /// The creator cannot be removed and has full permissions.
        /// </summary>
        public bool IsCreator { get; set; } = false;
        
        /// <summary>
        /// Timestamp when this author joined the initiative
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
