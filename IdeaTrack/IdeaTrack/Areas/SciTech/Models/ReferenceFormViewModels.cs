using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Areas.SciTech.Models
{
    /// <summary>
    /// ViewModel for displaying reference form information
    /// </summary>
    public class ReferenceFormViewModel
    {
        public int Id { get; set; }
        public string FormName { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public string? Description { get; set; }
        public int PeriodId { get; set; }
        public string? PeriodName { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a reference form
    /// </summary>
    public class ReferenceFormCreateVM
    {
        [Required(ErrorMessage = "Please enter form name")]
        [MaxLength(200, ErrorMessage = "Form name cannot exceed 200 characters")]
        [Display(Name = "Name bieu mau")]
        public string FormName { get; set; } = "";

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select initiative period")]
        [Display(Name = "Dot sang kien")]
        public int PeriodId { get; set; }

        [Required(ErrorMessage = "Please select file")]
        [Display(Name = "File bieu mau")]
        public IFormFile? File { get; set; }

        // Dropdowns
        public List<SelectListItem> Periods { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for editing a reference form (without file replacement)
    /// </summary>
    public class ReferenceFormEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter form name")]
        [MaxLength(200, ErrorMessage = "Form name cannot exceed 200 characters")]
        [Display(Name = "Name bieu mau")]
        public string FormName { get; set; } = "";

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select initiative period")]
        [Display(Name = "Dot sang kien")]
        public int PeriodId { get; set; }

        // Display only
        public string FileUrl { get; set; } = "";
        public string? FileName { get; set; }

        // Optional: New file to replace the old one
        [Display(Name = "File moi (de trong neu khong doi)")]
        public IFormFile? NewFile { get; set; }

        // Dropdowns
        public List<SelectListItem> Periods { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for listing reference forms with filtering
    /// </summary>
    public class ReferenceFormListVM
    {
        public int? SelectedPeriodId { get; set; }
        public string? SelectedPeriodName { get; set; }
        public List<ReferenceFormViewModel> Items { get; set; } = new();
        public List<SelectListItem> Periods { get; set; } = new();
    }
}
