namespace IdeaTrack.Areas.Faculty.Models
{
    /// <summary>
    /// ViewModel for Faculty Leader Profile page (view only)
    /// </summary>
    public class FacultyProfileVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }
        public string? AcademicRank { get; set; }
        public string? Degree { get; set; }
        public string? DepartmentName { get; set; }
        public string? AvatarUrl { get; set; }
        
        // Statistics
        public int InitiativeCount { get; set; }
        public int AchievementCount { get; set; }
    }

    /// <summary>
    /// ViewModel for editing user profile
    /// </summary>
    public class ProfileEditVM
    {
        public int UserId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Full name is required")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Invalid email address")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Email")]
        public string Email { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Phone(ErrorMessage = "Invalid phone number")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Display(Name = "Position")]
        public string Position { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Display(Name = "Academic Rank")]
        public string AcademicRank { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Display(Name = "Degree")]
        public string Degree { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Display(Name = "Avatar URL")]
        public string AvatarUrl { get; set; } = "";
    }
}
