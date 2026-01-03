using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Đợt sáng kiến - Quản lý các đợt nộp bài trong một năm học
    /// Tại một thời điểm chỉ có 1 đợt được mở (IsActive = true)
    /// </summary>
    public class InitiativePeriod
    {
        public int Id { get; set; }
        public string Name { get; set; } = ""; // VD: "Đợt 1 - 2024-2025"
        public string? Description { get; set; }
        
        public int AcademicYearId { get; set; }
        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<InitiativeCategory> Categories { get; set; } = new List<InitiativeCategory>();
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
        public virtual ICollection<ReferenceForm> ReferenceForms { get; set; } = new List<ReferenceForm>();
    }
}
