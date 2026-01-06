using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// INITIATIVE FILE - Tệp đính kèm Sáng kiến
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Stores file attachments uploaded by authors for their initiatives.
    /// Actual files are stored on server storage; this table stores metadata.
    /// 
    /// BUSINESS RULES:
    /// 1. Each initiative can have multiple attachments (1:N)
    /// 2. Supported types: PDF, DOCX, XLSX, PPTX, images
    /// 3. Files must be under configured size limit (default 10MB)
    /// 4. Files are preserved even when initiative is rejected/archived
    /// 
    /// RELATIONSHIPS:
    /// - Many Files belong to 1 Initiative (N:1)
    /// 
    /// FILE STORAGE:
    /// FilePath contains relative path to file on server.
    /// Full URL = BaseStorageUrl + FilePath
    /// </summary>
    public class InitiativeFile
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Original filename as uploaded by user
        /// </summary>
        public string FileName { get; set; } = "";
        
        /// <summary>
        /// Relative path to file in storage (e.g., "/uploads/2025/01/file.pdf")
        /// </summary>
        public string FilePath { get; set; } = "";
        
        /// <summary>
        /// File extension/type (e.g., ".pdf", ".docx", ".xlsx")
        /// </summary>
        public string FileType { get; set; } = "";
        
        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Timestamp when file was uploaded
        /// </summary>
        public DateTime UploadDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Foreign key to the parent Initiative
        /// </summary>
        public int InitiativeId { get; set; }
        
        // =====================
        // Navigation Property
        // =====================
        
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; } = null!;
    }
}
