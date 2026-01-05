using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class FinalResult
    {
        public int Id { get; set; }
        public int InitiativeId { get; set; }

        public decimal AverageScore { get; set; } // Diem TB tu cac thanh vien
        public decimal? FinalScore { get; set; }  // Diem chot cua Chu tich
        public string? Rank { get; set; }         // Xuat sac, Tot, Kha...
        public string? ChairmanDecision { get; set; } // Approved/Rejected

        public DateTime DecisionDate { get; set; } = DateTime.Now;
        public int ChairmanId { get; set; }

        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; }
        [ForeignKey("ChairmanId")]
        public virtual ApplicationUser Chairman { get; set; }
    }
}
