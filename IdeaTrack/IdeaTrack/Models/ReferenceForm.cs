using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// Evaluation Templates huong dan - Cac file tai lieu mau cho tung dot sang kien
    /// File duoc luu tren Supabase Storage, database chi luu URL
    /// </summary>
    public class ReferenceForm
    {
        public int Id { get; set; }
        public string FormName { get; set; } = ""; // Name hien thi
        public string FileUrl { get; set; } = ""; // URL tu Supabase Storage
        public string? FileName { get; set; } // Name file goc
        public string? FileType { get; set; } // docx, pdf, xlsx
        public string? Description { get; set; }
        
        public int PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual InitiativePeriod Period { get; set; } = null!;
        
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
