using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.ViewModels
{
    public class ExternalRegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; }

        public string ReturnUrl { get; set; }
    }
}
