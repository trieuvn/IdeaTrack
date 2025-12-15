using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdeaTrack.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }

        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<Initiative> Initiatives { get; set; }
    }
}
