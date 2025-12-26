using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class RevisionRequest
    {
        public int Id { get; set; }
        public int InitiativeId { get; set; }
        public int RequesterId { get; set; } // Admin hoặc Chuyên viên KHCN

        public string RequestContent { get; set; } // Nội dung cần sửa
        public DateTime RequestedDate { get; set; } = DateTime.Now;
        public DateTime? Deadline { get; set; }
        public bool IsResolved { get; set; } = false;
        public string Status { get; set; } = "Open"; 

        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; }
        [ForeignKey("RequesterId")]
        public virtual ApplicationUser Requester { get; set; }
    }
}
