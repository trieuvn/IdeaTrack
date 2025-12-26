using Microsoft.AspNetCore.Identity;

namespace IdeaTrack.Models
{
    public class ApplicationRole : IdentityRole<int> 
    {
        public string Description { get; set; }
    }
}
