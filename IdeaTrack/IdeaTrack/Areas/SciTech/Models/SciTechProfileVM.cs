namespace IdeaTrack.Areas.SciTech.Models
{
    public class SciTechProfileVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }
        public string? AcademicRank { get; set; }
        public string? Degree { get; set; }
        public string? AvatarUrl { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }

        // Additional info matching the provided sample design
        public string StartDate { get; set; } = "";
        public string TenureStatus { get; set; } = "";
        public string TeacherTitle { get; set; } = "";
        
        public string DateOfBirth { get; set; } = "";
        public string BirthPlace { get; set; } = "";
        public string Gender { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string Nationality { get; set; } = "";
        public string Religion { get; set; } = "";
        public string Ethnicity { get; set; } = "";
        public string ContactAddress { get; set; } = "";
    }
}
