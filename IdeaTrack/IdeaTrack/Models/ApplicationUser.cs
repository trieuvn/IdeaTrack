using Microsoft.AspNetCore.Identity;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// APPLICATION USER - Người dùng hệ thống
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Extends ASP.NET Identity for user management with additional profile
    /// information specific to the academic institution context.
    /// 
    /// USER ROLES:
    /// - Author: Faculty members who create and submit initiatives
    /// - Faculty: Department leaders who can pre-approve initiatives
    /// - Council: Board members who evaluate and score initiatives
    /// - OST: Office of Science & Technology staff (system administrators)
    /// 
    /// RELATIONSHIPS:
    /// - 1 User belongs to 0..1 Department (N:1 via DepartmentId)
    /// - 1 User has Many Initiatives as Creator (1:N via CreatorId)
    /// - 1 User has Many Authorships (1:N via InitiativeAuthorship)
    /// - 1 User has Many Board Memberships (1:N via BoardMember)
    /// - 1 User has Many Assignments for evaluation (1:N via InitiativeAssignment)
    /// </summary>
    public class ApplicationUser : IdentityUser<int>
    {
        /// <summary>
        /// Full display name of the user (e.g., "Nguyễn Văn A")
        /// </summary>
        public string FullName { get; set; } = "";
        
        /// <summary>
        /// URL to user's profile picture (optional)
        /// </summary>
        public string? AvatarUrl { get; set; }
        
        /// <summary>
        /// Foreign key to the department/faculty this user belongs to
        /// Nullable for system administrators who may not belong to a department
        /// </summary>
        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }
        
        /// <summary>
        /// Academic rank: "PGS" (Associate Professor), "GS" (Professor), etc.
        /// </summary>
        public string? AcademicRank { get; set; }
        
        /// <summary>
        /// Academic degree: "ThS" (Master), "TS" (PhD), "TSKH" (Doctor of Science)
        /// </summary>
        public string? Degree { get; set; }
        
        /// <summary>
        /// Job position: "Giảng viên", "Trưởng khoa", "Phó khoa", etc.
        /// </summary>
        public string? Position { get; set; }
        
        /// <summary>
        /// Account active status. Inactive users cannot login.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Timestamp of user's last activity for tracking purposes
        /// </summary>
        public DateTime? LastActive { get; set; }

        // =====================================================
        // NAVIGATION PROPERTIES
        // =====================================================
        
        /// <summary>
        /// Initiatives where this user is the primary creator/author
        /// </summary>
        public virtual ICollection<Initiative> MyInitiatives { get; set; } = new List<Initiative>();
        
        /// <summary>
        /// Evaluation assignments for this user (when serving as Board member)
        /// </summary>
        public virtual ICollection<InitiativeAssignment> MyAssignments { get; set; } = new List<InitiativeAssignment>();
        
        /// <summary>
        /// Initiatives where this user participates as co-author
        /// </summary>
        public virtual ICollection<InitiativeAuthorship> Authorships { get; set; } = new List<InitiativeAuthorship>();
        
        /// <summary>
        /// Board memberships (which evaluation boards this user belongs to)
        /// </summary>
        public virtual ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
    }
}