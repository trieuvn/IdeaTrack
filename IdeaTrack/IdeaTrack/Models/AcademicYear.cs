using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Năm học - Khung thời gian lớn nhất trong hệ thống
    /// </summary>
    public class AcademicYear
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = ""; // VD: "2024-2025"
        
        /// <summary>
        /// Đánh dấu năm học hiện tại
        /// </summary>
        public bool IsCurrent { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation - Một năm học có nhiều đợt sáng kiến
        public virtual ICollection<InitiativePeriod> Periods { get; set; } = new List<InitiativePeriod>();
    }
}
