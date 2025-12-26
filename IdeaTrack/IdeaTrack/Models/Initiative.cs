using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class Initiative
    {
        public int Id { get; set; }
        public string InitiativeCode { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public decimal Budget { get; set; }
        public InitiativeStatus Status { get; set; } = InitiativeStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SubmittedDate { get; set; }
        public virtual FinalResult? FinalResult { get; set; }

        public int ProposerId { get; set; }
        [ForeignKey("ProposerId")]
        public virtual ApplicationUser Proposer { get; set; }
        
        // --- QUAN HỆ VỚI PHÒNG BAN/KHOA ---
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        
        // --- CÁC QUAN HỆ KHÁC ---
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual InitiativeCategory Category { get; set; }

        // Thêm quan hệ với Năm học
        public int AcademicYearId { get; set; }
        [ForeignKey("AcademicYearId")]
        public virtual AcademicYear AcademicYear { get; set; }

        public virtual ICollection<InitiativeFile> Files { get; set; } = new List<InitiativeFile>();
        public virtual ICollection<InitiativeAssignment> Assignments { get; set; } = new List<InitiativeAssignment>();
        public virtual ICollection<RevisionRequest> RevisionRequests { get; set; } = new List<RevisionRequest>();
    }
}
