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
        public string? Degree { get; set; } // Thạc sĩ, Tiến sĩ
        public string? Position { get; set; } // Trưởng khoa, Giảng viên
        public bool IsActive { get; set; } = true;
        public DateTime? LastActive { get; set; }

        // ===== NAVIGATION PROPERTIES =====
        
        /// <summary>
        /// Các sáng kiến mà user này là Creator (người tạo đầu tiên)
        /// </summary>
        public virtual ICollection<Initiative> MyInitiatives { get; set; } = new List<Initiative>();
        
        /// <summary>
        /// Các phân công chấm điểm của user này (khi là thành viên Hội đồng)
        /// </summary>
        public virtual ICollection<InitiativeAssignment> MyAssignments { get; set; } = new List<InitiativeAssignment>();
        
        /// <summary>
        /// Các sáng kiến mà user này tham gia với tư cách đồng tác giả
        /// </summary>
        public virtual ICollection<InitiativeAuthorship> Authorships { get; set; } = new List<InitiativeAuthorship>();
        
        /// <summary>
        /// Các hội đồng mà user này là thành viên
        /// </summary>
        public virtual ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
    }
}