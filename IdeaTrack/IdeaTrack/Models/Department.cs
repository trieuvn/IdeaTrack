using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
