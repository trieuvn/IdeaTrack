using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;

namespace IdeaTrack.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = "";
        public string? AvatarUrl { get; set; }
        
        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }
        
        public string? AcademicRank { get; set; } // PGS, GS
        public string? Degree { get; set; } // Thac si, Tien si
        public string? Position { get; set; } // Truong khoa, Giang vien
        public bool IsActive { get; set; } = true;
        public DateTime? LastActive { get; set; }

        // ===== NAVIGATION PROPERTIES =====
        
        /// <summary>
        /// Cac sang kien ma user nay la Creator (nguoi tao dau tien)
        /// </summary>
        public virtual ICollection<Initiative> MyInitiatives { get; set; } = new List<Initiative>();
        
        /// <summary>
        /// Cac phan cong cham diem cua user nay (khi la thanh vien Hoi dong)
        /// </summary>
        public virtual ICollection<InitiativeAssignment> MyAssignments { get; set; } = new List<InitiativeAssignment>();
        
        /// <summary>
        /// Cac sang kien ma user nay tham gia voi tu cach dong tac gia
        /// </summary>
        public virtual ICollection<InitiativeAuthorship> Authorships { get; set; } = new List<InitiativeAuthorship>();
        
        /// <summary>
        /// Cac hoi dong ma user nay la thanh vien
        /// </summary>
        public virtual ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
    }
}