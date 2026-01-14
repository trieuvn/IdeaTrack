using System.Collections.Generic;

namespace IdeaTrack.Areas.SciTech.Models
{
    public class InitiativeResultVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string InitiativeCode { get; set; }
        public string ProposerName { get; set; }
        public string Status { get; set; }

        public decimal AverageScore { get; set; }
        public double ConsensusRate { get; set; }
        public int CompletedCount { get; set; }
        public int TotalMembers { get; set; }
        
        public string FinalStatus { get; set; }
        public string Rank { get; set; }

        public string ConsolidatedStrengths { get; set; }
        public string ConsolidatedLimitations { get; set; }
        public string ConsolidatedRecommendations { get; set; }
        public bool HidePersonalInfo { get; set; }

        public List<MemberScoreVM> MemberScores { get; set; } = new List<MemberScoreVM>();
        public List<CoAuthorVM> CoAuthors { get; set; } = new List<CoAuthorVM>();
    }

    public class MemberScoreVM
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string Role { get; set; }
        public bool IsCompleted { get; set; }
        public decimal TotalScore { get; set; }
        public Dictionary<string, decimal> Scores { get; set; } = new Dictionary<string, decimal>();

        public string Strengths { get; set; }
        public string Limitations { get; set; }
        public string Recommendations { get; set; }
    }
}
