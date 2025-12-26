using IdeaTrack.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public int? DepartmentId { get; set; }
    public virtual Department Department { get; set; }
    public string? AcademicRank { get; set; } // PGS, GS
    public string? Degree { get; set; } // Thạc sĩ, Tiến sĩ
    public string? Position { get; set; } // Trưởng khoa, Giảng viên
    public bool IsActive { get; set; } = true;
    public DateTime? LastActive { get; set; }

    // Navigation properties
    public virtual ICollection<Initiative> MyInitiatives { get; set; } = new List<Initiative>();
    public virtual ICollection<InitiativeAssignment> MyAssignments { get; set; } = new List<InitiativeAssignment>();
}