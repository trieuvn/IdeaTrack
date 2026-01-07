using IdeaTrack.Models;

namespace IdeaTrack.Areas.Councils.Models
{
    public class GradingVM
    {
        // --- PART 1: DISPLAY INFORMATION (READ-ONLY) ---
        public int AssignmentId { get; set; }
        public string InitiativeTitle { get; set; } = string.Empty;
        public string InitiativeCode { get; set; } = string.Empty;
        public string ProposerName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        
        // Evaluation round information
        public int RoundNumber { get; set; } = 1;
        public bool IsLocked { get; set; } = false; // True if already submitted
        
        // Attached files for download
        public List<InitiativeFile> Files { get; set; } = new();

        // --- PART 2: GRADING FORM (INPUT) ---
        // List of grading criteria
        public List<GradingItem> CriteriaItems { get; set; } = new();
        public decimal MaxTotalScore => CriteriaItems.Sum(c => c.MaxScore);

        public string? GeneralComment { get; set; } // Overall comment
        
        // Detailed Feedback
        public string? Strengths { get; set; }
        public string? Limitations { get; set; }
        public string? Recommendations { get; set; }

        // Form action: "SaveDraft" | "Submit"
        public string SubmitAction { get; set; } = "SaveDraft";
        
        // Whether there are previous rounds
        public bool HasPreviousRounds { get; set; } = false;
    }

    public class GradingItem
    {
        // Criteria information (from Template)
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal MaxScore { get; set; }

        // User-entered score
        public decimal ScoreGiven { get; set; }
        public string? Note { get; set; } // Per-criterion notes
    }
    
    /// <summary>
    /// ViewModel for viewing previous round history
    /// </summary>
    public class RoundHistoryVM
    {
        public int InitiativeId { get; set; }
        public string InitiativeTitle { get; set; } = "";
        public string InitiativeCode { get; set; } = "";
        public int CurrentRound { get; set; }
        
        public List<RoundHistoryItem> Rounds { get; set; } = new();
    }
    
    /// <summary>
    /// A single round in the history
    /// </summary>
    public class RoundHistoryItem
    {
        public int RoundNumber { get; set; }
        public int AssignmentId { get; set; }
        public string MemberName { get; set; } = "";
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal TotalScore { get; set; }
        public AssignmentStatus Status { get; set; }
        public string? Comment { get; set; }
        public List<GradingItem> Details { get; set; } = new();
    }
}