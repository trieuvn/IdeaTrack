using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Phân công chấm điểm - Liên kết giữa Sáng kiến và Thành viên Hội đồng
    /// Hỗ trợ cơ chế chấm đa vòng (Multi-round)
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
        
        // ===== HỖ TRỢ CHẤM ĐIỂM ĐA VÒNG =====
        /// <summary>
        /// Số thứ tự vòng chấm của bản ghi này
        /// Vòng 1, 2, 3... - Mỗi vòng tạo một bộ Assignment mới
        /// </summary>
        public int RoundNumber { get; set; } = 1;
        
        public DateTime AssignedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        
        /// <summary>
        /// Tên giai đoạn: "Vòng sơ loại Khoa", "Vòng chung khảo", v.v.
        /// </summary>
        public string StageName { get; set; } = "";
        
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
        
        // ===== LOGIC CHO CẤP KHOA (TemplateType = Screening) =====
        /// <summary>
        /// Quyết định: True = Approved, False = Rejected, null = Chưa quyết định
        /// </summary>
        public bool? Decision { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime? DecisionDate { get; set; }
        
        // Navigation
        public virtual ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
    }
}
