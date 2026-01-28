using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Areas.SciTech.Models
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsActive { get; set; } = false;

        [Required]
        public string SelectedRole { get; set; }

        // Optional fields - no [Required] attribute
        public string? Position { get; set; }
        public string? Degree { get; set; }
        public string? AcademicRank { get; set; }
    }
}
