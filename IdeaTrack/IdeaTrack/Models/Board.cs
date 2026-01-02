namespace IdeaTrack.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string BoardName { get; set; }
        public int FiscalYear { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
        public virtual ICollection<InitiativeAssignment> Assignments { get; set; } = new List<InitiativeAssignment>();
    }
}
