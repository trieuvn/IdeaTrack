namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// DEPARTMENT - Khoa/Phòng ban
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Represents organizational units (faculties, departments) within the institution.
    /// Used for grouping users and initiatives for reporting and statistics.
    /// 
    /// RELATIONSHIPS:
    /// - 1 Department has Many Users (1:N via DepartmentId in ApplicationUser)
    /// - 1 Department has Many Initiatives (1:N via DepartmentId in Initiative)
    /// 
    /// USE CASES:
    /// - Faculty-based statistics and leaderboards
    /// - Filtering initiatives by department
    /// - Organizational hierarchy display
    /// </summary>
    public class Department
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Full name of the department (e.g., "Khoa Công nghệ Thông tin")
        /// </summary>
        public string Name { get; set; } = "";
        
        /// <summary>
        /// Short code for the department (e.g., "CNTT", "KTXD")
        /// Used for compact display and reporting
        /// </summary>
        public string Code { get; set; } = "";

        // =====================================================
        // NAVIGATION PROPERTIES
        // =====================================================
        
        /// <summary>
        /// Staff members belonging to this department (1-to-Many)
        /// </summary>
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        /// <summary>
        /// Initiatives submitted from this department (1-to-Many)
        /// Used for department-level statistics and competitions
        /// </summary>
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
