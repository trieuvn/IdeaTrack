namespace IdeaTrack.Models
{
    public class CommitteeAssignment
    {
        public int Id { get; set; }

        public int InitiativeId { get; set; }
        public Initiative Initiative { get; set; } = null!;

        public Guid ReviewerId { get; set; }
        public ApplicationUser Reviewer { get; set; } = null!;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending – Scoring – Completed
    }

}
