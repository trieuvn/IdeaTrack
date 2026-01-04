using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Dot sang kien - Quan ly cac dot nop bai trong mot nam hoc
    /// Tai mot thoi diem chi co 1 dot duoc mo (IsActive = true)
    /// </summary>
    public class InitiativePeriod
    {
        public int Id { get; set; }
        public string Name { get; set; } = ""; // VD: "Dot 1 - 2024-2025"
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
