using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Danh mục sáng kiến - Phân loại theo từng đợt
    /// Mỗi danh mục liên kết với 1 Hội đồng và 1 Bộ tiêu chí (cơ chế 1-1-1)
    /// </summary>
    public class InitiativeCategory
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = ""; // VD: Cải tiến kỹ thuật, Phần mềm, Quản lý
        
        public string? Description { get; set; }
        
        // ===== LIÊN KẾT VỚI ĐỢT SÁNG KIẾN =====
        public int PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod Period { get; set; } = null!;
        
        // ===== CƠ CHẾ 1-1-1: MỖI DANH MỤC = 1 HỘI ĐỒNG + 1 BỘ TIÊU CHÍ =====
        
        /// <summary>
        /// Hội đồng cố định chấm danh mục này
        /// </summary>
        public int? BoardId { get; set; }
        [ForeignKey("BoardId")]
        public virtual Board? Board { get; set; }
        
        /// <summary>
        /// Bộ tiêu chí dùng để chấm danh mục này
        /// </summary>
        public int? TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate? Template { get; set; }
        
        // Navigation
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
