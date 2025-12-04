using IdeaTrack.Models;
using System.ComponentModel.DataAnnotations;

public class Initiative
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Trạng thái
    public string Status { get; set; } = "Submitted";
    // Submitted – Reviewing – NeedSupplement – Scoring – Approved – Rejected

    // Người tạo
    public Guid CreatorId { get; set; }
    public ApplicationUser Creator { get; set; } = null!;

    // Khoa
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    // Danh mục
    public int CategoryId { get; set; }
    public InitiativeCategory Category { get; set; } = null!;
    public List<ActivityLog> ActivityLogs { get; set; } = new();

    // Năm học
    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<ApprovalRecord> ApprovalRecords { get; set; } = new List<ApprovalRecord>();
    public ICollection<ScoreItem> ScoreItems { get; set; } = new List<ScoreItem>();
    public ICollection<CommitteeAssignment> Assignments { get; set; } = new List<CommitteeAssignment>();
}