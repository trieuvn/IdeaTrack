using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class SystemAuditLog
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } // VD: "Approve", "Update Score"
        public string TargetTable { get; set; }
        public int TargetId { get; set; }
        public string? Details { get; set; } // Save JSON cac thay doi
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string IpAddress { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
