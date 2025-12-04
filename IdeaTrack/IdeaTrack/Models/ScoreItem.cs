using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    public class ScoreItem
    {
        public int Id { get; set; }

        public int CriterionId { get; set; }
        public Criterion Criterion { get; set; } = null!;

        public int InitiativeId { get; set; }
        public Initiative Initiative { get; set; } = null!;

        public Guid ReviewerId { get; set; }
        public ApplicationUser Reviewer { get; set; } = null!;

        public int Score { get; set; }
        public string Comment { get; set; } = "";
    }

}
