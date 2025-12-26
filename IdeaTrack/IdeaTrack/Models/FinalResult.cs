using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class FinalResult
    {
        public int Id { get; set; }
        public int InitiativeId { get; set; }

        public decimal AverageScore { get; set; } // Điểm TB từ các thành viên
        public decimal? FinalScore { get; set; }  // Điểm chốt của Chủ tịch
        public string? Rank { get; set; }         // Xuất sắc, Tốt, Khá...
        public string? ChairmanDecision { get; set; } // Approved/Rejected

        public DateTime DecisionDate { get; set; } = DateTime.Now;
        public int ChairmanId { get; set; }

        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; }
        [ForeignKey("ChairmanId")]
        public virtual ApplicationUser Chairman { get; set; }
    }
}
