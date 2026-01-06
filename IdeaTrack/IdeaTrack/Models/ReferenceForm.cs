using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// REFERENCE FORM - Biểu mẫu Tham khảo
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Stores reference documents (templates, guidelines, forms) for each
    /// initiative period. Authors download these when preparing submissions.
    /// 
    /// BUSINESS RULES:
    /// 1. Each period can have multiple reference forms (1:N)
    /// 2. Actual files stored in cloud storage (Supabase/Azure)
    /// 3. Database only stores metadata and URL references
    /// 4. When cloning a period, reference forms are copied too
    /// 
    /// RELATIONSHIPS:
    /// - Many ReferenceForms belong to 1 Period (N:1)
    /// 
    /// EXAMPLE FILES:
    /// - "Mẫu Đề xuất Sáng kiến.docx"
    /// - "Hướng dẫn Viết Báo cáo.pdf"
    /// - "Bảng Tiêu chí Đánh giá.xlsx"
    /// </summary>
    public class ReferenceForm
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Display name shown to users (e.g., "Mẫu Đề xuất")
        /// </summary>
        public string FormName { get; set; } = "";
        
        /// <summary>
        /// Full URL to file in cloud storage (Supabase/Azure)
        /// </summary>
        public string FileUrl { get; set; } = "";
        
        /// <summary>
        /// Original filename as uploaded
        /// </summary>
        public string? FileName { get; set; }
        
        /// <summary>
        /// File type/extension (e.g., "docx", "pdf", "xlsx")
        /// </summary>
        public string? FileType { get; set; }
        
        /// <summary>
        /// Optional description of what this form is used for
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Foreign key to the parent InitiativePeriod
        /// </summary>
        public int PeriodId { get; set; }
        
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod Period { get; set; } = null!;
        
        /// <summary>
        /// Timestamp when this form was uploaded
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
