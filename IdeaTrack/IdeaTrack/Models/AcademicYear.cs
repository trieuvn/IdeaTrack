using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Nam hoc - Khung thoi gian lon nhat trong he thong
    /// </summary>
    public class AcademicYear
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = ""; // VD: "2024-2025"
        
        /// <summary>
        /// Danh dau nam hoc hien tai
        /// </summary>
        public bool IsCurrent { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation - Mot nam hoc co nhieu dot sang kien
        public virtual ICollection<InitiativePeriod> Periods { get; set; } = new List<InitiativePeriod>();
    }
}
