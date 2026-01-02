namespace IdeaTrack.Models
{
    public class EvaluationTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public TemplateType Type { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<EvaluationCriteria> CriteriaList { get; set; } = new List<EvaluationCriteria>();
    }
}
