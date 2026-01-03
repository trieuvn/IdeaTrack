namespace IdeaTrack.Areas.Author.ViewModels
{
    using IdeaTrack.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class InitiativeCreateViewModel
    {
        public Initiative Initiative { get; set; } = new();
        public SelectList? Categories { get; set; }
        public SelectList? Departments { get; set; }
        
        // Active period info (for display)
        public int? ActivePeriodId { get; set; }
        
        // File upload support
        public List<IFormFile>? UploadedFiles { get; set; }
    }
}
