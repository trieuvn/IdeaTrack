using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    public class AcademicYear
    {
        public int Id { get; set; }
        public string Name { get; set; } = ""; // 2024-2025
        public bool IsCurrent { get; set; }

        public ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
        public ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    }
}
