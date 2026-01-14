namespace IdeaTrack.Areas.Author.ViewModels
{
    using IdeaTrack.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class InitiativeCreateViewModel
    {
        public Initiative Initiative { get; set; } = new();
        
        // === DEPENDENT DROPDOWN SUPPORT ===
        // Step 1: User selects Academic Year (only years with open periods)
        public SelectList? AcademicYears { get; set; }
        public int? SelectedAcademicYearId { get; set; }
        
        // Step 2: Categories filtered by open periods in selected year
        public SelectList? Categories { get; set; }
        
        // Department auto-assigned from current user (no dropdown)
        public int? UserDepartmentId { get; set; }
        public string? UserDepartmentName { get; set; }
        
        // Active period info (for display)
        public int? ActivePeriodId { get; set; }
        
        // File upload support
        public List<IFormFile>? ProjectFiles { get; set; }
        
        // Existing files (for Edit view)
        public List<InitiativeFile>? ExistingFiles { get; set; }
        
        // === CO-AUTHORS SUPPORT ===
        // Selected co-author IDs (for form submission)
        public List<int>? CoAuthorIds { get; set; }
        
        // Existing co-authors (for Edit view display)
        public List<InitiativeAuthorship>? ExistingCoAuthors { get; set; }
        
        // Flag to check if user is owner (for edit permissions)
        public bool IsOwner { get; set; } = true;
    }
}
