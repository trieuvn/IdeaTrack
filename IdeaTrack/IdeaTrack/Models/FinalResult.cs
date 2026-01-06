using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    /// <summary>
    /// ======================================================================
    /// FINAL RESULT - Kết quả Cuối cùng
    /// ======================================================================
    /// 
    /// PURPOSE:
    /// Stores the final evaluation result after all council members have scored.
    /// The Chairman makes the final decision based on average scores.
    /// 
    /// BUSINESS RULES:
    /// 1. Created after all members complete their evaluations
    /// 2. AverageScore = Average of all member scores
    /// 3. Chairman can adjust FinalScore if needed
    /// 4. Rank is assigned based on score thresholds:
    ///    - "Xuất sắc" (Excellent): >= 90%
    ///    - "Tốt" (Good): >= 80%
    ///    - "Khá" (Fair): >= 70%
    ///    - "Đạt" (Pass): >= 50%
    /// 5. ChairmanDecision: "Approved" or "Rejected"
    /// 
    /// RELATIONSHIPS:
    /// - 1 FinalResult per Initiative (1:1)
    /// - 1 FinalResult references 1 Chairman/User (N:1)
    /// </summary>
    public class FinalResult
    {
        /// <summary>
        /// Primary key - Auto-incremented ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Foreign key to the evaluated Initiative
        /// </summary>
        public int InitiativeId { get; set; }

        /// <summary>
        /// Calculated average score from all council members.
        /// Formula: Sum(MemberScores) / NumberOfMembers
        /// </summary>
        public decimal AverageScore { get; set; }
        
        /// <summary>
        /// Final score after Chairman's adjustment (if any).
        /// If null, AverageScore is used for ranking.
        /// </summary>
        public decimal? FinalScore { get; set; }
        
        /// <summary>
        /// Achievement rank based on score thresholds.
        /// Values: "Xuất sắc", "Tốt", "Khá", "Đạt", "Không đạt"
        /// </summary>
        public string? Rank { get; set; }
        
        /// <summary>
        /// Chairman's final decision: "Approved" or "Rejected"
        /// </summary>
        public string? ChairmanDecision { get; set; }

        /// <summary>
        /// Date and time when the decision was made
        /// </summary>
        public DateTime DecisionDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Foreign key to the Chairman who made the decision
        /// </summary>
        public int ChairmanId { get; set; }

        // =====================
        // Navigation Properties
        // =====================
        
        [ForeignKey("InitiativeId")]
        public virtual Initiative Initiative { get; set; } = null!;
        
        [ForeignKey("ChairmanId")]
        public virtual ApplicationUser Chairman { get; set; } = null!;
    }
}
