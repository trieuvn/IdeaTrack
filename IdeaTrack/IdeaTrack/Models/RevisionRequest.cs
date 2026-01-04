using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class RevisionRequest
    {
        public int Id { get; set; }
        public int InitiativeId { get; set; }
        public int RequesterId { get; set; } // Admin hoac Chuyen vien KHCN

        public string RequestContent { get; set; } // Noi dung can sua
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
