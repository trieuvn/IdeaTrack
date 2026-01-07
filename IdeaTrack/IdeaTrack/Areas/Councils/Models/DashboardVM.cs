using IdeaTrack.Models;

namespace IdeaTrack.Areas.Councils.Models
{
    public class DashboardVM
    {
        // User Info for Header
        public string UserFullName { get; set; } = "Expert";
        
        // KPI Cards
        public int TotalAssigned { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int ProgressPercentage { get; set; } // Logic: (Completed/Total)*100

        // Up Next & Recent Activity lists
        public List<DashboardItem> Assignments { get; set; } = new();
        public List<DashboardItem> RecentActivityList { get; set; } = new();
    }


    public class DashboardItem
    {
        public int AssignmentId { get; set; }
        public string InitiativeCode { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public DateTime Timestamp { get; set; } // Assigned date or Score date
        public DateTime? DueDate { get; set; }  // For deadline calculation
        public decimal? Score { get; set; }
        public AssignmentStatus Status { get; set; } // Add cai nay de check trang thai nut bam
        public string Description { get; set; } // For Recent Activity

        // Time ago display logic
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

        // Business logic: Deadline warning
        public bool IsDueSoon => Status != AssignmentStatus.Completed 
                                 && DueDate.HasValue 
                                 && (DueDate.Value - DateTime.Now).TotalDays <= 7;
    }
}