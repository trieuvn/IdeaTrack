namespace IdeaTrack.Areas.SciTech
{
    public class EvaluationCriteriaDto
    {
        public int Id { get; set; }
        public string CriteriaName { get; set; }
        public string? Description { get; set; }
        public decimal MaxScore { get; set; }
    }

}
