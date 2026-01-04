using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Categories sang kien - Phan loai theo tung dot
    /// Moi danh muc lien ket voi 1 Hoi dong va 1 Evaluation Templates (co che 1-1-1)
    /// </summary>
    public class InitiativeCategory
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = ""; // VD: Cai tien ky thuat, Phan mem, Quan ly
        
        public string? Description { get; set; }
        
        // ===== LIEN KET VOI DOT SANG KIEN =====
        public int PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod Period { get; set; } = null!;
        
        // ===== CO CHE 1-1-1: MOI DANH MUC = 1 HOI DONG + 1 BO TIEU CHI =====
        
        /// <summary>
        /// Hoi dong co dinh cham danh muc nay
        /// </summary>
        public int? BoardId { get; set; }
        [ForeignKey("BoardId")]
        public virtual Board? Board { get; set; }
        
        /// <summary>
        /// Evaluation Templates dung de cham danh muc nay
        /// </summary>
        public int? TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate? Template { get; set; }
        
        // Navigation
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
