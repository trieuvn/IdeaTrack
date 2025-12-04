using IdeaTrack.Models;

public class ActivityLog
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int? InitiativeId { get; set; }          // nullable = optional
    public Initiative? Initiative { get; set; }     // optional navigation

    public string Action { get; set; } = null!;
    public string? Detail { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
