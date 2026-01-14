namespace IdeaTrack.Areas.SciTech.Models
{
    public class InitiativeListVM
    {
        public int Id { get; set; }
        public string InitiativeCode { get; set; }
        public string Title { get; set; }
        public string ProposerName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string Status { get; set; }
        public string PeriodName { get; set; }
        public string AcademicYear { get; set; }
        public int MemberCount { get; set; }
    }
}
