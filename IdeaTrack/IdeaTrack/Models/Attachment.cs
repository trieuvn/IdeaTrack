using IdeaTrack.Models;

public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int InitiativeId { get; set; }
    public Initiative Initiative { get; set; } = null!;

    public Guid UploadedById { get; set; }
    public ApplicationUser UploadedBy { get; set; } = null!;
}
