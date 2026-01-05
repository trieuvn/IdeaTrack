using IdeaTrack.Models;

namespace IdeaTrack.Areas.Councils.Models
{
    public class GradingVM
    {
        // --- PHAN 1: THONG TIN HIEN THI (READ-ONLY) ---
        public int AssignmentId { get; set; }
        public string InitiativeTitle { get; set; } = string.Empty;
        public string InitiativeCode { get; set; } = string.Empty;
        public string ProposerName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        
        // Information vong cham
        public int RoundNumber { get; set; } = 1;
        public bool IsLocked { get; set; } = false; // True if already submitted
        
        // Danh sach file dinh kem de tai ve
        public List<InitiativeFile> Files { get; set; } = new();

        // --- PHAN 2: FORM CHAM DIEM (INPUT) ---
        // Danh sach cac dong cham diem
        public List<GradingItem> CriteriaItems { get; set; } = new();
        public decimal MaxTotalScore => CriteriaItems.Sum(c => c.MaxScore);

        public string? GeneralComment { get; set; } // Nhan xet chung

        // Gia tri tu form: "SaveDraft" | "Submit"
        public string SubmitAction { get; set; } = "SaveDraft";
        
        // Co vong truoc do hay khong
        public bool HasPreviousRounds { get; set; } = false;
    }

    public class GradingItem
    {
        // Information tieu chi (Lay tu Template)
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal MaxScore { get; set; }

        // Diem nguoi dung nhap vao
        public decimal ScoreGiven { get; set; }
        public string? Note { get; set; } // Nhan xet tung phan
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