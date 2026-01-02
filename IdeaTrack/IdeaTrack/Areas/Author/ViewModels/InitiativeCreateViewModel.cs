namespace IdeaTrack.Areas.Author.ViewModels
{
    using IdeaTrack.Models;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class InitiativeCreateViewModel
    {
        public Initiative Initiative { get; set; } = new();
        public SelectList? AcademicYears { get; set; }
        public SelectList? Categories { get; set; }
        public SelectList? Departments { get; set; }
    }
}
