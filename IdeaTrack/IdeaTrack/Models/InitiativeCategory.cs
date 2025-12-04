using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    public class InitiativeCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
