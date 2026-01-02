using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaTrack.Models
{
    public class EvaluationCriteria
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string CriteriaName { get; set; }
        public string? Description { get; set; }
        public decimal MaxScore { get; set; } 
        public int SortOrder { get; set; }
        [ForeignKey("TemplateId")]
        public virtual EvaluationTemplate Template { get; set; }
    }
}
