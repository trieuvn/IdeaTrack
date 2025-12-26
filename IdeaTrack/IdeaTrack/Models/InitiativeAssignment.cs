using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class InitiativeAssignment
    {
        public int Id { get; set; }
        public int InitiativeId { get; set; }
        public int? BoardId { get; set; } // Nếu có quản lý Hội đồng
        public int MemberId { get; set; }
        public int TemplateId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public string StageName { get; set; } // VD: "Vòng sơ loại Khoa", "Vòng chung khảo"
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;

        // Logic cho Cấp Khoa (TemplateType = Screening)
        public bool? Decision { get; set; } // True: Approve, False: Reject
        public string? ReviewComment { get; set; }
        public DateTime? DecisionDate { get; set; }

        // Navigation
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; }
        [ForeignKey("MemberId")]
        public virtual ApplicationUser Member { get; set; }
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate Template { get; set; }
        public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
    }
}
