using IdeaTrack.Models;

public class ApprovalRecord
{
    public int Id { get; set; }

    public int InitiativeId { get; set; }
    public Initiative Initiative { get; set; } = null!;

    public Guid ApproverId { get; set; }
    public ApplicationUser Approver { get; set; } = null!;

    // Level: Khoa – Hội đồng – BGH
    public string Level { get; set; } = null!;
    public string Status { get; set; } = null!; // Approved – Rejected – NeedSupplement
    public string Note { get; set; } = "";
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
}

