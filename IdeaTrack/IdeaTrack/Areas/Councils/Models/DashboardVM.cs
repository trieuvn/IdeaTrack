using IdeaTrack.Models;

namespace IdeaTrack.Areas.Councils.Models
{
    public class DashboardVM
    {
        // KPI Cards
        public int TotalAssigned { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int ProgressPercentage { get; set; } // Logic: (Completed/Total)*100

        // Danh sách Up Next & Recent Activity
        public List<DashboardItem> UpNextList { get; set; } = new();
        public List<DashboardItem> RecentActivityList { get; set; } = new();
    }

    public class DashboardItem
    {
        public int AssignmentId { get; set; }
        public string InitiativeCode { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public DateTime Timestamp { get; set; } // Ngày giao hoặc Ngày chấm
        public DateTime? DueDate { get; set; }  // Thêm cái này để tính hạn chót
        public decimal? Score { get; set; }
        public AssignmentStatus Status { get; set; } // Thêm cái này để check trạng thái nút bấm

        // Logic hiển thị thời gian "Ago"
        public string TimeAgo 
        {
            get 
            {
                var span = DateTime.Now - Timestamp;
                if (span.TotalHours < 1) return $"{Math.Max(1, span.Minutes)} mins ago";
                if (span.TotalHours < 24) return $"{span.Hours} hours ago";
                if (span.TotalDays < 30) return $"{span.Days} days ago";
                return Timestamp.ToString("dd/MM/yyyy");
            }
        }

        // Logic nghiệp vụ: Cảnh báo hạn chót (Yêu cầu trong ảnh)
        public bool IsDueSoon => Status != AssignmentStatus.Completed 
                                 && DueDate.HasValue 
                                 && (DueDate.Value - DateTime.Now).TotalDays <= 7;
    }
}