using IdeaTrack.Models;

namespace IdeaTrack.Areas.Councils.Models
{
    public class GradingVM
    {
        // --- PHẦN 1: THÔNG TIN HIỂN THỊ (READ-ONLY) ---
        public int AssignmentId { get; set; }
        public string InitiativeTitle { get; set; } = string.Empty;
        public string InitiativeCode { get; set; } = string.Empty;
        public string ProposerName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        
        // Thông tin vòng chấm
        public int RoundNumber { get; set; } = 1;
        public bool IsLocked { get; set; } = false; // True if already submitted
        
        // Danh sách file đính kèm để tải về
        public List<InitiativeFile> Files { get; set; } = new();

        // --- PHẦN 2: FORM CHẤM ĐIỂM (INPUT) ---
        // Danh sách các dòng chấm điểm
        public List<GradingItem> CriteriaItems { get; set; } = new();

        public string? GeneralComment { get; set; } // Nhận xét chung

        // Giá trị từ form: "SaveDraft" | "Submit"
        public string SubmitAction { get; set; } = "SaveDraft";
        
        // Có vòng trước đó hay không
        public bool HasPreviousRounds { get; set; } = false;
    }

    public class GradingItem
    {
        // Thông tin tiêu chí (Lấy từ Template)
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal MaxScore { get; set; }

        // Điểm người dùng nhập vào
        public decimal ScoreGiven { get; set; }
        public string? Note { get; set; } // Nhận xét từng phần
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