using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class EvaluationDetail
    {
        public long Id { get; set; }
        public int AssignmentId { get; set; }
        public int CriteriaId { get; set; }
        public decimal ScoreGiven { get; set; }
        public string? Note { get; set; }
        [ForeignKey("AssignmentId")]
        public virtual InitiativeAssignment Assignment { get; set; }
        [ForeignKey("CriteriaId")]
        public virtual EvaluationCriteria Criteria { get; set; }
    }
}
