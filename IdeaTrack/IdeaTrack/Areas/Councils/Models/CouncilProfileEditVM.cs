using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Areas.Councils.Models
{
    public class CouncilProfileEditVM
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Chức vụ")]
        public string? Position { get; set; }

        [Display(Name = "Học hàm")]
        public string? AcademicRank { get; set; }

        [Display(Name = "Học vị")]
        public string? Degree { get; set; }
    }
}
