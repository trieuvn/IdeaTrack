using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Phan cong cham diem - Lien ket giua Sang kien va Member Hoi dong
    /// Ho tro co che cham da vong (Multi-round)
    /// </summary>
    public class InitiativeAssignment
    {
        public int Id { get; set; }
        
        public int InitiativeId { get; set; }
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; } = null!;
        
        public int? BoardId { get; set; }
        [ForeignKey("BoardId")]
        public virtual Board? Board { get; set; }
        
        public int MemberId { get; set; }
        [ForeignKey("MemberId")]
        public virtual ApplicationUser Member { get; set; } = null!;
        
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate Template { get; set; } = null!;
        
        // ===== HO TRO CHAM DIEM DA VONG =====
        /// <summary>
        /// So thu tu vong cham cua ban ghi nay
        /// Vong 1, 2, 3... - Moi vong tao mot bo Assignment moi
        /// </summary>
        public int RoundNumber { get; set; } = 1;
        
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        
        /// <summary>
        /// Name giai doan: "Vong so loai Khoa", "Vong chung khao", v.v.
        /// </summary>
        public string StageName { get; set; } = "";
        
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
        
        // ===== LOGIC CHO CAP KHOA (TemplateType = Screening) =====
        /// <summary>
        /// Quyet dinh: True = Approved, False = Rejected, null = Chua quyet dinh
        /// </summary>
        public bool? Decision { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime? DecisionDate { get; set; }
        
        // Navigation
        public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
    }
}
