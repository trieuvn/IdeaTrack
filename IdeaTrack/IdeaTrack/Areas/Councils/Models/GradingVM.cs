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
        
        // Danh sách file đính kèm để tải về
        public List<InitiativeFile> Files { get; set; } = new();

        // --- PHẦN 2: FORM CHẤM ĐIỂM (INPUT) ---
        // Danh sách các dòng chấm điểm
        public List<GradingItem> CriteriaItems { get; set; } = new();

        public string? GeneralComment { get; set; } // Nhận xét chung

        // Giá trị từ form: "SaveDraft" | "Submit"
        public string SubmitAction { get; set; } = "SaveDraft";
    }

    public class GradingItem
    {
        // Thông tin tiêu chí (Lấy từ Template)
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; }
        public string Description { get; set; }
        public decimal MaxScore { get; set; }

        // Điểm người dùng nhập vào
        public decimal ScoreGiven { get; set; }
        public string? Note { get; set; } // Nhận xét từng phần
    }
}