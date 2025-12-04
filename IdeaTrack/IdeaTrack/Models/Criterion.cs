using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    public class Criterion
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int MaxScore { get; set; }

        public int AcademicYearId { get; set; }
        public AcademicYear AcademicYear { get; set; } = null!;
    }
}
