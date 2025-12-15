namespace IdeaTrack.Models
{
    public class CommitteeAssignment
    {
        public int Id { get; set; }

        public int InitiativeId { get; set; }
        public Initiative Initiative { get; set; } = null!;

        public Guid ReviewerId { get; set; }
        public ApplicationUser Reviewer { get; set; } = null!;
<<<<<<< HEAD
        public string Role { get; set; } //Chairman,Secretary,Reviewer
=======

>>>>>>> d1c795b103ceec0ecfab0e936d47e204b79df8d4
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending – Scoring – Completed
    }

}
