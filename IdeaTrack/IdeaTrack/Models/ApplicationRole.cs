using Microsoft.AspNetCore.Identity;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// APPLICATION ROLE - Vai trò Ứng dụng
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Extends ASP.NET Core Identity role with custom properties.
    /// Defines authorization roles for the IdeaTrack system.
    /// 
    /// AVAILABLE ROLES:
    /// - Admin: System administrator, full access
    /// - SciTech: Office of Science & Technology staff
    /// - FacultyLeader: Department/Faculty leader for initial review
    /// - CouncilMember: Evaluation board member
    /// - Lecturer: Teaching staff, can author initiatives
    /// - Author: Can create and submit initiatives
    /// 
    /// RELATIONSHIPS:
    /// - Managed by ASP.NET Core Identity system
    /// - Users can have multiple roles
    /// </summary>
    public class ApplicationRole : IdentityRole<int>
    {
        /// <summary>
        /// Human-readable description of what this role can do
        /// </summary>
        public string Description { get; set; } = "";
    }
}
