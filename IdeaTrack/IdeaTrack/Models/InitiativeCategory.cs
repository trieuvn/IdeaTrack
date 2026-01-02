namespace IdeaTrack.Models
{
    public class InitiativeCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } // VD: Cải tiến kỹ thuật, Phần mềm, Quản lý
        public string? Description { get; set; }
        public virtual ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
