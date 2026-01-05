using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class InitiativeFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; } // .pdf, .docx...
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public int InitiativeId { get; set; }
        // Bo sung thuoc tinh dieu huong (Navigation Property)
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; }
    }
}
