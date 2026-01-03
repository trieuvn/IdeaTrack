using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Biểu mẫu hướng dẫn - Các file tài liệu mẫu cho từng đợt sáng kiến
    /// File được lưu trên Supabase Storage, database chỉ lưu URL
    /// </summary>
    public class ReferenceForm
    {
        public int Id { get; set; }
        public string FormName { get; set; } = ""; // Tên hiển thị
        public string FileUrl { get; set; } = ""; // URL từ Supabase Storage
        public string? FileName { get; set; } // Tên file gốc
        public string? FileType { get; set; } // docx, pdf, xlsx
        public string? Description { get; set; }
        
        public int PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod Period { get; set; } = null!;
        
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
